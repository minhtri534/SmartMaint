import { paths } from '@/app/paths'
import { LinkButton, PageHeader } from '@/shared/ui'
import { CaseListCard, type CaseTab } from '../components/CaseListCard'
import { MockDataBanner } from '../components/MockDataBanner'
import { useCases } from '../hooks'
import { FINAL_STATUSES, type CaseSummary } from '../types'

const isOpen = (c: CaseSummary) => !FINAL_STATUSES.includes(c.status)

const TABS = [
  { value: 'open', label: 'Open', match: isOpen, emptyText: 'You have no open requests.' },
  {
    value: 'finished',
    label: 'Finished',
    match: (c) => !isOpen(c),
    emptyText: 'No finished requests yet.',
  },
  {
    value: 'all',
    label: 'All',
    match: () => true,
    emptyText: 'You have not reported anything yet.',
  },
] as const satisfies readonly [CaseTab, ...CaseTab[]]

/** EMP-03 My requests (customer). Question: "What is happening with what I reported?" */
export function MyRequestsPage() {
  const cases = useCases()
  const open = cases.data?.filter(isOpen) ?? []
  const inWork = open.filter((c) => c.technician).length

  return (
    <div className="flex flex-col gap-5">
      <MockDataBanner />
      <PageHeader
        title="My requests"
        description={
          cases.data
            ? `${open.length} open, ${inWork} with a technician`
            : 'Problems you reported in your building'
        }
        actions={
          <LinkButton to={paths.newRequest} variant="primary">
            New request
          </LinkButton>
        }
      />
      <CaseListCard tabs={TABS} showTechnician />
    </div>
  )
}
