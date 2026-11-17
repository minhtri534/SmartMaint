import { motion } from 'motion/react'
import { useEffect, useState } from 'react'
import { Link, Outlet, useLocation } from 'react-router'
import { navItemsFor } from '@/app/navigation'
import { paths } from '@/app/paths'
import { AssistantLauncher } from '@/features/assistant'
import { useAuth } from '@/features/auth'
import { ROLE_WORKSPACES } from '@/shared/types/role'
import { Icon, ProductMark } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { pageFade } from '@/shared/utils/motion'
import { Greeting, TopBarActions } from './TopBar'

/**
 * Signed-in shell, ui/mercato-style (17/11, blue since the 60/30/10 re-colour): a navy board on the left holds the product, the role's workspace
 * tag, the pages as raised keys, an LED clock and the account keys. The work sits on a light grey desk: the
 * dashboards (Home, Analytics) lay their cards straight on it, every other page sits in one soft sheet.
 * Phone and tablet (< 1024px): a 56px bar with the menu button; the board slides in as a drawer.
 */

/** Pages built as a grid of cards (dashboards, the case lists): no sheet behind them, the cards sit on the desk. */
const CARD_PAGES: string[] = [paths.home, paths.myRequests, paths.cases, paths.workOrders]
const isDashboard = (pathname: string) =>
  CARD_PAGES.includes(pathname) || pathname.startsWith(paths.analytics)

export function AppLayout() {
  const { user, signOut } = useAuth()
  const [menuOpen, setMenuOpen] = useState(false)
  const { pathname } = useLocation()
  if (!user) return null

  const closeMenu = () => setMenuOpen(false)
  const bare = isDashboard(pathname)

  return (
    <div className="min-h-full lg:grid lg:grid-cols-[264px_minmax(0,1fr)]">
      <header className="bg-frame sticky top-0 z-20 flex h-14 items-center gap-3 px-4 lg:hidden print:hidden">
        <button
          type="button"
          onClick={() => setMenuOpen(true)}
          className="bump grid size-10 place-items-center rounded-full"
          aria-label="Open menu"
        >
          <Icon name="menu" className="size-5" />
        </button>
        <div className="ml-auto">
          <TopBarActions />
        </div>
      </header>

      {menuOpen && (
        <button
          type="button"
          aria-label="Close menu"
          onClick={closeMenu}
          className="fixed inset-0 z-30 bg-black/40 lg:hidden"
        />
      )}
      <aside
        className={cn(
          'ease-drawer fixed inset-y-0 left-0 z-40 w-[264px] p-3 transition-transform duration-300 print:hidden',
          'lg:sticky lg:top-0 lg:z-auto lg:h-screen lg:translate-x-0',
          menuOpen ? 'translate-x-0' : '-translate-x-full',
        )}
      >
        <div className="board flex h-full flex-col rounded-[22px] px-3.5 pt-3 pb-3.5">
          <div className="flex items-center gap-2.5 px-1">
            <span className="key key-light grid size-10 flex-none place-items-center rounded-xl">
              <ProductMark className="size-5" />
            </span>
            <div className="min-w-0 leading-tight">
              <div className="truncate text-[16px] font-semibold tracking-[-0.01em]">
                SmartMaint
              </div>
              <div className="text-board-muted truncate text-[10px] tracking-[0.1em] uppercase">
                Facility care
              </div>
            </div>
            <button
              type="button"
              onClick={closeMenu}
              className="text-board-muted hover:bg-board-ink/10 ml-auto grid size-9 place-items-center rounded-lg lg:hidden"
              aria-label="Close menu"
            >
              <Icon name="close" className="size-5" />
            </button>
          </div>

          <div className="mt-6 px-1">
            <span className="bg-accent text-on-accent rounded-md px-2 py-0.5 text-[10px] font-semibold tracking-[0.12em] uppercase shadow-[inset_0_1px_0_rgb(255_255_255/0.35),0_1px_2px_rgb(0_0_0/0.4)]">
              {ROLE_WORKSPACES[user.role]}
            </span>
          </div>

          <nav className="mt-3 flex flex-col gap-2 overflow-y-auto p-1">
            {navItemsFor(user.role).map((item) => {
              const active =
                pathname === item.to || (item.also ?? []).some((p) => pathname.startsWith(p))
              return (
                <Link
                  key={item.to}
                  to={item.to}
                  onClick={closeMenu}
                  aria-current={active ? 'page' : undefined}
                  className="key flex h-10 flex-none items-center gap-2.5 rounded-xl px-3 text-[13.5px] font-medium"
                >
                  <Icon
                    name={item.icon}
                    className={cn('size-[17px]', active ? 'text-board-accent' : 'opacity-70')}
                  />
                  <span className="truncate">{item.label}</span>
                  {active && (
                    <span className="bg-board-accent ml-auto size-1.5 rounded-full shadow-[0_0_6px_var(--color-board-accent)]" />
                  )}
                </Link>
              )
            })}
          </nav>

          <div className="mt-auto flex flex-col gap-2 p-1 pt-4">
            <LedClock />
            <Link
              to={paths.changePassword}
              onClick={closeMenu}
              className="key flex h-10 items-center gap-2.5 rounded-xl px-3 text-[13.5px] font-medium"
            >
              <Icon name="shield" className="size-[17px] opacity-70" />
              Change password
            </Link>
            <button
              type="button"
              onClick={() => signOut()}
              className="key key-danger flex h-10 items-center gap-2.5 rounded-xl px-3 text-left text-[13.5px] font-semibold"
            >
              <Icon name="logout" className="size-[17px]" />
              Sign out
            </button>
          </div>
        </div>
      </aside>

      <main className="min-w-0 px-4 pb-4 sm:px-6 lg:pr-5 lg:pl-3">
        {/* 88px: the row's centre (44px) lines up with the logo key on the board (12 + 12 + half of 40). */}
        <div className="hidden h-[88px] items-center justify-between gap-6 lg:flex print:hidden">
          <Greeting />
          <TopBarActions />
        </div>
        {/* Bottom padding: the assistant's launcher floats over the bottom-right corner (not for Admin). */}
        <div
          className={cn(
            !bare && 'card-soft rounded-sheet px-4 py-5 sm:px-6 lg:p-6',
            user.role !== 'admin' && 'mb-20',
          )}
        >
          <motion.div
            key={pathname}
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={pageFade}
          >
            <Outlet />
          </motion.div>
        </div>
      </main>
      <AssistantLauncher />
    </div>
  )
}

/** The LED clock on the board: hours and minutes with a blinking colon, the date under it. */
function LedClock() {
  const [now, setNow] = useState(() => new Date())
  useEffect(() => {
    const id = window.setInterval(() => setNow(new Date()), 1000)
    return () => window.clearInterval(id)
  }, [])
  const hh = String(now.getHours()).padStart(2, '0')
  const mm = String(now.getMinutes()).padStart(2, '0')
  const ss = String(now.getSeconds()).padStart(2, '0')
  const day = now.toLocaleDateString('en-GB', { weekday: 'short', day: 'numeric', month: 'short' })
  return (
    <div className="led rounded-xl px-3.5 py-2.5" role="timer" aria-label={`Time ${hh}:${mm}`}>
      <div className="flex items-baseline gap-1.5 font-mono">
        <span className="text-[28px] leading-8 font-semibold tracking-[0.06em] tabular-nums">
          {hh}
          <span className="led-colon">:</span>
          {mm}
        </span>
        <span className="text-[13px] tabular-nums opacity-70">{ss}</span>
      </div>
      <div className="text-[10px] tracking-[0.16em] uppercase opacity-70">{day}</div>
    </div>
  )
}
