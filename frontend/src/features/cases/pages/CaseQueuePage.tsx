import { useAuth } from '@/features/auth'
import { PageHeader } from '@/shared/ui'
import { CaseListCard, type CaseTab } from '../components/CaseListCard'
import { MockDataBanner } from '../components/MockDataBanner'
import { FINAL_STATUSES, type CaseStatus } from '../types'

const IN_PROGRESS: readonly CaseStatus[] = [
  'assigned',
  'accepted',
  'in_progress',
  'on_hold',
  'reopened',
]

/** Tabs follow the officer's work in F3: review -> assign -> follow up -> verify. */
const TABS = [
  {
    value: 'review',
    label: 'To review',
    match: (c) => c.status === 'pending' && !c.isEmergencyFlow,
    emptyText: 'No new requests to review.',
  },
  {
    value: 'assign',
    label: 'To assign',
    match: (c) => c.status === 'approved' && !c.isEmergencyFlow,
    emptyText: 'Every approved case has a technician.',
  },
  {
    value: 'progress',
    label: 'In progress',
    match: (c) => IN_PROGRESS.includes(c.status),
    emptyText: 'Nothing in progress.',
  },
  {
    value: 'verify',
    label: 'To verify',
    match: (c) => c.status === 'completed',
    emptyText: 'No completed work waiting for your check.',
  },
  {
    value: 'finished',
    label: 'Finished',
    match: (c) => FINAL_STATUSES.includes(c.status),
    emptyText: 'No finished cases.',
  },
  { value: 'all', label: 'All', match: () => true, emptyText: 'No cases.' },
] as const satisfies readonly [CaseTab, ...CaseTab[]]

/**
 * OFF-02 Case queue + OFF-04 Verification queue (officer: only the buildings in charge, N11)
 * and OFF-03 All cases (manager: read only).
 */
export function CaseQueuePage() {
  const { user } = useAuth()
  const isManager = user?.role === 'manager'

  return (
    <div className="flex flex-col gap-4">
      <MockDataBanner />
      <PageHeader
        title={isManager ? 'All cases' : 'Cases in my buildings'}
        description={
          isManager
            ? 'Read only. Follow progress, deadlines and technician declines.'
            : 'Review new requests, assign technicians and verify finished work.'
        }
      />
      <CaseListCard tabs={TABS} showRequester showTechnician />
    </div>
  )
}
