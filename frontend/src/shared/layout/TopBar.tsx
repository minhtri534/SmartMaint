import { AnimatePresence, motion } from 'motion/react'
import { useEffect, useRef, useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router'
import { paths } from '@/app/paths'
import { useAuth } from '@/features/auth'
import { NotificationBell } from '@/features/notifications'
import { setTheme, useTheme } from '@/shared/theme/theme'
import type { Role } from '@/shared/types/role'
import { Icon } from '@/shared/ui'
import { EASE_SNAP } from '@/shared/utils/motion'
import { EmergencyAlert } from './EmergencyAlert'

/*
 * Top bar (Leader 18/11): the search on the left; on the right an unacknowledged emergency (only while there is one),
 * the theme switch and the bell. The counts that used to sit here are on Home and the case lists.
 */

/** Where the search box looks: the role's own case list (filtered by ?q=). Admin has no case list. */
const SEARCH_TARGET: Partial<Record<Role, string>> = {
  customer: paths.myRequests,
  staff: paths.cases,
  manager: paths.cases,
  technician: paths.workOrders,
}

/** The left of the bar: the search, or a quiet greeting for the Administrator, who has no cases to search. */
export function TopBarStart() {
  const { user } = useAuth()
  if (!user) return null
  const target = SEARCH_TARGET[user.role]
  if (target) return <Search target={target} />
  const given = user.fullName.trim().split(/\s+/).at(-1) ?? user.fullName
  return (
    <p className="text-muted hidden truncate text-[17px] tracking-[-0.01em] sm:block">
      Welcome back, <span className="text-ink font-semibold">{given}</span>
    </p>
  )
}

export function TopBarActions() {
  const { user } = useAuth()
  return (
    <div className="flex flex-none items-center gap-2">
      {(user?.role === 'staff' || user?.role === 'manager') && <EmergencyAlert role={user.role} />}
      <ThemeSwitch />
      <NotificationBell className={ROUND} />
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
      className="well flex h-10 w-full max-w-[520px] min-w-0 items-center gap-2 rounded-full pr-1.5 pl-3.5"
    >
      <Icon name="search" className="text-muted size-4 flex-none" />
      <input
        ref={input}
        value={q}
        onChange={(e) => setQ(e.target.value)}
        placeholder="Search cases…"
        aria-label="Search cases by title, code or room"
        className="placeholder:text-faint min-w-0 flex-1 bg-transparent text-sm outline-none"
      />
      <kbd className="bump text-muted hidden h-6 min-w-6 place-items-center rounded-md px-1 font-mono text-[11px] lg:grid">
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
