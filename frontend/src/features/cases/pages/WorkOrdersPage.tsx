import { PageHeader } from '@/shared/ui'
import { CaseListCard, type CaseTab } from '../components/CaseListCard'
import { MockDataBanner } from '../components/MockDataBanner'
import { useActor } from '../hooks'
import type { CaseStatus, CaseSummary } from '../types'

const ACTIVE: readonly CaseStatus[] = ['accepted', 'in_progress', 'on_hold']

/** TEC-02 My work orders (technician). Only cases currently assigned to me. */
export function WorkOrdersPage() {
  const actor = useActor()
  const mine = (c: CaseSummary) => c.technician?.id === actor?.id

  const tabs = [
    {
      value: 'new',
      label: 'New',
      match: (c) => mine(c) && (c.status === 'assigned' || c.status === 'reopened'),
      emptyText: 'No new jobs waiting for your answer.',
    },
    {
      value: 'active',
      label: 'Active',
      match: (c) => mine(c) && ACTIVE.includes(c.status),
      emptyText: 'No active jobs.',
    },
    {
      value: 'done',
      label: 'Done',
      match: (c) => mine(c) && (c.status === 'completed' || c.status === 'closed'),
      emptyText: 'No finished jobs yet.',
    },
  ] as const satisfies readonly [CaseTab, ...CaseTab[]]

  return (
    <div className="flex flex-col gap-4">
      <MockDataBanner />
      <PageHeader
        title="My work orders"
        description="Accept new jobs, record the repair and report when you are done."
      />
      <CaseListCard tabs={tabs} showRequester />
    </div>
  )
}
