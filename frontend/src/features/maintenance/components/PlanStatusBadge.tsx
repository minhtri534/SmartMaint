import { Badge, type BadgeTone } from '@/shared/ui'
import { PLAN_STATUS_LABELS, type PlanStatus } from '../types'

const TONES: Record<PlanStatus, BadgeTone> = {
  pending_approval: 'warning',
  active: 'positive',
  rejected: 'negative',
  paused: 'neutral',
  ended: 'neutral',
}

export function PlanStatusBadge({ status }: { status: PlanStatus }) {
  return <Badge tone={TONES[status]}>{PLAN_STATUS_LABELS[status]}</Badge>
}
