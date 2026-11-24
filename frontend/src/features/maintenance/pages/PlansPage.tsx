import { useState } from 'react'
import { Link, useSearchParams } from 'react-router'
import { paths, planDetailPath } from '@/app/paths'
import { useAuth } from '@/features/auth'
import {
  EmptyBlock,
  ErrorBlock,
  LinkButton,
  LoadingBlock,
  PageHeader,
  SearchInput,
  SortHeader,
  sortRows,
  Tabs,
  useTableSort,
} from '@/shared/ui'
import { formatDate } from '@/shared/utils/format'
import { PlanStatusBadge } from '../components/PlanStatusBadge'
import { usePlans } from '../hooks'
import { everyListText, type PlanStatus, type PlanSummary } from '../types'

/** Meeting note 18/11: an expired plan (every end day passed) has its own tab, apart from the archived ones. */
const TABS = [
  { value: 'running', label: 'Running', statuses: ['active'], expired: false },
  {
    value: 'waiting',
    label: 'Waiting for approval',
    statuses: ['pending_approval'],
    expired: false,
  },
  {
    value: 'stopped',
    label: 'Paused or rejected',
    statuses: ['paused', 'rejected'],
    expired: false,
  },
  { value: 'expired', label: 'Expired', statuses: ['active', 'paused'], expired: true },
  { value: 'archived', label: 'Archived', statuses: ['archived'], expired: false },
] as const satisfies readonly {
  value: string
  label: string
  statuses: readonly PlanStatus[]
  expired: boolean
}[]

/**
 * PM-01 Preventive maintenance plans of the caller's buildings, each with one or more triggers (like UpKeep). Staff make and run them; the Manager of the building
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
    (t.statuses as readonly PlanStatus[]).includes(p.status) && p.expired === t.expired
  const [search, setSearch] = useState('')
  const { sort, toggle } = useTableSort<'name' | 'staff' | 'next'>()
  const words = search.trim().toLowerCase()
  const rows = sortRows(
    plans.data?.filter(
      (p) =>
        inTab(tab, p) &&
        (!words ||
          p.name.toLowerCase().includes(words) ||
          p.staff.fullName.toLowerCase().includes(words)),
    ) ?? [],
    sort,
    { name: (p) => p.name, staff: (p) => p.staff.fullName, next: (p) => p.nextDueDate },
  )
  const waiting = plans.data?.filter((p) => p.status === 'pending_approval').length ?? 0

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title="Maintenance plans"
        description={
          plans.data &&
          `${plans.data.filter((p) => p.status === 'active' && !p.expired).length} running, ${waiting} waiting for approval.`
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
      <SearchInput
        label="Search plans"
        placeholder="Plan or staff member"
        value={search}
        onChange={(e) => setSearch(e.target.value)}
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
              <SortHeader sortKey="name" sort={sort} onSort={toggle}>
                Plan
              </SortHeader>
              <SortHeader
                sortKey="staff"
                sort={sort}
                onSort={toggle}
                className="hidden md:table-cell"
              >
                Made by
              </SortHeader>
              <th className="h-9 pr-4 font-normal">Status</th>
              <SortHeader sortKey="next" sort={sort} onSort={toggle} align="right" className="pr-0">
                Next due
              </SortHeader>
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
                    {everyListText(p.every)} · {p.assetCount}{' '}
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
                  <PlanStatusBadge status={p.status} expired={p.expired} />
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
