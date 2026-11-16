import { Badge } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { formatDateTime } from '@/shared/utils/format'
import { STATUS_LABELS, type CaseDetail, type CaseStatus } from '../types'

/*
 * The case's road through F3 (page 5 of the flow), read left to right. Branch statuses sit on the step
 * they belong to: On hold on "In progress", Reopened on "Approved" (meeting 07/11: no "Assigned" step, the
 * technician is chosen on approval); Rejected / Cancelled end the road
 * at the step where it stopped.
 */

const STEPS: { status: CaseStatus; label: string }[] = [
  { status: 'pending', label: 'Reported' },
  { status: 'approved', label: 'Approved' },
  { status: 'accepted', label: 'Accepted' },
  { status: 'in_progress', label: 'In progress' },
  { status: 'completed', label: 'Completed' },
  { status: 'closed', label: 'Closed' },
]

const STEP_OF: Partial<Record<CaseStatus, number>> = {
  pending: 0,
  approved: 1,
  reopened: 1,
  accepted: 2,
  in_progress: 3,
  on_hold: 3,
  completed: 4,
  closed: 5,
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
      <ol className="grid min-w-[560px] grid-cols-6">
        {STEPS.map((step, i) => {
          const done = i < current
          const here = i === current
          const at = done || here ? reachedAt(step.status) : undefined
          return (
            <li key={step.status} className="flex flex-col gap-1.5 pr-3">
              <div className="flex items-center">
                {/* When the case moves on, the dot recolours and the next segment fills from the left. */}
                <span
                  className={cn(
                    'size-2.5 flex-none rounded-full transition-[background-color,box-shadow] duration-300',
                    done && 'bg-ink',
                    here && !ended && 'bg-accent ring-accent/25 ring-4',
                    here && ended && 'bg-muted',
                    !done && !here && 'border-control bg-surface border-[1.5px]',
                  )}
                />
                {i < STEPS.length - 1 && (
                  <span className="bg-rule h-0.5 flex-1 overflow-hidden">
                    <span
                      className={cn(
                        'bg-ink ease-glide block h-full origin-left transition-transform duration-500',
                        done ? 'scale-x-100' : 'scale-x-0',
                      )}
                    />
                  </span>
                )}
              </div>
              <span
                className={cn(
                  'text-sm',
                  here ? 'text-ink font-semibold' : done ? 'text-ink' : 'text-faint',
                )}
              >
                {c.isPreventive && step.status === 'pending' ? 'Planned' : step.label}
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
