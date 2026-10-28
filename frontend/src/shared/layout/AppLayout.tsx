import { useState } from 'react'
import { NavLink, Outlet, useLocation } from 'react-router'
import { navItemsFor } from '@/app/navigation'
import { useAuth } from '@/features/auth'
import { ROLE_LABELS, ROLE_WORKSPACES } from '@/shared/types/role'
import { Icon, ProductMark } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'

/**
 * Signed-in shell (style "framed-sheet"): the sidebar sits on the tinted frame, the page on a white sheet
 * inset 8px from the window's top, right and bottom with a 1px edge and 16px corners — no shadow, no rule
 * between frame and sidebar. No top bar on desktop: there is no search or switcher to put in it yet.
 * Phone and tablet (< 1024px): the frame goes, a 48px bar with the menu button appears, the sidebar opens as a full sheet.
 */
export function AppLayout() {
  const { user, signOut } = useAuth()
  const [menuOpen, setMenuOpen] = useState(false)
  const { pathname } = useLocation()
  if (!user) return null

  const closeMenu = () => setMenuOpen(false)

  return (
    <div className="min-h-full lg:grid lg:grid-cols-[228px_minmax(0,1fr)]">
      <header className="bg-frame sticky top-0 z-20 flex h-12 items-center gap-3 px-4 lg:hidden">
        <button
          type="button"
          onClick={() => setMenuOpen(true)}
          className="hover:bg-nav-active -ml-1.5 grid size-9 place-items-center rounded-lg"
          aria-label="Open menu"
        >
          <Icon name="menu" className="size-5" />
        </button>
        <ProductName />
      </header>

      <aside
        className={cn(
          'bg-frame fixed inset-0 z-30 flex flex-col px-3 pt-4 pb-3 transition-transform duration-200',
          'lg:sticky lg:top-0 lg:z-auto lg:h-screen lg:translate-x-0',
          menuOpen ? 'translate-x-0' : '-translate-x-full',
        )}
      >
        <div className="flex items-start justify-between gap-2 px-2">
          <div>
            <ProductName />
            <div className="text-muted mt-0.5 pl-7 text-xs">{ROLE_WORKSPACES[user.role]}</div>
          </div>
          <button
            type="button"
            onClick={closeMenu}
            className="hover:bg-nav-active grid size-9 place-items-center rounded-lg lg:hidden"
            aria-label="Close menu"
          >
            <Icon name="close" className="size-5" />
          </button>
        </div>

        <nav className="mt-6 flex flex-col gap-0.5">
          {navItemsFor(user.role).map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end
              onClick={closeMenu}
              className={({ isActive: exact }) => {
                const isActive = exact || (item.also ?? []).some((p) => pathname.startsWith(p))
                return cn(
                  'flex h-8 items-center gap-2.5 rounded-lg px-2 text-sm transition-colors duration-150',
                  isActive
                    ? 'bg-nav-active text-ink'
                    : 'text-muted hover:bg-nav-active/60 hover:text-ink',
                )
              }}
            >
              <Icon name={item.icon} />
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="mt-auto flex items-center gap-2.5 px-2 pt-4">
          <span className="bg-pill-accent-bg text-pill-accent-ink grid size-7 flex-none place-items-center rounded-full text-xs font-semibold">
            {initials(user.fullName)}
          </span>
          <div className="min-w-0 flex-1 leading-tight">
            <div className="truncate text-sm font-medium">{user.fullName}</div>
            <div className="text-muted truncate text-xs">{ROLE_LABELS[user.role]}</div>
          </div>
          <button
            type="button"
            onClick={() => signOut()}
            className="text-muted hover:bg-nav-active hover:text-ink grid size-8 place-items-center rounded-lg"
            aria-label="Sign out"
            title="Sign out"
          >
            <Icon name="logout" />
          </button>
        </div>
      </aside>

      <main className="min-w-0 lg:py-2 lg:pr-2">
        <div className="bg-surface lg:border-border lg:rounded-sheet min-h-[calc(100vh-48px)] px-4 py-5 sm:px-6 lg:min-h-[calc(100vh-16px)] lg:border lg:p-6">
          <Outlet />
        </div>
      </main>
    </div>
  )
}

function ProductName() {
  return (
    <div className="flex items-center gap-2">
      <ProductMark />
      <span className="text-[15px] font-semibold tracking-[-0.01em]">SmartMaint</span>
    </div>
  )
}

function initials(name: string) {
  const parts = name.trim().split(/\s+/)
  return ((parts[0]?.[0] ?? '') + (parts.length > 1 ? (parts.at(-1)?.[0] ?? '') : '')).toUpperCase()
}
