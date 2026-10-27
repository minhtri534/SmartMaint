import { NavLink, Outlet } from 'react-router'
import { navItemsFor } from '@/app/navigation'
import { useAuth } from '@/features/auth'
import { ROLE_LABELS } from '@/shared/types/role'
import { Button } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'

/** Signed-in shell: sidebar filtered by role + top bar. Only rendered inside <RequireAuth>. */
export function AppLayout() {
  const { user, signOut } = useAuth()
  if (!user) return null

  return (
    <div className="flex min-h-full">
      <aside className="border-line bg-surface hidden w-60 shrink-0 flex-col border-r p-4 md:flex">
        <div className="px-2 text-lg font-semibold tracking-tight">SmartMaint</div>
        <nav className="mt-6 flex flex-col gap-1">
          {navItemsFor(user.role).map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end
              className={({ isActive }) =>
                cn(
                  'rounded-md px-2 py-1.5 text-sm',
                  isActive
                    ? 'bg-primary-soft text-primary font-medium'
                    : 'text-muted hover:bg-canvas',
                )
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="border-line bg-surface flex h-14 items-center justify-end gap-4 border-b px-6">
          <div className="text-right text-sm leading-tight">
            <div className="font-medium">{user.fullName}</div>
            <div className="text-muted text-xs">{ROLE_LABELS[user.role]}</div>
          </div>
          <Button variant="secondary" onClick={() => signOut()}>
            Sign out
          </Button>
        </header>
        <main className="flex-1 p-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
