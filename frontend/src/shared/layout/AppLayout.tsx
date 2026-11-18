import { motion } from 'motion/react'
import { useEffect, useState } from 'react'
import { Outlet, useLocation } from 'react-router'
import { paths } from '@/app/paths'
import { AssistantLauncher } from '@/features/assistant'
import { clearJustSignedIn, useAuth, wasJustSignedIn } from '@/features/auth'
import { Icon } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { pageFade } from '@/shared/utils/motion'
import { Ribbon } from './Ribbon'
import { Sidebar } from './Sidebar'
import { TopBarActions, TopBarStart } from './TopBar'

/**
 * Signed-in shell: the sidebar panel on the left (Sidebar.tsx), the work on a light grey desk. The dashboards (Home,
 * Analytics) lay their cards straight on it, every other page sits in one soft sheet.
 * Phone and tablet (< 1024px): a 56px bar with the menu button; the sidebar slides in as a drawer.
 */

/** Pages built as a grid of cards (dashboards, the case lists): no sheet behind them, the cards sit on the desk. */
const CARD_PAGES: string[] = [paths.home, paths.myRequests, paths.cases, paths.workOrders]
const isDashboard = (pathname: string) =>
  CARD_PAGES.includes(pathname) || pathname.startsWith(paths.analytics)

export function AppLayout() {
  const { user } = useAuth()
  const [menuOpen, setMenuOpen] = useState(false)
  const { pathname } = useLocation()
  if (!user) return null

  const closeMenu = () => setMenuOpen(false)
  const bare = isDashboard(pathname)

  return (
    <div className="isolate min-h-full lg:grid lg:grid-cols-[264px_minmax(0,1fr)]">
      <DeskRibbons />
      {menuOpen && (
        <button
          type="button"
          aria-label="Close menu"
          onClick={closeMenu}
          className="fixed inset-0 z-30 bg-black/40 lg:hidden"
        />
      )}
      <Sidebar open={menuOpen} onClose={closeMenu} />

      <main className="min-w-0 px-4 pb-4 sm:px-6 lg:pr-5 lg:pl-3">
        {/* 88px: the row's centre (44px) lines up with the logo in the sidebar (12 + 12 + half of 40). */}
        <header className="bg-frame sticky top-0 z-20 -mx-4 flex h-14 items-center gap-3 px-4 sm:-mx-6 sm:px-6 lg:relative lg:mx-0 lg:h-[88px] lg:gap-6 lg:bg-transparent lg:px-0 print:hidden">
          <button
            type="button"
            onClick={() => setMenuOpen(true)}
            className="bump grid size-10 flex-none place-items-center rounded-full lg:hidden"
            aria-label="Open menu"
          >
            <Icon name="menu" className="size-5" />
          </button>
          <div className="min-w-0 flex-1">
            <TopBarStart />
          </div>
          <TopBarActions />
        </header>
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
      <SignInVeil />
    </div>
  )
}

/**
 * The sign-in page's ribbons, faint, across the whole desk (Leader 18/11): the same product as the door you came
 * through. Fixed to the screen, so they stay still while the cards scroll over them. Large screens only: on a phone
 * the cards take the full width.
 */
function DeskRibbons() {
  return (
    <div
      aria-hidden="true"
      className="desk-ribbon pointer-events-none fixed inset-0 -z-10 hidden overflow-hidden lg:block print:hidden"
    >
      <Ribbon
        id="desk-ribbon-top"
        d="M -240 150 C 160 -40, 430 300, 780 150 C 1060 30, 1280 250, 1840 90"
        width={74}
      />
      <Ribbon
        id="desk-ribbon-thin"
        d="M -220 470 C 240 320, 540 590, 900 440 C 1240 300, 1480 540, 1840 400"
        width={30}
      />
      <Ribbon
        id="desk-ribbon-low"
        d="M 640 1160 C 880 760, 1250 1010, 1460 730 C 1570 590, 1700 570, 1860 620"
        width={118}
      />
      <Ribbon
        id="desk-ribbon-main"
        d="M -280 800 C 170 430, 470 990, 830 630 C 1090 370, 1230 60, 1860 210"
        width={156}
      />
    </div>
  )
}

/** Right after signing in, the app fades up from the night colour of the sign-in page. */
function SignInVeil() {
  const [show, setShow] = useState(wasJustSignedIn)
  useEffect(() => {
    clearJustSignedIn()
    const done = window.setTimeout(() => setShow(false), 700)
    return () => window.clearTimeout(done)
  }, [])
  if (!show) return null
  return <div aria-hidden="true" className="sign-in-veil pointer-events-none fixed inset-0 z-50" />
}
