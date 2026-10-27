import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router'
import { paths } from '@/app/paths'
import type { Role } from '@/shared/types/role'
import { FullPageSpinner } from '@/shared/ui'
import { useAuth } from '../hooks'

interface RequireAuthProps {
  /** Omit = any signed-in role. The API enforces the same rule with [AuthorizeRoles]. */
  roles?: readonly Role[]
  children: ReactNode
}

/** Route guard. Wrap a route element; never check roles by hand inside pages. */
export function RequireAuth({ roles, children }: RequireAuthProps) {
  const { user, isLoading } = useAuth()
  const location = useLocation()

  if (isLoading) return <FullPageSpinner />
  if (!user) return <Navigate to={paths.login} replace state={{ from: location }} />
  if (user.mustChangePassword && location.pathname !== paths.changePassword) {
    return <Navigate to={paths.changePassword} replace />
  }
  if (roles && !roles.includes(user.role)) return <Navigate to={paths.forbidden} replace />

  return children
}
