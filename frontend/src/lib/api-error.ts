import { isAxiosError } from 'axios'

/** Shape of every error the API returns (RFC 7807 ProblemDetails + "code"). */
export interface ProblemDetails {
  status: number
  title?: string
  detail?: string
  code: string
  errors?: Record<string, string[]>
}

/**
 * Normalised error for UI code. Switch on `code` ("auth.invalid_credentials"...), never on message text.
 * `fieldErrors` maps camelCase field names to messages, ready for react-hook-form setError.
 */
export class ApiError extends Error {
  readonly status: number
  readonly code: string
  readonly fieldErrors: Record<string, string[]>

  constructor(
    status: number,
    code: string,
    message: string,
    fieldErrors: Record<string, string[]> = {},
  ) {
    super(message)
    this.status = status
    this.code = code
    this.fieldErrors = fieldErrors
  }
}

export function toApiError(error: unknown): ApiError {
  if (error instanceof ApiError) return error
  if (isAxiosError<ProblemDetails>(error)) {
    const data = error.response?.data
    if (data?.code) {
      return new ApiError(
        data.status,
        data.code,
        data.detail ?? data.title ?? 'Request failed',
        data.errors,
      )
    }
    if (!error.response) {
      return new ApiError(
        0,
        'network.unreachable',
        'Cannot reach the server. Check your connection.',
      )
    }
    return new ApiError(error.response.status, 'http.unknown', error.message)
  }
  return new ApiError(0, 'client.unknown', 'Something went wrong.')
}
