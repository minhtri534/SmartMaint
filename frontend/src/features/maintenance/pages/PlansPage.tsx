import { Link, useSearchParams } from 'react-router'
import { paths, planDetailPath } from '@/app/paths'
import { useAuth } from '@/features/auth'
import { EmptyBlock, ErrorBlock, LinkButton, LoadingBlock, PageHeader, Tabs } from '@/shared/ui'
import { formatDate } from '@/shared/utils/format'
import { PlanStatusBadge } from '../components/PlanStatusBadge'
import { usePlans } from '../hooks'
import { everyText, type PlanStatus, type PlanSummary } from '../types'

const TABS = [
  { value: 'running', label: 'Running', statuses: ['active'] },
  { value: 'waiting', label: 'Waiting for approval', statuses: ['pending_approval'] },
  { value: 'stopped', label: 'Paused or rejected', statuses: ['paused', 'rejected'] },
  { value: 'ended', label: 'Ended', statuses: ['ended'] },
] as const satisfies readonly { value: string; label: string; statuses: readonly PlanStatus[] }[]

/**
 * PM-01 Preventive maintenance plans of the caller's buildings. Staff make and run them; the Manager of the building
 * approves (E10). The list opens on what needs a look: the Manager on "Waiting for approval".
 */
export function PlansPage() {
  const { user } = useAuth()
  const plans = usePlans()
  const [params, setParams] = useSearchParams()
  const fallback = user?.role === 'manager' ? 'waiting' : 'running'
  const tab =
    TABS.find((t) => t.value === params.get('tab')) ?? TABS.find((t) => t.value === fallback)!

  const inTab = (t: (typeof TABS)[number], p: PlanSummary) =>
    (t.statuses as readonly PlanStatus[]).includes(p.status)
  const rows = plans.data?.filter((p) => inTab(tab, p)) ?? []
  const waiting = plans.data?.filter((p) => p.status === 'pending_approval').length ?? 0

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title="Maintenance plans"
        description={
          plans.data &&
          `${plans.data.filter((p) => p.status === 'active').length} running, ${waiting} waiting for approval.`
        }
        actions={
          user?.role === 'staff' && (
            <LinkButton variant="primary" to={paths.planNew}>
              New plan
            </LinkButton>
          )
        }
      />
      <Tabs
        items={TABS.map((t) => ({
          value: t.value,
          label: t.label,
          count: plans.data?.filter((p) => inTab(t, p)).length,
        }))}
        value={tab.value}
        onChange={(v) => setParams({ tab: v }, { replace: true })}
      />
      {plans.isPending ? (
        <LoadingBlock rows={5} />
      ) : plans.error ? (
        <ErrorBlock
          what="the plans"
          message={plans.error.message}
          onRetry={() => void plans.refetch()}
        />
      ) : rows.length === 0 ? (
        <EmptyBlock>No plan here.</EmptyBlock>
      ) : (
        <table className="w-full text-left text-sm">
          <thead>
            <tr className="border-border text-muted border-b text-xs">
              <th className="h-9 pr-4 font-normal">Plan</th>
              <th className="hidden h-9 pr-4 font-normal md:table-cell">Made by</th>
              <th className="h-9 pr-4 font-normal">Status</th>
              <th className="h-9 text-right font-normal">Next due</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((p) => (
              <tr key={p.id} className="border-rule border-b align-top last:border-0">
                <td className="max-w-[380px] py-3 pr-4">
                  <Link
                    to={planDetailPath(p.id)}
                    className="text-ink block truncate font-medium underline-offset-2 hover:underline"
                  >
                    {p.name}
                  </Link>
                  <div className="text-muted mt-0.5 text-[13px]">
                    {everyText(p.frequency, p.frequencyType)} · {p.assetCount}{' '}
                    {p.assetCount === 1 ? 'asset' : 'assets'}
                  </div>
                </td>
                <td className="hidden py-3 pr-4 whitespace-nowrap md:table-cell">
                  {p.staff.fullName}
                  {p.approvingManager && (
                    <div className="text-muted mt-0.5 text-[13px]">
                      approved by {p.approvingManager.fullName}
                    </div>
                  )}
                </td>
                <td className="py-3 pr-4">
                  <PlanStatusBadge status={p.status} />
                </td>
                <td className="py-3 text-right whitespace-nowrap tabular-nums">
                  {p.nextDueDate ? (
                    formatDate(p.nextDueDate)
                  ) : (
                    <span className="text-muted">—</span>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}
