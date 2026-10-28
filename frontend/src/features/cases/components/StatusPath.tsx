import { Badge } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { formatDateTime } from '@/shared/utils/format'
import { STATUS_LABELS, type CaseDetail, type CaseStatus } from '../types'

/*
 * The case's road through F3 (page 5 of the flow), read left to right. Branch statuses sit on the step
 * they belong to: On hold on "In progress", Reopened on "Assigned"; Rejected / Cancelled end the road
 * at the step where it stopped.
 */

const STEPS: { status: CaseStatus; label: string }[] = [
  { status: 'pending', label: 'Reported' },
  { status: 'approved', label: 'Approved' },
  { status: 'assigned', label: 'Assigned' },
  { status: 'accepted', label: 'Accepted' },
  { status: 'in_progress', label: 'In progress' },
  { status: 'completed', label: 'Completed' },
  { status: 'closed', label: 'Closed' },
]

const STEP_OF: Partial<Record<CaseStatus, number>> = {
  pending: 0,
  approved: 1,
  assigned: 2,
  reopened: 2,
  accepted: 3,
  in_progress: 4,
  on_hold: 4,
  completed: 5,
  closed: 6,
}

export function StatusPath({ c }: { c: CaseDetail }) {
  const ended = c.status === 'rejected' || c.status === 'cancelled'
  // Where a rejected / cancelled case stopped: the status it left.
  const stoppedFrom = ended ? c.history.at(-1)?.from : null
  const current = ended ? (STEP_OF[stoppedFrom ?? 'pending'] ?? 0) : (STEP_OF[c.status] ?? 0)
  const branch = c.status === 'on_hold' || c.status === 'reopened' || ended ? c.status : null

  // The last time the case reached each step (a reopened case reaches some steps twice).
  const reachedAt = (s: CaseStatus) => c.history.findLast((h) => h.to === s)?.at

  return (
    <div className="overflow-x-auto">
      <ol className="grid min-w-[640px] grid-cols-7">
        {STEPS.map((step, i) => {
          const done = i < current
          const here = i === current
          const at = done || here ? reachedAt(step.status) : undefined
          return (
            <li key={step.status} className="flex flex-col gap-1.5 pr-3">
              <div className="flex items-center">
                <span
                  className={cn(
                    'size-2.5 flex-none rounded-full',
                    done && 'bg-ink',
                    here && !ended && 'bg-accent ring-accent/25 ring-4',
                    here && ended && 'bg-muted',
                    !done && !here && 'border-control bg-surface border-[1.5px]',
                  )}
                />
                {i < STEPS.length - 1 && (
                  <span className={cn('h-0.5 flex-1', done ? 'bg-ink' : 'bg-rule')} />
                )}
              </div>
              <span
                className={cn(
                  'text-sm',
                  here ? 'text-ink font-semibold' : done ? 'text-ink' : 'text-faint',
                )}
              >
                {step.label}
              </span>
              {here && branch ? (
                <span>
                  <Badge
                    tone={
                      branch === 'reopened'
                        ? 'negative'
                        : branch === 'on_hold'
                          ? 'warning'
                          : 'neutral'
                    }
                  >
                    {STATUS_LABELS[branch]}
                  </Badge>
                </span>
              ) : (
                at && <span className="text-muted text-xs tabular-nums">{formatDateTime(at)}</span>
              )}
            </li>
          )
        })}
      </ol>
    </div>
  )
}
