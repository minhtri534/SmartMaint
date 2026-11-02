import { Badge } from '@/shared/ui'
import type { AssetSummary } from '../types'

/** One pill per asset (U4): retired > open cases > in service. */
export function AssetStatus({ a }: { a: Pick<AssetSummary, 'retiredAt' | 'openCases'> }) {
  if (a.retiredAt) return <Badge>Retired</Badge>
  if (a.openCases > 0)
    return (
      <Badge tone="warning">
        {a.openCases === 1 ? '1 open case' : `${a.openCases} open cases`}
      </Badge>
    )
  return <Badge tone="positive">In service</Badge>
}
