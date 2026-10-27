import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { SESSION_EXPIRED_EVENT } from '@/lib/api-client'
import { tokenStorage } from '@/lib/token-storage'
import { authApi, authKeys } from '../api'
import { AuthContext, type AuthContextValue, type SignOutReason } from '../hooks'
import type { LoginResponse } from '../types'

/** Owns the session: token in storage, current user in the query cache (key authKeys.me). */
export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()
  const [hasToken, setHasToken] = useState(() => tokenStorage.get() !== null)
  const [signOutReason, setSignOutReason] = useState<SignOutReason | null>(null)

  const meQuery = useQuery({ queryKey: authKeys.me, queryFn: authApi.me, enabled: hasToken })

  const acceptSession = useCallback(
    (session: LoginResponse) => {
      tokenStorage.set(session.accessToken)
      queryClient.setQueryData(authKeys.me, session.user)
      setSignOutReason(null)
      setHasToken(true)
    },
    [queryClient],
  )

  const signOut = useCallback(
    (reason: SignOutReason = 'manual') => {
      tokenStorage.clear()
      queryClient.clear()
      setSignOutReason(reason)
      setHasToken(false)
    },
    [queryClient],
  )

  useEffect(() => {
    const onExpired = () => signOut('session_expired')
    window.addEventListener(SESSION_EXPIRED_EVENT, onExpired)
    return () => window.removeEventListener(SESSION_EXPIRED_EVENT, onExpired)
  }, [signOut])

  const value = useMemo<AuthContextValue>(
    () => ({
      user: hasToken ? (meQuery.data ?? null) : null,
      isLoading: hasToken && meQuery.isPending,
      signOutReason,
      acceptSession,
      signOut,
    }),
    [hasToken, meQuery.data, meQuery.isPending, signOutReason, acceptSession, signOut],
  )

  return <AuthContext value={value}>{children}</AuthContext>
}
