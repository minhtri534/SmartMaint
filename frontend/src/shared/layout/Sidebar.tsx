import { AnimatePresence, motion, useReducedMotion } from 'motion/react'
import { useEffect, useRef, useState } from 'react'
import { Link, useLocation } from 'react-router'
import { navGroupsFor } from '@/app/navigation'
import { paths } from '@/app/paths'
import { useAuth } from '@/features/auth'
import { ROLE_LABELS, ROLE_WORKSPACES } from '@/shared/types/role'
import { Icon, ProductMark } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { EASE_SNAP, slide } from '@/shared/utils/motion'

/**
 * The sidebar (Leader 18/11): one quiet floating panel. The product on top, the pages in named groups, and only the
 * open page carries a surface, which slides from item to item. The account sits at the bottom; change password and
 * sign out live in its menu. Phone and tablet (< 1024px): the same panel slides in as a drawer.
 */
export function Sidebar({ open, onClose }: { open: boolean; onClose: () => void }) {
  const { user } = useAuth()
  const { pathname } = useLocation()
  const reduce = useReducedMotion()
  if (!user) return null

  return (
    <aside
      className={cn(
        'ease-drawer fixed inset-y-0 left-0 z-40 w-[264px] p-3 transition-transform duration-300 print:hidden',
        'lg:sticky lg:top-0 lg:z-auto lg:h-screen lg:translate-x-0',
        open ? 'translate-x-0' : '-translate-x-full',
      )}
    >
      <div className="card-soft rounded-sheet flex h-full flex-col px-3 pt-3 pb-2">
        <div className="flex items-center gap-3 px-1">
          {/* the tile follows the light / dark theme (.brand-tile) */}
          <span className="brand-tile grid size-10 flex-none place-items-center rounded-xl">
            <ProductMark className="size-5" />
          </span>
          <div className="min-w-0 leading-tight">
            <div className="truncate text-[15px] font-semibold tracking-[-0.01em]">SmartMaint</div>
            <div className="text-faint mt-0.5 truncate text-[11px]">
              {ROLE_WORKSPACES[user.role]}
            </div>
          </div>
          <button
            type="button"
            onClick={onClose}
            className="text-muted hover:bg-active ml-auto grid size-9 flex-none place-items-center rounded-lg lg:hidden"
            aria-label="Close menu"
          >
            <Icon name="close" className="size-5" />
          </button>
        </div>
        <div className="bg-rule mt-3 h-px flex-none" />

        <motion.nav
          layoutScroll
          aria-label="Main"
          className="-mx-1 min-h-0 flex-1 [scrollbar-width:thin] [scrollbar-color:var(--color-control)_transparent] overflow-y-auto px-1 pb-3"
        >
          {navGroupsFor(user.role).map((group) => (
            <div key={group.label} role="group" aria-label={group.label}>
              <p className="text-faint px-3 pt-4 pb-1.5 text-[11px] font-medium">{group.label}</p>
              <ul className="flex flex-col gap-0.5">
                {group.items.map((item) => {
                  const active =
                    pathname === item.to || (item.also ?? []).some((p) => pathname.startsWith(p))
                  return (
                    <li key={item.to}>
                      <Link
                        to={item.to}
                        onClick={onClose}
                        aria-current={active ? 'page' : undefined}
                        className={cn(
                          'relative flex h-[38px] items-center gap-3 rounded-xl px-3 text-[13.5px] font-medium transition-colors duration-150',
                          active ? 'text-ink' : 'text-muted hover:bg-active hover:text-ink',
                        )}
                      >
                        {active && (
                          <motion.span
                            layoutId="nav-active"
                            transition={reduce ? { duration: 0 } : slide}
                            className="nav-surface absolute inset-0 rounded-xl"
                          />
                        )}
                        <Icon
                          name={item.icon}
                          className={cn('relative size-[17px]', active && 'text-accent-ink')}
                        />
                        <span className="relative truncate">{item.label}</span>
                      </Link>
                    </li>
                  )
                })}
              </ul>
            </div>
          ))}
        </motion.nav>

        <div className="bg-rule h-px flex-none" />
        <AccountMenu onNavigate={onClose} />
      </div>
    </aside>
  )
}

/** The signed-in person at the foot of the panel; the menu opens upwards. */
function AccountMenu({ onNavigate }: { onNavigate: () => void }) {
  const { user, signOut } = useAuth()
  const [open, setOpen] = useState(false)
  const box = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!open) return
    const close = (e: MouseEvent | KeyboardEvent) => {
      if (
        e instanceof KeyboardEvent ? e.key === 'Escape' : !box.current?.contains(e.target as Node)
      )
        setOpen(false)
    }
    document.addEventListener('mousedown', close)
    document.addEventListener('keydown', close)
    return () => {
      document.removeEventListener('mousedown', close)
      document.removeEventListener('keydown', close)
    }
  }, [open])

  if (!user) return null
  return (
    <div ref={box} className="relative mt-2">
      <AnimatePresence>
        {open && (
          <motion.div
            role="menu"
            initial={{ opacity: 0, scale: 0.96, y: 4 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.97, transition: { duration: 0.12 } }}
            transition={{ duration: 0.18, ease: EASE_SNAP }}
            style={{ transformOrigin: 'bottom center' }}
            className="card-soft absolute right-0 bottom-full left-0 z-40 mb-2 rounded-2xl p-1.5"
          >
            <Link
              role="menuitem"
              to={paths.changePassword}
              onClick={() => {
                setOpen(false)
                onNavigate()
              }}
              className="hover:bg-active flex h-9 items-center gap-2.5 rounded-lg px-2.5 text-[13.5px]"
            >
              <Icon name="shield" className="text-muted size-4" />
              Change password
            </Link>
            <button
              role="menuitem"
              type="button"
              onClick={() => signOut()}
              className="hover:bg-active flex h-9 w-full items-center gap-2.5 rounded-lg px-2.5 text-left text-[13.5px]"
            >
              <Icon name="logout" className="text-muted size-4" />
              Sign out
            </button>
          </motion.div>
        )}
      </AnimatePresence>

      <button
        type="button"
        aria-haspopup="menu"
        aria-expanded={open}
        onClick={() => setOpen((o) => !o)}
        className={cn(
          'flex w-full items-center gap-3 rounded-xl p-1.5 text-left transition-colors duration-150',
          open ? 'bg-active' : 'hover:bg-active',
        )}
      >
        <span className="brand-fill grid size-9 flex-none place-items-center rounded-xl text-xs font-semibold">
          {initials(user.fullName)}
        </span>
        <span className="min-w-0 flex-1 leading-tight">
          <span className="block truncate text-[13.5px] font-semibold">{user.fullName}</span>
          <span className="text-faint mt-0.5 block truncate text-[11px]">
            {ROLE_LABELS[user.role]}
          </span>
        </span>
        <Icon
          name="back"
          className={cn(
            'text-faint ease-snap mr-1 size-3.5 flex-none transition-transform duration-150',
            open ? '-rotate-90' : 'rotate-90',
          )}
        />
      </button>
    </div>
  )
}

function initials(name: string) {
  const parts = name.trim().split(/\s+/)
  return ((parts[0]?.[0] ?? '') + (parts.length > 1 ? (parts.at(-1)?.[0] ?? '') : '')).toUpperCase()
}
