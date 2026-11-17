import { motion, useReducedMotion } from 'motion/react'
import { Link } from 'react-router'
import { paths } from '@/app/paths'
import { isOverdue, URGENCY_LABELS, type CaseSummary } from '@/features/cases'
import { CountUp, Icon, type IconName } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { EASE_SNAP } from '@/shared/utils/motion'

/*
 * Staff home pieces (17/11): the facility staff run a queue, so the first thing they see is where the work is
 * stuck. The pipeline shows every open case at its stage, with the overdue ones in red; the stage that holds the
 * most work glows. The urgency bar is the triage view of everything open.
 */

interface Stage {
  key: string
  label: string
  hint: string
  icon: IconName
  match: (c: CaseSummary) => boolean
  /** Work waiting for the staff themselves (not for the technician). */
  yours: boolean
  to: string
}

const STAGES: Stage[] = [
  {
    key: 'pending',
    label: 'New',
    hint: 'to review',
    icon: 'requests',
    match: (c) => c.status === 'pending',
    yours: true,
    to: `${paths.cases}?tab=all&status=pending`,
  },
  {
    key: 'approved',
    label: 'Approved',
    hint: 'waiting for a technician',
    icon: 'users',
    match: (c) => c.status === 'approved',
    yours: false,
    to: `${paths.cases}?tab=all&status=approved`,
  },
  {
    key: 'work',
    label: 'In work',
    hint: 'accepted or in progress',
    icon: 'wrench',
    match: (c) => c.status === 'accepted' || c.status === 'in_progress' || c.status === 'reopened',
    yours: false,
    to: `${paths.cases}?tab=all&status=in_progress`,
  },
  {
    key: 'hold',
    label: 'On hold',
    hint: 'paused',
    icon: 'clock',
    match: (c) => c.status === 'on_hold',
    yours: false,
    to: `${paths.cases}?tab=all&status=on_hold`,
  },
  {
    key: 'verify',
    label: 'To verify',
    hint: 'reported done',
    icon: 'checklist',
    match: (c) => c.status === 'completed',
    yours: true,
    to: `${paths.cases}?tab=all&status=completed`,
  },
]

export function CasePipeline({ cases }: { cases: CaseSummary[] }) {
  const reduce = useReducedMotion()
  const rows = STAGES.map((s) => {
    const inStage = cases.filter(s.match)
    return { s, count: inStage.length, late: inStage.filter((c) => isOverdue(c)).length }
  })
  const max = Math.max(1, ...rows.map((r) => r.count))
  const busiest = rows.reduce((a, b) => (b.count > a.count ? b : a), rows[0]!)

  return (
    <ol className="grid grid-cols-2 gap-3 sm:grid-cols-3 xl:grid-cols-5">
      {rows.map(({ s, count, late }, i) => {
        const hot = count > 0 && busiest.s.key === s.key
        return (
          <motion.li
            key={s.key}
            className="relative min-w-0"
            initial={reduce ? false : { opacity: 0, x: -10 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ duration: 0.4, ease: EASE_SNAP, delay: 0.1 + i * 0.08 }}
          >
            <Link
              to={s.to}
              className={cn(
                'card-soft card-hover rounded-card flex h-full flex-col p-4',
                hot && 'border-accent/40! shadow-[0_0_0_3px_rgb(47_111_237/0.12)]',
              )}
            >
              <div className="flex items-center justify-between gap-2">
                <span className="text-faint text-[11px] font-semibold tracking-[0.08em] uppercase">
                  {i + 1} · {s.yours ? 'yours' : 'technician'}
                </span>
                <Icon name={s.icon} className={cn('size-4', hot ? 'text-accent' : 'text-faint')} />
              </div>
              <div className="mt-2 text-[15px] font-semibold">{s.label}</div>
              <div className="mt-1 flex items-baseline gap-2">
                <span className="text-[32px] leading-9 font-semibold tracking-[-0.03em] tabular-nums">
                  <CountUp value={count} />
                </span>
                {late > 0 && (
                  <span className="text-negative text-xs font-medium tabular-nums">
                    {late} overdue
                  </span>
                )}
              </div>
              <div className="text-muted text-xs">{s.hint}</div>
              {/* how full this stage is, against the fullest one; the overdue share of it in red */}
              <div className="bg-rule mt-3 h-1.5 overflow-hidden rounded-full">
                <motion.div
                  className="flex h-full origin-left overflow-hidden rounded-full"
                  style={{ width: `${(count / max) * 100}%` }}
                  initial={reduce ? false : { scaleX: 0 }}
                  animate={{ scaleX: 1 }}
                  transition={{ duration: 0.7, ease: EASE_SNAP, delay: 0.3 + i * 0.08 }}
                >
                  <span
                    className="bg-negative h-full"
                    style={{ width: `${(late / Math.max(1, count)) * 100}%` }}
                  />
                  <span className="bar-3d h-full flex-1" />
                </motion.div>
              </div>
            </Link>
            {/* the arrow to the next stage */}
            {i < STAGES.length - 1 && (
              <span
                aria-hidden="true"
                className="bump text-faint absolute top-1/2 -right-[13px] z-10 hidden size-6 -translate-y-1/2 place-items-center rounded-full xl:grid"
              >
                <Icon name="arrowRight" className="size-3.5" />
              </span>
            )}
          </motion.li>
        )
      })}
    </ol>
  )
}

// ---------- urgency mix ----------

const URGENCY_ORDER = ['emergency', 'high', 'normal', 'low'] as const
const URGENCY_BAR: Record<(typeof URGENCY_ORDER)[number], string> = {
  emergency: 'bg-negative',
  high: 'bg-warning',
  normal: 'bg-accent',
  low: 'bg-control',
}

/** Everything open, split by urgency, as one bar; the oldest open case under it. */
export function UrgencyMix({ cases, now }: { cases: CaseSummary[]; now: number }) {
  const reduce = useReducedMotion()
  // Same "open" as the F6 numbers: not finished, merged reports aside (their work lives in the main case).
  const open = cases.filter(
    (c) => !['closed', 'rejected', 'cancelled'].includes(c.status) && !c.emergency?.mergedInto,
  )
  const total = open.length
  const oldest = [...open].sort((a, b) => a.submittedAt.localeCompare(b.submittedAt))[0]
  const days = oldest ? Math.floor((now - Date.parse(oldest.submittedAt)) / 86_400_000) : 0

  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-baseline justify-between">
        <span className="text-[32px] leading-9 font-semibold tabular-nums">
          <CountUp value={total} />
        </span>
        <span className="text-muted text-xs">open in your buildings</span>
      </div>
      <div className="bg-rule flex h-3.5 overflow-hidden rounded-full">
        {URGENCY_ORDER.map((u, i) => {
          const n = open.filter((c) => c.urgency === u).length
          if (n === 0) return null
          return (
            <motion.div
              key={u}
              title={`${URGENCY_LABELS[u]}: ${n}`}
              className={cn('h-full', URGENCY_BAR[u], i > 0 && 'border-surface border-l-2')}
              style={{ width: `${(n / Math.max(1, total)) * 100}%` }}
              initial={reduce ? false : { opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ duration: 0.35, delay: 0.2 + i * 0.1 }}
            />
          )
        })}
      </div>
      <ul className="grid grid-cols-2 gap-2 text-[13px]">
        {URGENCY_ORDER.map((u) => (
          <li key={u} className="flex items-center gap-2">
            <span className={cn('size-2.5 rounded-full', URGENCY_BAR[u])} />
            <span className="text-muted flex-1">{URGENCY_LABELS[u]}</span>
            <span className="font-semibold tabular-nums">
              {open.filter((c) => c.urgency === u).length}
            </span>
          </li>
        ))}
      </ul>
      {oldest && (
        <Link
          to={`${paths.cases}?tab=all&sort=oldest`}
          className="well hover:text-accent-ink flex items-center justify-between gap-3 rounded-xl px-3.5 py-2.5 text-[13px] transition-colors"
        >
          <span className="min-w-0">
            <span className="text-muted block text-[11px]">Oldest open</span>
            <span className="block truncate font-medium">{oldest.title}</span>
          </span>
          <span
            className={cn('flex-none tabular-nums', days >= 3 ? 'text-negative' : 'text-muted')}
          >
            {days === 0 ? 'today' : `${days} d`}
          </span>
        </Link>
      )}
    </div>
  )
}
