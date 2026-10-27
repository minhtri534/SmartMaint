import axios from 'axios'
import { toApiError } from './api-error'
import { env } from './env'
import { tokenStorage } from './token-storage'

/** Fired when the API rejects the token (expired / invalid). AuthProvider listens and signs the user out. */
export const SESSION_EXPIRED_EVENT = 'smartmaint:session-expired'

/**
 * The single HTTP client. Feature code never imports axios directly: it calls functions in
 * features/<feature>/api.ts, which use this instance. Errors always reject as ApiError.
 */
export const apiClient = axios.create({
  baseURL: `${env.apiBaseUrl}/api`,
  headers: { 'Content-Type': 'application/json' },
  timeout: 30_000,
})

apiClient.interceptors.request.use((config) => {
  const token = tokenStorage.get()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  (error: unknown) => {
    const apiError = toApiError(error)
    if (apiError.status === 401 && apiError.code !== 'auth.invalid_credentials') {
      window.dispatchEvent(new CustomEvent(SESSION_EXPIRED_EVENT, { detail: apiError.code }))
    }
    return Promise.reject(apiError)
  },
)
