import { AnimatePresence, motion } from 'motion/react'
import { useEffect, useRef, useState } from 'react'
import { Link } from 'react-router'
import { paths } from '@/app/paths'
import { useAuth } from '@/features/auth'
import { useUnseenNotifications } from '@/features/notifications'
import { ROLE_LABELS } from '@/shared/types/role'
import { Icon } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { EASE_SNAP } from '@/shared/utils/motion'

/*
 * The right end of the top bar (16/11): the bell with what is new since the user last opened Notifications,
 * then the account button with its menu (who is signed in, change password, sign out). Used in the desktop bar
 * above the sheet and in the phone bar.
 */
export function TopBarActions() {
  return (
    <div className="flex items-center gap-1">
      <Bell />
      <AccountMenu />
    </div>
  )
}

function Bell() {
  const unseen = useUnseenNotifications()
  return (
    <Link
      to={paths.notifications}
      aria-label={unseen > 0 ? `Notifications, ${unseen} new` : 'Notifications'}
      className="text-muted hover:text-ink hover:bg-active ease-snap relative grid size-9 place-items-center rounded-full transition-[color,background-color,transform] duration-150 active:scale-95"
    >
      <Icon name="bell" className="size-[18px]" />
      {unseen > 0 && (
        <span className="bg-accent ring-frame absolute top-0.5 -right-0.5 grid h-[15px] min-w-[15px] place-items-center rounded-full px-1 text-[9.5px] leading-none font-semibold text-white tabular-nums ring-2">
          {unseen > 9 ? '9+' : unseen}
        </span>
      )}
    </Link>
  )
}

function AccountMenu() {
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
    <div ref={box} className="relative">
      <button
        type="button"
        aria-haspopup="menu"
        aria-expanded={open}
        onClick={() => setOpen((o) => !o)}
        className="hover:bg-active ease-snap flex items-center gap-2 rounded-full py-1 pr-2.5 pl-1 transition-[background-color,transform] duration-150 active:scale-[0.97]"
      >
        <span className="bg-accent grid size-7 place-items-center rounded-full text-[11px] font-semibold text-white">
          {initials(user.fullName)}
        </span>
        <span className="hidden max-w-36 truncate text-sm font-medium sm:block">
          {user.fullName}
        </span>
        <Icon name="back" className={cn('text-muted size-3.5 -rotate-90', open && 'rotate-90')} />
      </button>

      <AnimatePresence>
        {open && (
          // opens from its button (top right), 180ms; closes faster
          <motion.div
            role="menu"
            initial={{ opacity: 0, scale: 0.96, y: -4 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.97, transition: { duration: 0.12 } }}
            transition={{ duration: 0.18, ease: EASE_SNAP }}
            style={{ transformOrigin: 'top right' }}
            className="bg-surface border-border absolute top-full right-0 z-40 mt-2 w-60 rounded-xl border p-1.5 shadow-[0_12px_32px_-8px_rgb(15_27_45/0.25)]"
          >
            <div className="px-2.5 py-2">
              <div className="truncate text-sm font-semibold">{user.fullName}</div>
              <div className="text-muted text-xs">{ROLE_LABELS[user.role]}</div>
            </div>
            <div className="bg-rule my-1 h-px" />
            <Link
              role="menuitem"
              to={paths.changePassword}
              onClick={() => setOpen(false)}
              className="hover:bg-active flex h-9 items-center gap-2.5 rounded-lg px-2.5 text-sm"
            >
              <Icon name="shield" className="text-muted size-4" />
              Change password
            </Link>
            <button
              role="menuitem"
              type="button"
              onClick={() => signOut()}
              className="hover:bg-active flex h-9 w-full items-center gap-2.5 rounded-lg px-2.5 text-left text-sm"
            >
              <Icon name="logout" className="text-muted size-4" />
              Sign out
            </button>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  )
}

function initials(name: string) {
  const parts = name.trim().split(/\s+/)
  return ((parts[0]?.[0] ?? '') + (parts.length > 1 ? (parts.at(-1)?.[0] ?? '') : '')).toUpperCase()
}
