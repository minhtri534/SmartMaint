import { Badge, type BadgeTone } from '@/shared/ui'
import { STATUS_LABELS, URGENCY_LABELS, type CaseStatus, type Urgency } from '../types'

/**
 * One hue at three intensities (app-screens §7.6): grey when nothing is happening, the accent tint while
 * work is running, warning when someone has to act, positive when it waits for the staff's check.
 */
const STATUS_TONES: Record<CaseStatus, BadgeTone> = {
  pending: 'warning',
  approved: 'neutral',
  accepted: 'accent',
  in_progress: 'accent',
  on_hold: 'warning',
  completed: 'positive',
  closed: 'neutral',
  reopened: 'negative',
  rejected: 'neutral',
  cancelled: 'neutral',
}

export function StatusBadge({ status }: { status: CaseStatus }) {
  return <Badge tone={STATUS_TONES[status]}>{STATUS_LABELS[status]}</Badge>
}

/** The urgency column of the case lists (meeting 07/11: its own column, not under the status). Words, no pill. */
export function UrgencyText({ urgency }: { urgency: Urgency }) {
  return (
    <span
      className={
        urgency === 'emergency'
          ? 'text-negative font-medium'
          : urgency === 'high'
            ? 'text-warning'
            : urgency === 'low'
              ? 'text-muted'
              : undefined
      }
    >
      {URGENCY_LABELS[urgency]}
    </span>
  )
}
