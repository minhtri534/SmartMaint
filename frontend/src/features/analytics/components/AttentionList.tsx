import { Link } from 'react-router'
import { paths } from '@/app/paths'
import { assetDetailPath, caseDetailPath, planDetailPath } from '@/app/paths'
import { Card, EmptyBlock, ErrorBlock, Icon, LoadingBlock, PanelHeader } from '@/shared/ui'
import { useAttention } from '../hooks'
import type { AttentionItem, AttentionKind } from '../types'

const KIND_LABELS: Record<AttentionKind, string> = {
  overdue_case: 'Overdue',
  long_hold: 'On hold',
  preventive_overdue: 'Maintenance late',
  frequent_failure: 'Fails often',
  extended_twice: 'Deadline moved',
  technician_declines: 'Declines',
  no_technician: 'No technician',
  not_fixed_report: 'Not fixed',
  emergency_not_acknowledged: 'Emergency',
}

function target(i: AttentionItem): string | null {
  if (i.caseId) return caseDetailPath(i.caseId)
  if (i.planId) return planDetailPath(i.planId)
  if (i.assetId) return assetDetailPath(i.assetId)
  return null
}

/**
 * F6 step ④ "Needs attention" (staff and managers), newest first: every case past its deadline, on hold over 3 days,
 * late preventive work, an asset repaired 3 times in 30 days, a deadline moved twice, a technician declining 3 jobs in a
 * week, a case left without a technician, a customer's "not fixed" report, an emergency nobody acknowledged
 * (meeting note 18/11). Shown only: F2 / F3 / F4 already sent their notifications. A home shows the first few and
 * links to the full list.
 */
export function AttentionList({ limit, viewAll }: { limit?: number; viewAll?: boolean }) {
  const attention = useAttention()
  const items = attention.data ?? []
  const shown = limit ? items.slice(0, limit) : items

  return (
    <Card>
      <PanelHeader
        title="Needs attention"
        help="Cases and assets that need a look now: past the deadline, on hold over 3 days, late maintenance, an asset that fails often, a deadline moved twice, a technician declining often, no technician, a customer's not-fixed report, an emergency nobody acknowledged. Newest first."
        aside={
          viewAll && items.length > 0 ? (
            <Link
              to={`${paths.analytics}?tab=attention`}
              className="text-muted hover:text-ink flex items-center gap-1 text-xs font-medium"
            >
              View all {items.length} <Icon name="arrowRight" className="size-3.5" />
            </Link>
          ) : (
            attention.data && <span className="text-muted text-xs">{`${items.length} now`}</span>
          )
        }
      />
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
      {limit && !viewAll && items.length > limit && (
        <p className="text-muted mt-3 text-xs">and {items.length - limit} more</p>
      )}
    </Card>
  )
}
