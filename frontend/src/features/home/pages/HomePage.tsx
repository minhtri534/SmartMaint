import { useAuth } from '@/features/auth'
import { ROLE_LABELS } from '@/shared/types/role'
import { Card } from '@/shared/ui'

/** Placeholder Home. Card H4 (Hùng) replaces this with one Home per role + KPI tiles for F6. */
export function HomePage() {
  const { user } = useAuth()
  if (!user) return null

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold">Welcome, {user.fullName}</h1>
        <p className="text-muted mt-1 text-sm">Signed in as {ROLE_LABELS[user.role]}.</p>
      </div>
      <Card>
        <p className="text-muted text-sm">
          This is the code-frame Home page. Role-specific content comes next.
        </p>
      </Card>
    </div>
  )
}
