import { paths } from '@/app/paths'
import { LinkButton, PageHeader } from '@/shared/ui'
import { CaseListCard, type CaseTab } from '../components/CaseListCard'
import { MockDataBanner } from '../components/MockDataBanner'
import { FINAL_STATUSES } from '../types'

const TABS = [
  {
    value: 'open',
    label: 'Open',
    match: (c) => !FINAL_STATUSES.includes(c.status),
    emptyText: 'You have no open requests.',
  },
  {
    value: 'closed',
    label: 'Finished',
    match: (c) => FINAL_STATUSES.includes(c.status),
    emptyText: 'No finished requests yet.',
  },
  {
    value: 'all',
    label: 'All',
    match: () => true,
    emptyText: 'You have not reported anything yet.',
  },
] as const satisfies readonly [CaseTab, ...CaseTab[]]

/** EMP-03 My requests (customer). */
export function MyRequestsPage() {
  return (
    <div className="flex flex-col gap-4">
      <MockDataBanner />
      <PageHeader
        title="My requests"
        description="Problems you reported in your building, and where each one is now."
        actions={<LinkButton to={paths.newRequest}>New request</LinkButton>}
      />
      <CaseListCard tabs={TABS} showTechnician />
    </div>
  )
}
