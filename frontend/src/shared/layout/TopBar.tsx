import { AnimatePresence, motion } from 'motion/react'
import { useEffect, useRef, useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router'
import { paths } from '@/app/paths'
import { useAuth } from '@/features/auth'
import { useUnseenNotifications } from '@/features/notifications'
import { setTheme, useTheme } from '@/shared/theme/theme'
import { ROLE_LABELS, type Role } from '@/shared/types/role'
import { Icon } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { EASE_SNAP } from '@/shared/utils/motion'
import { StatusChips } from './StatusChips'

/*
 * Top bar (ui/mercato-style): search, theme switch, bell and the account pill on the right, each a raised round
 * button; on the left one quiet line, "Welcome back, <name>" (17/11: the earlier "Hi, …! Welcome back." with a
 * subtitle read as generated).
 */

/** Where the search box looks: the role's own case list (filtered by ?q=). Admin has no case list. */
const SEARCH_TARGET: Partial<Record<Role, string>> = {
  customer: paths.myRequests,
  staff: paths.cases,
  manager: paths.cases,
  technician: paths.workOrders,
}

/**
 * The left of the top bar (Leader 17/11): one quiet line, "Welcome back, Trí" (the given name = the last word of
 * a Vietnamese full name), then after a hairline the live status chips (what waits for this person now).
 */
export function Greeting() {
  const { user } = useAuth()
  if (!user) return null
  const given = user.fullName.trim().split(/\s+/).at(-1) ?? user.fullName
  return (
    <div className="flex min-w-0 items-center gap-4">
      <p className="text-muted flex-none text-[17px] tracking-[-0.01em] whitespace-nowrap">
        Welcome back, <span className="text-ink font-semibold">{given}</span>
      </p>
      {user.role !== 'admin' && (
        <>
          <span className="bg-border h-6 w-px flex-none" aria-hidden="true" />
          <StatusChips role={user.role} />
        </>
      )}
    </div>
  )
}

export function TopBarActions() {
  const { user } = useAuth()
  const target = user ? SEARCH_TARGET[user.role] : undefined
  return (
    <div className="flex items-center gap-2">
      {target && <Search target={target} />}
      <ThemeSwitch />
      <Bell />
      <AccountMenu />
    </div>
  )
}

function Search({ target }: { target: string }) {
  const navigate = useNavigate()
  const [q, setQ] = useState('')
  const input = useRef<HTMLInputElement>(null)
  // "/" focuses the search, as the hint in the box says.
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      const typing = (e.target as HTMLElement).closest('input, textarea, select, [contenteditable]')
      if (e.key === '/' && !typing) {
        e.preventDefault()
        input.current?.focus()
      }
    }
    document.addEventListener('keydown', onKey)
    return () => document.removeEventListener('keydown', onKey)
  }, [])
  const submit = (e: FormEvent) => {
    e.preventDefault()
    const text = q.trim()
    navigate(text ? `${target}?q=${encodeURIComponent(text)}` : target)
    input.current?.blur()
  }
  return (
    <form
      role="search"
      onSubmit={submit}
      className="well hidden h-10 w-72 items-center gap-2 rounded-full pr-1.5 pl-3.5 xl:flex"
    >
      <Icon name="search" className="text-muted size-4" />
      <input
        ref={input}
        value={q}
        onChange={(e) => setQ(e.target.value)}
        placeholder="Search cases…"
        aria-label="Search cases by title, code or room"
        className="placeholder:text-faint min-w-0 flex-1 bg-transparent text-sm outline-none"
      />
      <kbd className="bump text-muted grid h-6 min-w-6 place-items-center rounded-md px-1 font-mono text-[11px]">
        /
      </kbd>
    </form>
  )
}

const ROUND =
  'bump text-muted hover:text-ink ease-snap relative grid size-10 place-items-center rounded-full transition-[color,transform] duration-150 active:scale-95'

function ThemeSwitch() {
  const theme = useTheme()
  const next = theme === 'dark' ? 'light' : 'dark'
  return (
    <button
      type="button"
      onClick={() => setTheme(next)}
      aria-label={`Switch to ${next} theme`}
      title={`Switch to ${next} theme`}
      className={ROUND}
    >
      <AnimatePresence initial={false} mode="popLayout">
        <motion.span
          key={theme}
          initial={{ opacity: 0, rotate: -90, scale: 0.5 }}
          animate={{ opacity: 1, rotate: 0, scale: 1 }}
          exit={{ opacity: 0, rotate: 90, scale: 0.5 }}
          transition={{ duration: 0.22, ease: EASE_SNAP }}
          className="grid place-items-center"
        >
          <Icon name={theme === 'dark' ? 'moon' : 'sun'} className="size-[18px]" />
        </motion.span>
      </AnimatePresence>
    </button>
  )
}

function Bell() {
  const unseen = useUnseenNotifications()
  return (
    <Link
      to={paths.notifications}
      aria-label={unseen > 0 ? `Notifications, ${unseen} new` : 'Notifications'}
      className={ROUND}
    >
      <Icon name="bell" className="size-[18px]" />
      {unseen > 0 && (
        <span className="bg-accent ring-surface text-on-accent absolute -top-0.5 -right-0.5 grid h-[17px] min-w-[17px] place-items-center rounded-full px-1 text-[10px] leading-none font-semibold tabular-nums ring-2">
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
        className="bump ease-snap flex h-10 items-center gap-2.5 rounded-full py-1 pr-3 pl-1 transition-transform duration-150 active:scale-[0.97]"
      >
        <span className="bg-accent text-on-accent grid size-8 place-items-center rounded-full text-[11px] font-semibold">
          {initials(user.fullName)}
        </span>
        <span className="hidden max-w-36 min-w-0 text-left leading-tight sm:block">
          <span className="block truncate text-[13px] font-semibold">{user.fullName}</span>
          <span className="text-muted block truncate text-[11px]">{ROLE_LABELS[user.role]}</span>
        </span>
        <Icon
          name="back"
          className={cn(
            'text-muted size-3.5 -rotate-90 transition-transform duration-150',
            open && 'rotate-90',
          )}
        />
      </button>

      <AnimatePresence>
        {open && (
          <motion.div
            role="menu"
            initial={{ opacity: 0, scale: 0.96, y: -4 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.97, transition: { duration: 0.12 } }}
            transition={{ duration: 0.18, ease: EASE_SNAP }}
            style={{ transformOrigin: 'top right' }}
            className="card-soft absolute top-full right-0 z-40 mt-2 w-60 rounded-2xl p-1.5"
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
