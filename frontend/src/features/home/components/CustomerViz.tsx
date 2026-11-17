import { motion, useReducedMotion } from 'motion/react'
import { Link } from 'react-router'
import { caseDetailPath } from '@/app/paths'
import { CHIP_CLASSES, groupOf, groupStyle, type CaseSummary } from '@/features/cases'
import { EmptyBlock, Icon } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { formatDateTime, formatDuration, formatRelative } from '@/shared/utils/format'
import { EASE_SNAP } from '@/shared/utils/motion'

/*
 * Customer home pieces (17/11): a tenant asks "where is my request?" and "what keeps breaking in my space?".
 * The tracker reads like a parcel tracker; the donut shows what the tenant reported, by group.
 */

const STEPS = ['Reported', 'Approved', 'Work started', 'Fixed', 'Closed'] as const

/** Which step a request is on (0..4) and a word for the current moment. */
function stepOf(c: CaseSummary): { step: number; note: string } {
  switch (c.status) {
    case 'pending':
      return { step: 0, note: 'The building staff will review it' }
    case 'approved':
      return {
        step: 1,
        note: c.technician
          ? `${c.technician.fullName} will take it`
          : 'A technician is being chosen',
      }
    case 'accepted':
      return { step: 1, note: `${c.technician?.fullName ?? 'The technician'} accepted it` }
    case 'in_progress':
    case 'reopened':
      return { step: 2, note: `${c.technician?.fullName ?? 'A technician'} is on it` }
    case 'on_hold':
      return { step: 2, note: 'Paused for now' }
    case 'completed':
      return { step: 3, note: 'Fixed — the building checks the work' }
    default:
      return { step: 4, note: 'Closed' }
  }
}

/** One request as a five-step track: done steps filled, the current one pulsing. */
function Track({ c, index }: { c: CaseSummary; index: number }) {
  const reduce = useReducedMotion()
  const { step, note } = stepOf(c)
  const kind = groupStyle(groupOf(c.serviceTypeName))
  return (
    <li className="border-rule border-b py-4 first:pt-0 last:border-0 last:pb-0">
      <div className="flex items-start gap-3">
        <span
          className={cn(
            'grid size-9 flex-none place-items-center rounded-xl',
            CHIP_CLASSES[kind.hue],
          )}
        >
          <Icon name={kind.icon} className="size-[18px]" />
        </span>
        <div className="min-w-0 flex-1">
          <Link
            to={caseDetailPath(c.id)}
            className="hover:text-accent-ink block truncate text-[15px] font-semibold transition-colors"
          >
            {c.title}
          </Link>
          <div className="text-muted truncate text-[13px]">
            <span className="font-mono text-xs">{c.code}</span> · {c.location.roomName} · reported{' '}
            {formatRelative(c.submittedAt)}
          </div>
        </div>
      </div>

      {/* the track */}
      <div className="relative mt-4 px-1">
        <div className="bg-rule absolute top-[7px] right-3 left-3 h-1 rounded-full" />
        <motion.div
          className="bg-accent absolute top-[7px] left-3 h-1 origin-left rounded-full"
          style={{ width: `calc((100% - 24px) * ${step / (STEPS.length - 1)})` }}
          initial={reduce ? false : { scaleX: 0 }}
          animate={{ scaleX: 1 }}
          transition={{ duration: 0.8, ease: EASE_SNAP, delay: 0.2 + index * 0.1 }}
        />
        <ol className="relative grid grid-cols-5">
          {STEPS.map((label, i) => {
            const done = i < step
            const current = i === step
            return (
              <li key={label} className="flex flex-col items-center gap-1.5 text-center">
                <span className="relative grid size-[18px] place-items-center">
                  {current && !reduce && (
                    <span className="bg-accent/30 absolute inset-[-4px] animate-ping rounded-full" />
                  )}
                  <span
                    className={cn(
                      'relative size-[18px] rounded-full border-2',
                      done && 'bg-accent border-accent',
                      current && 'border-accent bg-surface',
                      !done && !current && 'border-control bg-surface',
                    )}
                  >
                    {done && (
                      <svg viewBox="0 0 16 16" className="text-on-accent size-full p-[2px]">
                        <path
                          d="M4 8.5 7 11l5-6"
                          fill="none"
                          stroke="currentColor"
                          strokeWidth="2"
                          strokeLinecap="round"
                          strokeLinejoin="round"
                        />
                      </svg>
                    )}
                    {current && <span className="bg-accent absolute inset-[3px] rounded-full" />}
                  </span>
                </span>
                <span
                  className={cn(
                    'text-[11px] leading-tight',
                    current ? 'text-ink font-semibold' : done ? 'text-muted' : 'text-faint',
                  )}
                >
                  {label}
                </span>
              </li>
            )
          })}
        </ol>
      </div>
      <div className="mt-3 flex flex-wrap items-center justify-between gap-2 text-[13px]">
        <span className="text-accent-ink font-medium">{note}</span>
        {c.dueAt && c.status !== 'completed' && (
          <span className="text-muted tabular-nums">fix due {formatDateTime(c.dueAt)}</span>
        )}
      </div>
    </li>
  )
}

export function RequestTracker({ cases }: { cases: CaseSummary[] }) {
  if (cases.length === 0) {
    return <EmptyBlock>Nothing in progress. Report a problem when something breaks.</EmptyBlock>
  }
  return (
    <ul className="flex flex-col">
      {cases.slice(0, 4).map((c, i) => (
        <Track key={c.id} c={c} index={i} />
      ))}
    </ul>
  )
}

// ---------- what breaks most ----------

const SLICE = [
  'var(--color-chip-orange)',
  'var(--color-chip-green)',
  'var(--color-chip-violet)',
  'var(--color-chip-blue)',
  'var(--color-control)',
]

/** Donut of the tenant's requests by group, the total in the middle; average time to fix under it. */
export function BreakdownDonut({ cases }: { cases: CaseSummary[] }) {
  const reduce = useReducedMotion()
  const counts = new Map<string, number>()
  for (const c of cases)
    counts.set(groupOf(c.serviceTypeName), (counts.get(groupOf(c.serviceTypeName)) ?? 0) + 1)
  const groups = [...counts.entries()].sort((a, b) => b[1] - a[1])
  const top = groups.slice(0, 4)
  const rest = groups.slice(4).reduce((n, [, v]) => n + v, 0)
  const slices = rest > 0 ? [...top, ['Other', rest] as [string, number]] : top
  const total = cases.length

  const fixed = cases.filter((c) => c.completedAt)
  const avgFix =
    fixed.length > 0
      ? fixed.reduce((n, c) => n + (Date.parse(c.completedAt!) - Date.parse(c.submittedAt)), 0) /
        fixed.length
      : null

  if (total === 0) return <EmptyBlock>No request yet.</EmptyBlock>

  const r = 52
  const circ = 2 * Math.PI * r
  // where each slice starts on the circle: the sum of the slices before it
  const starts = slices.map((_, i) =>
    slices.slice(0, i).reduce((sum, [, v]) => sum + (v / total) * circ, 0),
  )
  return (
    <div className="flex flex-col gap-5">
      <div className="flex items-center gap-5">
        <div className="relative size-[132px] flex-none">
          <svg viewBox="0 0 132 132" className="size-full -rotate-90" aria-hidden="true">
            <circle cx="66" cy="66" r={r} fill="none" stroke="var(--color-rule)" strokeWidth="16" />
            {slices.map(([name, v], i) => {
              const len = (v / total) * circ
              const dash = `${Math.max(0, len - 3)} ${circ}`
              return (
                <motion.circle
                  key={name}
                  cx="66"
                  cy="66"
                  r={r}
                  fill="none"
                  stroke={SLICE[i]}
                  strokeWidth="16"
                  strokeLinecap="butt"
                  strokeDasharray={dash}
                  strokeDashoffset={-starts[i]!}
                  initial={reduce ? false : { opacity: 0 }}
                  animate={{ opacity: 1 }}
                  transition={{ duration: 0.4, delay: 0.2 + i * 0.12 }}
                />
              )
            })}
          </svg>
          <div className="absolute inset-0 grid place-items-center text-center">
            <div>
              <div className="text-[26px] leading-7 font-semibold tabular-nums">{total}</div>
              <div className="text-muted text-[11px]">requests</div>
            </div>
          </div>
        </div>
        <ul className="flex min-w-0 flex-1 flex-col gap-2 text-[13px]">
          {slices.map(([name, v], i) => (
            <li key={name} className="flex items-center gap-2">
              <span className="size-2.5 flex-none rounded-full" style={{ background: SLICE[i] }} />
              <span className="min-w-0 flex-1 truncate">{name}</span>
              <span className="text-muted tabular-nums">{v}</span>
            </li>
          ))}
        </ul>
      </div>
      <div className="well flex items-center justify-between rounded-xl px-4 py-3">
        <div>
          <div className="text-muted text-[11px]">Average time to fix</div>
          <div className="text-[17px] font-semibold tabular-nums">
            {avgFix === null ? '—' : formatDuration(avgFix)}
          </div>
        </div>
        <Icon name="clock" className="text-faint size-5" />
      </div>
    </div>
  )
}
