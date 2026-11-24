import { Badge, type BadgeTone } from '@/shared/ui'
import { PLAN_STATUS_LABELS, type PlanStatus } from '../types'

const TONES: Record<PlanStatus, BadgeTone> = {
  pending_approval: 'warning',
  active: 'positive',
  rejected: 'negative',
  paused: 'neutral',
  archived: 'neutral',
}

/** A running or paused plan past every end day reads "Expired" (it can be extended); archived is for good. */
export function PlanStatusBadge({
  status,
  expired = false,
}: {
  status: PlanStatus
  expired?: boolean
}) {
  if (expired) return <Badge tone="warning">Expired</Badge>
  return <Badge tone={TONES[status]}>{PLAN_STATUS_LABELS[status]}</Badge>
}
