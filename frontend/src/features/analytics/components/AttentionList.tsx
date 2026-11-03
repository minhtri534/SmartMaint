import { Link } from 'react-router'
import { assetDetailPath, caseDetailPath, planDetailPath } from '@/app/paths'
import { Card, CardHeader, EmptyBlock, ErrorBlock, LoadingBlock } from '@/shared/ui'
import { useAttention } from '../hooks'
import type { AttentionItem, AttentionKind } from '../types'

const KIND_LABELS: Record<AttentionKind, string> = {
  overdue_case: 'Overdue',
  long_hold: 'On hold',
  preventive_overdue: 'Maintenance late',
  frequent_failure: 'Fails often',
  extended_twice: 'Deadline moved',
  technician_declines: 'Declines',
}

function target(i: AttentionItem): string | null {
  if (i.caseId) return caseDetailPath(i.caseId)
  if (i.planId) return planDetailPath(i.planId)
  if (i.assetId) return assetDetailPath(i.assetId)
  return null
}

/**
 * F6 step ④ "Needs attention" (staff and managers): over 24 h late, on hold over 3 days, late preventive work,
 * an asset repaired 3 times in 30 days, a deadline moved twice, a technician declining 3 jobs in a week.
 * Shown only: F2 / F3 / F4 already sent their notifications.
 */
export function AttentionList({ limit }: { limit?: number }) {
  const attention = useAttention()
  const items = attention.data ?? []
  const shown = limit ? items.slice(0, limit) : items

  return (
    <Card>
      <CardHeader title="Needs attention" aside={attention.data && `${items.length} now`} />
      {attention.isPending ? (
        <LoadingBlock rows={3} />
      ) : attention.error ? (
        <ErrorBlock
          what="the list"
          message={attention.error.message}
          onRetry={() => void attention.refetch()}
        />
      ) : items.length === 0 ? (
        <EmptyBlock>Nothing needs attention.</EmptyBlock>
      ) : (
        <ul className="flex flex-col">
          {shown.map((i, n) => {
            const to = target(i)
            return (
              <li
                key={`${i.kind}-${i.caseId ?? i.assetId ?? i.technicianId ?? n}`}
                className="border-rule flex items-baseline gap-3 border-b py-2.5 text-sm first:pt-0 last:border-0 last:pb-0"
              >
                <span className="text-muted w-28 flex-none text-xs">{KIND_LABELS[i.kind]}</span>
                {to ? (
                  <Link to={to} className="min-w-0 underline-offset-2 hover:underline">
                    {i.text}
                  </Link>
                ) : (
                  <span className="min-w-0">{i.text}</span>
                )}
              </li>
            )
          })}
        </ul>
      )}
      {limit && items.length > limit && (
        <p className="text-muted mt-3 text-xs">and {items.length - limit} more</p>
      )}
    </Card>
  )
}
