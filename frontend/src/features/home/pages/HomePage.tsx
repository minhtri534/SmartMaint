import { AdminHomePage } from '@/features/accounts'
import { useAuth } from '@/features/auth'
import { CustomerHome, ManagerHome, StaffHome, TechnicianHome } from './RoleHomes'

/** "/" is every role's Home (EMP-01, OFF-01, TEC-01, MGR-01, ADM-01), the page each role lands on after signing in. */
export function HomePage() {
  const { user } = useAuth()
  if (!user) return null
  switch (user.role) {
    case 'customer':
      return <CustomerHome />
    case 'staff':
      return <StaffHome />
    case 'technician':
      return <TechnicianHome />
    case 'manager':
      return <ManagerHome />
    default:
      return <AdminHomePage />
  }
}
