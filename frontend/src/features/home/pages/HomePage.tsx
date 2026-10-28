import { Navigate } from 'react-router'
import { HOME_BY_ROLE } from '@/app/navigation'
import { paths } from '@/app/paths'
import { useAuth } from '@/features/auth'
import { Card, PageHeader } from '@/shared/ui'

/**
 * "/" sends each role to its main screen (HOME_BY_ROLE). Only the Administrator stays here for now.
 * Card H4 (Hùng) replaces this with one Home per role + the F6 KPIs.
 */
export function HomePage() {
  const { user } = useAuth()
  if (!user) return null
  if (HOME_BY_ROLE[user.role] !== paths.home)
    return <Navigate to={HOME_BY_ROLE[user.role]} replace />

  return (
    <div className="flex flex-col gap-6">
      <PageHeader title="Administration" description="Accounts, buildings and system settings." />
      <Card className="text-muted">
        User management (ADM-02) and the building catalogue (F1) are not built yet.
      </Card>
    </div>
  )
}
