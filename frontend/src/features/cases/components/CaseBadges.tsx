import { Badge, type BadgeTone } from '@/shared/ui'
import { STATUS_LABELS, URGENCY_LABELS, type CaseStatus, type Urgency } from '../types'

/**
 * One hue at three intensities (app-screens §7.6): grey when nothing is happening, the accent tint while
 * work is running, warning when someone has to act, positive when it waits for the staff's check.
 */
const STATUS_TONES: Record<CaseStatus, BadgeTone> = {
  pending: 'warning',
  approved: 'neutral',
  assigned: 'neutral',
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

/** Urgency is printed only above normal, as words (not a second pill on the row). */
export function UrgencyNote({ urgency }: { urgency: Urgency }) {
  if (urgency === 'normal' || urgency === 'low') return null
  return (
    <span
      className={
        urgency === 'emergency' ? 'text-negative text-xs font-medium' : 'text-warning text-xs'
      }
    >
      {URGENCY_LABELS[urgency]} urgency
    </span>
  )
}
