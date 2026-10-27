import { useMutation } from '@tanstack/react-query'
import { createContext, useContext } from 'react'
import { authApi } from './api'
import type { LoginResponse, CurrentUser } from './types'

export type SignOutReason = 'manual' | 'session_expired'

export interface AuthContextValue {
  user: CurrentUser | null
  isLoading: boolean
  signOutReason: SignOutReason | null
  /** Store the token + user returned by login / change-password. */
  acceptSession: (session: LoginResponse) => void
  signOut: (reason?: SignOutReason) => void
}

/** Filled by <AuthProvider>; read it with useAuth(). */
export const AuthContext = createContext<AuthContextValue | null>(null)

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth must be used inside <AuthProvider>')
  return context
}

export function useLogin() {
  const { acceptSession } = useAuth()
  return useMutation({ mutationFn: authApi.login, onSuccess: acceptSession })
}

export function useChangePassword() {
  const { acceptSession } = useAuth()
  return useMutation({ mutationFn: authApi.changePassword, onSuccess: acceptSession })
}
