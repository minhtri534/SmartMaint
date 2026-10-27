import type { Role } from '@/shared/types/role'

/** Mirrors backend Modules/Auth/Dtos. Keep field names identical (camelCase JSON). */
export interface CurrentUser {
  id: number
  email: string
  fullName: string
  role: Role
  mustChangePassword: boolean
  lastLoginAt: string | null
}

export interface LoginRequest {
  email: string
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
