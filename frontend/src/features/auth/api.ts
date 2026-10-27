import { apiClient } from '@/lib/api-client'
import type { ChangePasswordRequest, CurrentUser, LoginRequest, LoginResponse } from './types'

/** One function per endpoint of AuthController. Pages call these through hooks, never apiClient directly. */
export const authApi = {
  login: (body: LoginRequest) =>
    apiClient.post<LoginResponse>('/auth/login', body).then((r) => r.data),

  me: () => apiClient.get<CurrentUser>('/auth/me').then((r) => r.data),

  changePassword: (body: ChangePasswordRequest) =>
    apiClient.post<LoginResponse>('/auth/change-password', body).then((r) => r.data),
}

/** Query keys of this feature, in one place so invalidation never misses one. */
export const authKeys = {
  me: ['auth', 'me'] as const,
}
