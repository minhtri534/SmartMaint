import { Badge, type BadgeTone } from '@/shared/ui'
import { CHANGE_LIST_STATUS_LABELS, type ChangeListStatus } from '../types'

const TONES: Record<ChangeListStatus, BadgeTone> = {
  draft: 'neutral',
  pending: 'warning',
  approved: 'positive',
  rejected: 'negative',
}

export function ChangeListStatusBadge({ status }: { status: ChangeListStatus }) {
  return <Badge tone={TONES[status]}>{CHANGE_LIST_STATUS_LABELS[status]}</Badge>
}
