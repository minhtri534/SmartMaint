import type { Role } from '@/shared/types/role'

/** Mirrors backend Modules/Auth/Dtos. Keep field names identical (camelCase JSON). */
export interface CurrentUser {
  id: number
  username: string
  /** Signs in (meeting note 18/11). NOT IN ERD v5. */
  email: string | null
  fullName: string
  role: Role
  mustChangePassword: boolean
  lastLoginAt: string | null
}

/** Meeting note 18/11: login = the e-mail address or the phone number. */
export interface LoginRequest {
  login: string
  password: string
}

export interface LoginResponse {
  accessToken: string
  expiresAt: string
  user: CurrentUser
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}
