import { useAuth } from '@/features/auth'
import { PageHeader } from '@/shared/ui'
import { CaseListCard, type CaseTab } from '../components/CaseListCard'
import { useCases } from '../hooks'
import { isOverdue } from '../sla'
import { FINAL_STATUSES, type CaseStatus, type CaseSummary } from '../types'

const HANDLING: readonly CaseStatus[] = [
  'approved',
  'accepted',
  'in_progress',
  'on_hold',
  'reopened',
]

const isDuplicate = (c: CaseSummary) => c.duplicateOf !== null

// Meeting note 18/11: every counted case is in exactly one of the three, so they add up to All.
const toReview = (c: CaseSummary) => c.status === 'pending'
const toAssign = (c: CaseSummary) => HANDLING.includes(c.status) && !c.technician
const toVerify = (c: CaseSummary) => c.status === 'completed'
const waitingForStaff = (c: CaseSummary) =>
  !isDuplicate(c) && (toReview(c) || toAssign(c) || toVerify(c))
const withTechnician = (c: CaseSummary) =>
  !isDuplicate(c) && HANDLING.includes(c.status) && !!c.technician
const finished = (c: CaseSummary) => !isDuplicate(c) && FINAL_STATUSES.includes(c.status)

/** Overdue first, then the staff's steps in flow order, oldest first. */
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
    value: 'waiting',
    label: 'Waiting for staff',
    help: 'New requests to review, cases with no technician, and finished work to check and close.',
    match: waitingForStaff,
    sort: byUrgencyOfWork,
    note: countLine,
    emptyText: 'Nothing waits for the staff. New requests appear here.',
  },
  {
    value: 'with-technicians',
    label: 'With technicians',
    help: 'Cases a technician holds: not accepted yet, accepted, in progress, on hold or reopened.',
    match: withTechnician,
    sort: byUrgencyOfWork,
    emptyText: 'No technician is working on a case.',
  },
  {
    value: 'finished',
    label: 'Finished',
    help: 'Closed, rejected or cancelled cases. Duplicates are not included.',
    match: finished,
    emptyText: 'No finished cases.',
  },
  {
    value: 'all',
    label: 'All',
    help: 'Waiting for staff + With technicians + Finished. Duplicates are not included.',
    match: (c) => !isDuplicate(c),
    emptyText: 'No cases yet.',
  },
  {
    value: 'duplicates',
    label: 'Duplicates',
    help: 'Requests rejected because they repeat another case. You can open them, but no figure counts them.',
    match: isDuplicate,
    emptyText: 'No duplicate requests.',
  },
] as const satisfies readonly [CaseTab, ...CaseTab[]]

/**
 * OFF-02 Case queue + OFF-04 Verification queue (staff: only the buildings in charge, N11) and
 * OFF-03 All cases (manager: read only). Question: "Which cases need me now?"
 */
export function CaseQueuePage() {
  const { user } = useAuth()
  const cases = useCases()
  const isManager = user?.role === 'manager'

  const data = cases.data ?? []
  const reviewCount = data.filter(toReview).length
  const overdue = data.filter((c) => isOverdue(c)).length
  // Only the staff review requests, so the manager gets no "to review" summary (meeting note 18/11).
  const finding =
    cases.data && !isManager
      ? [
          `${reviewCount} new ${reviewCount === 1 ? 'request' : 'requests'} to review`,
          overdue > 0 ? `${overdue} past the deadline` : 'nothing past the deadline',
        ].join(', ')
      : undefined

  return (
    <div className="flex flex-col gap-5">
      <PageHeader title={isManager ? 'All cases' : 'Cases'} description={finding} />
      <CaseListCard tabs={TABS} variant="cards" advanced showRequester showTechnician />
    </div>
  )
}
