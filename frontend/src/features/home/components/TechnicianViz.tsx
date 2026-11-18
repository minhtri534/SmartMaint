import { motion, useReducedMotion } from 'motion/react'
import { Link } from 'react-router'
import { caseDetailPath } from '@/app/paths'
import { groupOf, groupStyle, slaOf, type CaseSummary } from '@/features/cases'
import { EmptyBlock, Icon } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { formatDuration, formatTime } from '@/shared/utils/format'
import { EASE_SNAP } from '@/shared/utils/motion'

/*
 * Technician home pieces (17/11): a technician plans the week and races deadlines. The planner puts each job on
 * the day it is planned to start (solid) or, when nobody planned it, on the day it is due (outlined); late jobs sit
 * on today in red. The rings count down the deadlines that are closest.
 */

const DAY = 86_400_000
const OPEN = ['approved', 'accepted', 'in_progress', 'on_hold', 'reopened']

interface Slot {
  c: CaseSummary
  kind: 'planned' | 'due' | 'late'
  at: number
}

function short(ms: number) {
  return formatDuration(Math.abs(ms)).split(' ').slice(0, 2).join(' ')
}

export function WeekPlanner({ cases, now }: { cases: CaseSummary[]; now: number }) {
  const reduce = useReducedMotion()
  const today = new Date(now)
  today.setHours(0, 0, 0, 0)
  const start = today.getTime()
  const days = Array.from({ length: 7 }, (_, i) => new Date(start + i * DAY))
  const columns: Slot[][] = days.map(() => [])
  const anytime: CaseSummary[] = []

  for (const c of cases.filter((x) => OPEN.includes(x.status))) {
    const planned = c.plannedStartAt ? Date.parse(c.plannedStartAt) : null
    const due = c.dueAt ? Date.parse(c.dueAt) : null
    let slot: Slot | null = null
    if (due !== null && due < now) slot = { c, kind: 'late', at: now }
    else if (planned !== null && planned >= start && planned < start + 7 * DAY)
      slot = { c, kind: 'planned', at: planned }
    else if (due !== null && due >= start && due < start + 7 * DAY)
      slot = { c, kind: 'due', at: due }
    if (slot) columns[Math.min(6, Math.max(0, Math.floor((slot.at - start) / DAY)))]!.push(slot)
    else anytime.push(c)
  }
  columns.forEach((col) => col.sort((a, b) => a.at - b.at))

  return (
    <div className="flex flex-col gap-4">
      {/* Phone: seven readable days that slide sideways inside the card, so the page itself never scrolls sideways. */}
      <div
        tabIndex={0}
        role="group"
        aria-label="Your next 7 days"
        className="-mx-5 grid snap-x scroll-px-5 [scrollbar-width:thin] [scrollbar-color:var(--color-control)_transparent] grid-cols-[repeat(7,minmax(112px,1fr))] gap-2 overflow-x-auto px-5 pb-2 sm:mx-0 sm:grid-cols-7 sm:overflow-visible sm:px-0 sm:pb-0"
      >
        {days.map((d, i) => (
          <div key={d.getTime()} className="flex min-w-0 snap-start flex-col gap-2">
            <div
              className={cn(
                'rounded-xl px-2 py-1.5 text-center',
                i === 0 ? 'bg-accent text-on-accent' : 'well',
              )}
            >
              <div className={cn('text-[11px] uppercase', i === 0 ? 'opacity-85' : 'text-muted')}>
                {i === 0 ? 'Today' : d.toLocaleDateString('en-GB', { weekday: 'short' })}
              </div>
              <div className="text-[17px] leading-6 font-semibold tabular-nums">{d.getDate()}</div>
            </div>
            <div className="flex min-h-24 flex-col gap-1.5">
              {columns[i]!.map((s, j) => {
                const kind = groupStyle(groupOf(s.c.serviceTypeName))
                return (
                  <motion.div
                    key={s.c.id}
                    initial={reduce ? false : { opacity: 0, y: 6 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{
                      duration: 0.35,
                      ease: EASE_SNAP,
                      delay: 0.15 + i * 0.05 + j * 0.04,
                    }}
                  >
                    <Link
                      to={caseDetailPath(s.c.id)}
                      title={s.c.title}
                      className={cn(
                        'block rounded-lg border px-1.5 py-1 text-[11px] leading-tight transition-transform duration-150 hover:-translate-y-px',
                        s.kind === 'planned' && 'bg-accent-soft border-accent/30 text-accent-ink',
                        s.kind === 'due' && 'border-control bg-surface border-dashed',
                        s.kind === 'late' &&
                          'border-negative/40 bg-pill-negative-bg text-pill-negative-ink',
                      )}
                    >
                      <span className="flex items-start gap-1">
                        <Icon name={kind.icon} className="mt-px size-3 flex-none" />
                        <span className="line-clamp-2 font-medium break-words">{s.c.title}</span>
                      </span>
                      <span className="mt-0.5 block truncate opacity-80">
                        {s.kind === 'late'
                          ? 'late'
                          : `${s.kind === 'planned' ? 'starts' : 'due'} ${formatTime(new Date(s.at).toISOString())}`}
                      </span>
                    </Link>
                  </motion.div>
                )
              })}
            </div>
          </div>
        ))}
      </div>
      <div className="border-rule flex flex-wrap items-center gap-x-4 gap-y-2 border-t pt-3 text-xs">
        <span className="text-muted flex items-center gap-1.5">
          <span className="bg-accent-soft border-accent/30 size-3 rounded border" /> planned start
        </span>
        <span className="text-muted flex items-center gap-1.5">
          <span className="border-control size-3 rounded border border-dashed" /> deadline
        </span>
        <span className="text-muted flex items-center gap-1.5">
          <span className="bg-pill-negative-bg border-negative/40 size-3 rounded border" /> late
        </span>
        {anytime.length > 0 && (
          <span className="text-muted ml-auto">
            {anytime.length} more with no date yet (deadline starts with the work)
          </span>
        )}
      </div>
    </div>
  )
}

// ---------- deadline rings ----------

function Ring({ c, index }: { c: CaseSummary; index: number }) {
  const reduce = useReducedMotion()
  const sla = slaOf(c)
  const late = sla.state === 'overdue'
  const used = late ? 1 : Math.max(0.03, Math.min(1, sla.used))
  const r = 30
  const circ = 2 * Math.PI * r
  const stroke = late
    ? 'var(--color-negative)'
    : used > 0.75
      ? 'var(--color-warning)'
      : 'var(--color-accent)'
  return (
    <Link
      to={caseDetailPath(c.id)}
      className="group flex min-w-0 items-center gap-3 rounded-xl p-1 transition-colors duration-150"
    >
      <div className="relative size-[76px] flex-none">
        <svg viewBox="0 0 76 76" className="size-full -rotate-90" aria-hidden="true">
          <circle cx="38" cy="38" r={r} fill="none" stroke="var(--color-rule)" strokeWidth="7" />
          <motion.circle
            cx="38"
            cy="38"
            r={r}
            fill="none"
            stroke={stroke}
            strokeWidth="7"
            strokeLinecap="round"
            strokeDasharray={circ}
            initial={{ strokeDashoffset: reduce ? circ * (1 - used) : circ }}
            animate={{ strokeDashoffset: circ * (1 - used) }}
            transition={{ duration: 1, ease: EASE_SNAP, delay: 0.25 + index * 0.12 }}
          />
        </svg>
        <div className="absolute inset-0 grid place-items-center text-center">
          <div
            className={cn(
              'text-[12px] leading-tight font-semibold tabular-nums',
              late && 'text-negative',
            )}
          >
            {short(sla.remaining)}
            <div className="text-muted text-[10px] font-normal">{late ? 'late' : 'left'}</div>
          </div>
        </div>
      </div>
      <div className="min-w-0">
        <div className="group-hover:text-accent-ink truncate text-[13.5px] font-semibold transition-colors">
          {c.title}
        </div>
        <div className="text-muted truncate text-xs">
          <span className="font-mono">{c.code}</span> · {c.location.roomName}
        </div>
      </div>
    </Link>
  )
}

export function DeadlineRings({ cases }: { cases: CaseSummary[] }) {
  const racing = cases
    .map((c) => ({ c, sla: slaOf(c) }))
    .filter(({ sla }) => sla.state === 'running' || sla.state === 'overdue')
    .sort((a, b) => a.sla.remaining - b.sla.remaining)
    .slice(0, 3)
  if (racing.length === 0) {
    return <EmptyBlock>No clock is running. A deadline starts when you start the work.</EmptyBlock>
  }
  return (
    <ul className="flex flex-col gap-3">
      {racing.map(({ c }, i) => (
        <li key={c.id}>
          <Ring c={c} index={i} />
        </li>
      ))}
    </ul>
  )
}
