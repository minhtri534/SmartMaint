import { Card, CardHeader } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { formatDateTime, formatDuration } from '@/shared/utils/format'
import { slaOf, type Sla } from '../sla'
import type { CaseDetail, CaseSummary } from '../types'

/*
 * SmartMaint's own element: every case carries its SLA as a track at the true ratio of the window used.
 * Teal while it runs, red only when overdue (the one red mark on a case), a quiet word once it is done.
 */

function headline(sla: Sla): { text: string; tone: string } {
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
      return { text: 'Starts with the work', tone: 'text-muted' }
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
      <div
        className={cn('h-full rounded-full', fill)}
        style={{ width: `${Math.max(2, Math.round(sla.used * 100))}%` }}
      />
    </div>
  )
}

/** Compact form for a table cell: label, track, deadline — right-aligned. */
export function SlaCell({ c }: { c: CaseSummary }) {
  const sla = slaOf(c)
  const head = headline(sla)
  return (
    <div className="ml-auto flex w-36 flex-col items-end gap-1 text-right">
      <span className={cn('text-sm tabular-nums', head.tone)}>{head.text}</span>
      <Track sla={sla} />
      {c.dueAt && sla.state !== 'met' && (
        <span className="text-muted text-xs tabular-nums">Due {formatDateTime(c.dueAt)}</span>
      )}
    </div>
  )
}

/** Inline form for a phone row: one line of words. */
export function SlaText({ c }: { c: CaseSummary }) {
  const head = headline(slaOf(c))
  return <span className={cn('text-xs tabular-nums', head.tone)}>{head.text}</span>
}

/** Detail page: the case's answer figure ("3 h 10 m left") with the full window under it. */
export function SlaPanel({ c }: { c: CaseDetail }) {
  const sla = slaOf(c)
  const head = headline(sla)

  return (
    <Card>
      <CardHeader
        title="Deadline"
        aside={sla.window ? `${formatDuration(sla.window)} window` : undefined}
      />
      <div className={cn('text-kpi tabular-nums', head.tone, 'font-normal')}>{head.text}</div>
      <Track sla={sla} className="mt-3 h-1.5" />
      <dl className="mt-4 flex flex-col gap-2 text-sm">
        {c.slaStartedAt && <Row label="Clock started" value={formatDateTime(c.slaStartedAt)} />}
        <Row label="Due" value={c.dueAt ? formatDateTime(c.dueAt) : 'Starts when work starts'} />
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
