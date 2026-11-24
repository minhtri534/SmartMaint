import { Card, CardHeader } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { formatDateTime, formatDuration } from '@/shared/utils/format'
import { slaOf, type Sla } from '../sla'
import type { CaseDetail, CaseSummary } from '../types'

/*
 * SmartMaint's own element: every case carries its SLA as a track at the true ratio of the window used.
 * Teal while it runs, red only when overdue (the one red mark on a case), a quiet word once it is done.
 */

/** Before the work starts there is no deadline yet: the time it is expected, or when that will be known. */
function waitingText(c: CaseSummary) {
  if (c.expectedDueAt) return 'Expected due'
  return c.status === 'pending' ? 'Set when approved' : 'Set when scheduled'
}

function headline(sla: Sla, c: CaseSummary): { text: string; tone: string } {
  switch (sla.state) {
    case 'running':
      return { text: `${formatDuration(sla.remaining)} left`, tone: 'text-ink' }
    case 'overdue':
      return { text: `Overdue ${formatDuration(sla.remaining)}`, tone: 'text-negative font-medium' }
    case 'met':
      return { text: 'On time', tone: 'text-positive' }
    case 'late':
      return { text: `Late by ${formatDuration(sla.remaining)}`, tone: 'text-negative' }
    case 'waiting':
      return { text: waitingText(c), tone: 'text-muted' }
    default:
      return { text: '—', tone: 'text-muted' }
  }
}

function Track({ sla, className }: { sla: Sla; className?: string }) {
  if (sla.state === 'waiting' || sla.state === 'none') return null
  const fill =
    sla.state === 'overdue' || sla.state === 'late'
      ? 'bg-negative'
      : sla.state === 'met'
        ? 'bg-control'
        : 'bg-accent'
  return (
    <div className={cn('bg-rule h-1 w-full overflow-hidden rounded-full', className)}>
      {/* scaleX (not width) so a refresh moves the fill on the GPU */}
      <div
        className={cn(
          'ease-snap h-full origin-left rounded-full transition-transform duration-500',
          fill,
        )}
        style={{ transform: `scaleX(${Math.max(0.02, Math.min(1, sla.used))})` }}
      />
    </div>
  )
}

/**
 * Compact form for a table cell: label, track, deadline — right-aligned.
 * The track shows only while the clock runs (or finished on time): once a case is past its deadline the
 * red words are its one red mark, and a full red bar under every overdue row read as a stripe (16/11).
 */
export function SlaCell({ c }: { c: CaseSummary }) {
  const sla = slaOf(c)
  const head = headline(sla, c)
  const pastDeadline = sla.state === 'overdue' || sla.state === 'late'
  return (
    <div className="ml-auto flex w-36 flex-col items-end gap-1 text-right">
      <span className={cn('text-sm tabular-nums', head.tone)}>{head.text}</span>
      {!pastDeadline && <Track sla={sla} />}
      {c.dueAt && sla.state !== 'met' && (
        <span className="text-muted text-xs tabular-nums">Due {formatDateTime(c.dueAt)}</span>
      )}
      {c.expectedDueAt && sla.state === 'waiting' && (
        <span className="text-muted text-xs tabular-nums">{formatDateTime(c.expectedDueAt)}</span>
      )}
    </div>
  )
}

/** Inline form for a phone row: one line of words. */
export function SlaText({ c }: { c: CaseSummary }) {
  const sla = slaOf(c)
  const head = headline(sla, c)
  const expected = sla.state === 'waiting' && c.expectedDueAt
  return (
    <span className={cn('text-xs tabular-nums', head.tone)}>
      {head.text}
      {expected && ` ${formatDateTime(expected)}`}
    </span>
  )
}

/** Detail page: the case's answer figure ("3 h 10 m left") with the full window under it. */
export function SlaPanel({ c }: { c: CaseDetail }) {
  const sla = slaOf(c)
  const head = headline(sla, c)
  const expected = sla.state === 'waiting' ? c.expectedDueAt : null
  // Past the deadline the red figure is the case's one red mark: no full red bar under it (16/11).
  const pastDeadline = sla.state === 'overdue' || sla.state === 'late'

  return (
    <Card>
      <CardHeader
        title={expected ? 'Expected due' : 'Deadline'}
        aside={sla.window ? `${formatDuration(sla.window)} window` : undefined}
      />
      <div
        className={cn(
          'font-normal tracking-[-0.03em] tabular-nums',
          sla.state === 'waiting' ? 'text-2xl leading-8' : 'text-[34px] leading-10',
          head.tone,
        )}
      >
        {expected ? formatDateTime(expected) : head.text}
      </div>
      {!pastDeadline && <Track sla={sla} className="mt-3 h-1.5" />}
      <dl className="mt-4 flex flex-col gap-2 text-sm">
        {c.slaStartedAt && <Row label="Clock started" value={formatDateTime(c.slaStartedAt)} />}
        {expected && c.plannedStartAt && (
          <Row label="Planned start" value={formatDateTime(c.plannedStartAt)} />
        )}
        {c.dueAt && <Row label="Due" value={formatDateTime(c.dueAt)} />}
        {c.completedAt && <Row label="Reported done" value={formatDateTime(c.completedAt)} />}
      </dl>
    </Card>
  )
}

function Row({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-baseline justify-between gap-3">
      <dt className="text-muted">{label}</dt>
      <dd className="tabular-nums">{value}</dd>
    </div>
  )
}
