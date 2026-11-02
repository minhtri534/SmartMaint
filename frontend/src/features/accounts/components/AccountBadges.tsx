import { Badge } from '@/shared/ui'
import type { AccountSummary } from '../types'

/** One pill per row (U4): Locked wins over Deactivated wins over Active. */
export function AccountStatusBadge({
  account,
}: {
  account: Pick<AccountSummary, 'status' | 'isLocked'>
}) {
  if (account.isLocked) return <Badge tone="warning">Locked</Badge>
  if (account.status === 'deactivated') return <Badge>Deactivated</Badge>
  return <Badge tone="positive">Active</Badge>
}
