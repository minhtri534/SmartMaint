import { Navigate } from 'react-router'
import { HOME_BY_ROLE } from '@/app/navigation'
import { paths } from '@/app/paths'
import { AdminHomePage } from '@/features/accounts'
import { useAuth } from '@/features/auth'

/**
 * "/" sends each role to its main screen (HOME_BY_ROLE). The Administrator stays here: ADM-01.
 * F6 (Hùng) later gives every role its own Home with KPIs.
 */
export function HomePage() {
  const { user } = useAuth()
  if (!user) return null
  if (HOME_BY_ROLE[user.role] !== paths.home)
    return <Navigate to={HOME_BY_ROLE[user.role]} replace />
  return <AdminHomePage />
}
