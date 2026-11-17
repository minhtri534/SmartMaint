import { motion, useReducedMotion } from 'motion/react'
import { Link } from 'react-router'
import { paths } from '@/app/paths'
import { FINAL_STATUSES, isOverdue, useCases, type CaseSummary } from '@/features/cases'
import type { Role } from '@/shared/types/role'
import { cn } from '@/shared/utils/cn'
import { EASE_SNAP } from '@/shared/utils/motion'

/*
 * Live status chips beside the greeting (ui/mercato-style, 17/11): what waits for this person right now, as small
 * raised pills that link to the list behind them. Only counts above zero are shown, the most urgent first; with
 * nothing waiting one green "All clear". Red chips pulse softly. Same case list (cache) as the pages.
 */

type Tone = 'red' | 'orange' | 'violet' | 'green'

interface Chip {
  key: string
  count: number
  label: string
  tone: Tone
  to: string
}

const DOT: Record<Tone, string> = {
  red: 'bg-negative',
  orange: 'bg-accent',
  violet: 'bg-chip-violet',
  green: 'bg-positive',
}

const isOpen = (c: CaseSummary) => !FINAL_STATUSES.includes(c.status)

function chipsFor(role: Role, cases: CaseSummary[]): Chip[] {
  const overdue = cases.filter((c) => isOverdue(c)).length
  const emergencies = cases.filter(
    (c) => c.isEmergencyFlow && isOpen(c) && !c.emergency?.acknowledgedAt,
  ).length
  switch (role) {
    case 'customer':
      return [
        {
          key: 'open',
          count: cases.filter((c) => isOpen(c) && c.status !== 'completed').length,
          label: 'open',
          tone: 'orange',
          to: `${paths.myRequests}?tab=open`,
        },
        {
          key: 'check',
          count: cases.filter((c) => c.status === 'completed').length,
          label: 'being checked',
          tone: 'green',
          to: `${paths.myRequests}?tab=open`,
        },
      ]
    case 'staff':
      return [
        {
          key: 'emergency',
          count: emergencies,
          label: emergencies === 1 ? 'emergency' : 'emergencies',
          tone: 'red',
          to: `${paths.cases}?tab=all&urgency=emergency`,
        },
        { key: 'overdue', count: overdue, label: 'overdue', tone: 'red', to: paths.cases },
        {
          key: 'review',
          count: cases.filter((c) => c.status === 'pending').length,
          label: 'to review',
          tone: 'orange',
          to: paths.cases,
        },
        {
          key: 'verify',
          count: cases.filter((c) => c.status === 'completed').length,
          label: 'to verify',
          tone: 'violet',
          to: paths.cases,
        },
      ]
    case 'technician':
      return [
        { key: 'overdue', count: overdue, label: 'overdue', tone: 'red', to: paths.workOrders },
        {
          key: 'new',
          count: cases.filter((c) => c.status === 'approved' || c.status === 'reopened').length,
          label: 'new jobs',
          tone: 'orange',
          to: paths.workOrders,
        },
        {
          key: 'working',
          count: cases.filter((c) => c.status === 'accepted' || c.status === 'in_progress').length,
          label: 'in hand',
          tone: 'violet',
          to: paths.workOrders,
        },
      ]
    case 'manager':
      return [
        {
          key: 'emergency',
          count: emergencies,
          label: emergencies === 1 ? 'emergency' : 'emergencies',
          tone: 'red',
          to: paths.emergencies,
        },
        { key: 'overdue', count: overdue, label: 'overdue', tone: 'red', to: paths.cases },
        {
          key: 'open',
          count: cases.filter(isOpen).length,
          label: 'open',
          tone: 'orange',
          to: paths.cases,
        },
      ]
    default:
      return []
  }
}

export function StatusChips({ role }: { role: Role }) {
  const cases = useCases(60_000)
  const reduce = useReducedMotion()
  if (!cases.data) return null

  const chips = chipsFor(role, cases.data).filter((c) => c.count > 0)
  const shown: Chip[] = chips.length
    ? chips.slice(0, 3)
    : [{ key: 'clear', count: 0, label: 'All clear', tone: 'green', to: paths.home }]

  return (
    <ul className="flex min-w-0 items-center gap-2" aria-label="What waits for you">
      {shown.map((chip, i) => (
        <motion.li
          key={chip.key}
          initial={reduce ? { opacity: 0 } : { opacity: 0, y: -4 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.35, ease: EASE_SNAP, delay: 0.1 + i * 0.07 }}
          // the third chip only where the bar is wide enough
          className={cn(i === 2 && 'hidden 2xl:block')}
        >
          <Link
            to={chip.to}
            className="bump hover:text-ink ease-snap flex h-8 items-center gap-2 rounded-full pr-3 pl-2.5 text-[12.5px] whitespace-nowrap transition-[color,transform] duration-150 active:scale-[0.97]"
          >
            <span className="relative flex size-2">
              {chip.tone === 'red' && (
                <span
                  className={cn(
                    'absolute inset-0 animate-ping rounded-full opacity-60',
                    DOT[chip.tone],
                  )}
                />
              )}
              <span className={cn('relative size-2 rounded-full', DOT[chip.tone])} />
            </span>
            {chip.key === 'clear' ? (
              <span className="text-muted font-medium">{chip.label}</span>
            ) : (
              <span className="text-muted">
                <b className="text-ink font-semibold tabular-nums">{chip.count}</b> {chip.label}
              </span>
            )}
          </Link>
        </motion.li>
      ))}
    </ul>
  )
}
