import { Badge, type BadgeTone } from '@/shared/ui'
import { STATUS_LABELS, URGENCY_LABELS, type CaseStatus, type Urgency } from '../types'

const STATUS_TONES: Record<CaseStatus, BadgeTone> = {
  pending: 'warning',
  approved: 'info',
  assigned: 'info',
  accepted: 'info',
  in_progress: 'info',
  on_hold: 'warning',
  completed: 'success',
  closed: 'neutral',
  reopened: 'danger',
  rejected: 'neutral',
  cancelled: 'neutral',
}

const URGENCY_TONES: Record<Urgency, BadgeTone> = {
  emergency: 'danger',
  high: 'warning',
  normal: 'neutral',
  low: 'neutral',
}

export function StatusBadge({ status }: { status: CaseStatus }) {
  return <Badge tone={STATUS_TONES[status]}>{STATUS_LABELS[status]}</Badge>
}

export function UrgencyBadge({ urgency }: { urgency: Urgency }) {
  return <Badge tone={URGENCY_TONES[urgency]}>{URGENCY_LABELS[urgency]}</Badge>
}
