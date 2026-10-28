import { useAuth } from '@/features/auth'
import { PageHeader } from '@/shared/ui'
import { CaseListCard, type CaseTab } from '../components/CaseListCard'
import { MockDataBanner } from '../components/MockDataBanner'
import { useCases } from '../hooks'
import { isOverdue } from '../sla'
import { FINAL_STATUSES, type CaseStatus, type CaseSummary } from '../types'

const WITH_TECHNICIANS: readonly CaseStatus[] = [
  'assigned',
  'accepted',
  'in_progress',
  'on_hold',
  'reopened',
]

// The officer's own steps in F3: review (④), assign (⑤), verify (⑩).
const toReview = (c: CaseSummary) => c.status === 'pending' && !c.isEmergencyFlow
const toAssign = (c: CaseSummary) => c.status === 'approved' && !c.isEmergencyFlow
const toVerify = (c: CaseSummary) => c.status === 'completed'
/** Waiting for the officer, or past its deadline wherever it is. */
const needsYou = (c: CaseSummary) => toReview(c) || toAssign(c) || toVerify(c) || isOverdue(c)

/** Overdue first, then the officer's steps in flow order, oldest first. */
const STEP_ORDER: Partial<Record<CaseStatus, number>> = { pending: 0, approved: 1, completed: 2 }
const byUrgencyOfWork = (a: CaseSummary, b: CaseSummary) =>
  Number(isOverdue(b)) - Number(isOverdue(a)) ||
  (STEP_ORDER[a.status] ?? 3) - (STEP_ORDER[b.status] ?? 3) ||
  a.submittedAt.localeCompare(b.submittedAt)

function countLine(rows: CaseSummary[]) {
  const parts = [
    [rows.filter(toReview).length, 'review'],
    [rows.filter(toAssign).length, 'assign'],
    [rows.filter(toVerify).length, 'verify'],
  ] as const
  return parts
    .filter(([n]) => n > 0)
    .map(([n, word]) => `${n} ${word}`)
    .join(' · ')
}

const TABS = [
  {
    value: 'needs-you',
    label: 'Needs you',
    match: needsYou,
    sort: byUrgencyOfWork,
    note: countLine,
    emptyText: 'Nothing waits for you. New requests appear here.',
  },
  {
    value: 'with-technicians',
    label: 'With technicians',
    match: (c) => WITH_TECHNICIANS.includes(c.status),
    sort: byUrgencyOfWork,
    emptyText: 'No technician is working on a case.',
  },
  {
    value: 'closed',
    label: 'Closed',
    match: (c) => FINAL_STATUSES.includes(c.status),
    emptyText: 'No closed cases.',
  },
  { value: 'all', label: 'All', match: () => true, emptyText: 'No cases yet.' },
] as const satisfies readonly [CaseTab, ...CaseTab[]]

/**
 * OFF-02 Case queue + OFF-04 Verification queue (officer: only the buildings in charge, N11) and
 * OFF-03 All cases (manager: read only). Question: "Which cases need me now?"
 */
export function CaseQueuePage() {
  const { user } = useAuth()
  const cases = useCases()
  const isManager = user?.role === 'manager'

  const data = cases.data ?? []
  const reviewCount = data.filter(toReview).length
  const overdue = data.filter((c) => isOverdue(c)).length
  const finding = cases.data
    ? [
        `${reviewCount} new ${reviewCount === 1 ? 'request' : 'requests'} to review`,
        overdue > 0 ? `${overdue} past the deadline` : 'nothing past the deadline',
      ].join(', ')
    : undefined

  return (
    <div className="flex flex-col gap-5">
      <MockDataBanner />
      <PageHeader title={isManager ? 'All cases' : 'Cases'} description={finding} />
      <CaseListCard tabs={TABS} variant="cards" showRequester showTechnician />
    </div>
  )
}
