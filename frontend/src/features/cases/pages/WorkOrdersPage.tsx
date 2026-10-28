import { PageHeader } from '@/shared/ui'
import { CaseListCard, type CaseTab } from '../components/CaseListCard'
import { MockDataBanner } from '../components/MockDataBanner'
import { useActor, useCases } from '../hooks'
import { isOverdue } from '../sla'
import type { CaseStatus, CaseSummary } from '../types'

const WAITING: readonly CaseStatus[] = ['assigned', 'reopened']
const IN_HAND: readonly CaseStatus[] = ['accepted', 'in_progress', 'on_hold']

/** Jobs to answer first, then overdue, then the nearest deadline. */
const byNextJob = (a: CaseSummary, b: CaseSummary) =>
  Number(WAITING.includes(b.status)) - Number(WAITING.includes(a.status)) ||
  Number(isOverdue(b)) - Number(isOverdue(a)) ||
  (a.dueAt ?? '').localeCompare(b.dueAt ?? '')

/**
 * TEC-02 My work orders (technician). Question: "What do I do next?"
 * Only cases currently assigned to me; jobs waiting for my answer come first.
 */
export function WorkOrdersPage() {
  const actor = useActor()
  const cases = useCases()
  const mine = (c: CaseSummary) => c.technician?.id === actor?.id

  const tabs = [
    {
      value: 'to-do',
      label: 'To do',
      match: (c) => mine(c) && (WAITING.includes(c.status) || IN_HAND.includes(c.status)),
      sort: byNextJob,
      note: (rows) => {
        const waiting = rows.filter((c) => WAITING.includes(c.status)).length
        return waiting ? `${waiting} waiting for your answer` : null
      },
      emptyText: 'Nothing to do. New jobs from the officer appear here.',
    },
    {
      value: 'done',
      label: 'Done, waiting for check',
      match: (c) => mine(c) && c.status === 'completed',
      emptyText: 'No finished work waiting for the officer.',
    },
    {
      value: 'closed',
      label: 'Closed',
      match: (c) => mine(c) && c.status === 'closed',
      emptyText: 'No closed jobs yet.',
    },
  ] as const satisfies readonly [CaseTab, ...CaseTab[]]

  const waiting = cases.data?.filter((c) => mine(c) && WAITING.includes(c.status)).length ?? 0

  return (
    <div className="flex flex-col gap-5">
      <MockDataBanner />
      <PageHeader
        title="My work orders"
        description={
          cases.data
            ? `${waiting} new ${waiting === 1 ? 'job' : 'jobs'} waiting for your answer`
            : undefined
        }
      />
      <CaseListCard tabs={tabs} variant="cards" showRequester />
    </div>
  )
}
