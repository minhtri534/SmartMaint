import { useState } from 'react'
import { Link } from 'react-router'
import { caseDetailPath, paths } from '@/app/paths'
import { AttentionList, KpiTiles, StatStrip, useOverview } from '@/features/analytics'
import {
  FINAL_STATUSES,
  isOverdue,
  StatusBadge,
  useCases,
  type CaseSummary,
} from '@/features/cases'
import { useFacilityTree } from '@/features/facilities'
import {
  Card,
  CardHeader,
  EmptyBlock,
  ErrorBlock,
  LinkButton,
  LoadingBlock,
  PageHeader,
} from '@/shared/ui'
import { formatDateTime, formatRelative } from '@/shared/utils/format'
import { BuildingFloors } from '../components/BuildingFloors'

/*
 * Home pages per role (Screen catalog rule 15, F6): one strip of numbers and "what to do now". Every number is a
 * link to the list behind it. Counts come from the cases the role may see; KPIs from the F6 analytics (same
 * definitions). Titles are nouns with a finding from the data under them, never "Hello" (U5, 16/11).
 */

const isOpen = (c: CaseSummary) => !FINAL_STATUSES.includes(c.status)

function useHomeCases() {
  const cases = useCases(60_000)
  return { cases, data: cases.data ?? [] }
}

/** "2 to review, 1 overdue." — the non-zero parts only. */
function sentence(parts: [number, string][], none: string) {
  const said = parts.filter(([n]) => n > 0).map(([n, words]) => `${n} ${words}`)
  return said.length ? said.join(', ') + '.' : none
}

function CasesState({ cases }: { cases: ReturnType<typeof useCases> }) {
  if (cases.isPending) return <LoadingBlock rows={4} />
  if (cases.error)
    return (
      <ErrorBlock
        what="your cases"
        message={cases.error.message}
        onRetry={() => void cases.refetch()}
      />
    )
  return null
}

/** A short case list: title, code, status, a right-hand note. */
function CaseList({
  rows,
  note,
  empty,
}: {
  rows: CaseSummary[]
  note: (c: CaseSummary) => string
  empty: string
}) {
  if (rows.length === 0) return <EmptyBlock>{empty}</EmptyBlock>
  return (
    <ul className="flex flex-col">
      {rows.map((c) => (
        <li
          key={c.id}
          className="border-rule flex items-start justify-between gap-3 border-b py-2.5 text-sm first:pt-0 last:border-0 last:pb-0"
        >
          <div className="min-w-0">
            <Link
              to={caseDetailPath(c.id)}
              className="block truncate font-medium underline-offset-2 hover:underline"
            >
              {c.title}
            </Link>
            <div className="text-muted truncate text-[13px]">
              <span className="font-mono text-xs">{c.code}</span> · {c.location.roomName} ·{' '}
              {note(c)}
            </div>
          </div>
          <StatusBadge status={c.status} />
        </li>
      ))}
    </ul>
  )
}

// ----- EMP-01 -----

/** Customer: "Where are my requests?" */
export function CustomerHome() {
  const { cases, data } = useHomeCases()
  const open = data.filter(isOpen)
  const done = data.filter((c) => c.status === 'completed')
  const month = cases.dataUpdatedAt - 30 * 86_400_000
  const closed = data.filter(
    (c) => c.status === 'closed' && new Date(c.submittedAt).getTime() >= month,
  )

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title="Overview"
        description={
          cases.data &&
          sentence(
            [
              [
                open.length - done.length,
                open.length - done.length === 1 ? 'request open' : 'requests open',
              ],
              [done.length, 'fixed and waiting for the building to check'],
            ],
            'No open request. Report a problem when something breaks.',
          )
        }
        actions={
          <LinkButton variant="primary" to={paths.newRequest}>
            New request
          </LinkButton>
        }
      />
      <CasesState cases={cases} />
      {cases.data && (
        <>
          <StatStrip
            className="md:grid-cols-3"
            items={[
              { label: 'Open requests', value: open.length, to: `${paths.myRequests}?tab=open` },
              {
                label: 'Fixed, being checked',
                value: done.length,
                to: `${paths.myRequests}?tab=open`,
              },
              {
                label: 'Closed in 30 days',
                value: closed.length,
                to: `${paths.myRequests}?tab=finished`,
              },
            ]}
          />
          <Card>
            <CardHeader
              title="Open requests"
              aside={
                <Link to={paths.myRequests} className="underline">
                  All
                </Link>
              }
            />
            <CaseList
              rows={open.slice(0, 6)}
              note={(c) => `reported ${formatRelative(c.submittedAt)}`}
              empty="You have no open request."
            />
          </Card>
        </>
      )}
    </div>
  )
}

// ----- TEC-01 -----

/** Technician: "What do I do next?" — new jobs to answer, then the work by deadline. */
export function TechnicianHome() {
  const { cases, data } = useHomeCases()
  // Meeting 07/11: a job given to the technician waits as "approved" until they accept it.
  const newJobs = data.filter((c) => c.status === 'approved' || c.status === 'reopened')
  const working = data.filter((c) => c.status === 'accepted' || c.status === 'in_progress')
  const onHold = data.filter((c) => c.status === 'on_hold')
  const waitingCheck = data.filter((c) => c.status === 'completed')
  const byDeadline = [...newJobs, ...working, ...onHold].sort((a, b) =>
    (a.dueAt ?? '9999').localeCompare(b.dueAt ?? '9999'),
  )
  const overdue = byDeadline.filter((c) => isOverdue(c)).length

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title="Today"
        description={
          cases.data &&
          sentence(
            [
              [
                newJobs.length,
                newJobs.length === 1
                  ? 'new job waits for your answer'
                  : 'new jobs wait for your answer',
              ],
              [working.length, 'in hand'],
              [overdue, 'past the deadline'],
            ],
            'Nothing assigned to you right now.',
          )
        }
        actions={<LinkButton to={paths.workOrders}>My work orders</LinkButton>}
      />
      <CasesState cases={cases} />
      {cases.data && (
        <>
          <StatStrip
            className="md:grid-cols-4"
            items={[
              {
                label: 'New jobs',
                value: newJobs.length,
                tone: newJobs.length > 0 ? 'action' : undefined,
                to: paths.workOrders,
                hint: 'accept or decline',
              },
              { label: 'Working on', value: working.length, to: paths.workOrders },
              { label: 'On hold', value: onHold.length, to: paths.workOrders },
              {
                label: 'Waiting for check',
                value: waitingCheck.length,
                to: `${paths.workOrders}?tab=done`,
              },
            ]}
          />
          <Card>
            <CardHeader title="By deadline" aside="soonest first" />
            <CaseList
              rows={byDeadline.slice(0, 8)}
              // Meeting 07/11: when each job is planned to start, then where its deadline stands.
              note={(c) =>
                [
                  c.plannedStartAt && `starts ${formatDateTime(c.plannedStartAt)}`,
                  isOverdue(c)
                    ? 'overdue'
                    : c.dueAt
                      ? `due ${formatDateTime(c.dueAt)}`
                      : c.isEmergencyFlow
                        ? 'emergency'
                        : 'deadline starts with the work',
                ]
                  .filter(Boolean)
                  .join(' · ')
              }
              empty="Nothing to do right now."
            />
          </Card>
        </>
      )}
    </div>
  )
}

// ----- OFF-01 -----

/** Staff: "What waits for me in my buildings?" */
export function StaffHome() {
  const { cases, data } = useHomeCases()
  const tree = useFacilityTree()
  const overview = useOverview({})
  const review = data.filter((c) => c.status === 'pending')
  // Approved with nobody: a technician declined, or an emergency came in on its own.
  const assign = data.filter(
    (c) => c.status === 'approved' && !c.technician && !c.emergency?.mergedInto,
  )
  const verify = data.filter((c) => c.status === 'completed')
  const emergencies = data.filter(
    (c) => c.isEmergencyFlow && isOpen(c) && !c.emergency?.acknowledgedAt,
  )
  const decisions = data.filter(
    (c) => c.status === 'on_hold' && c.holdReason === 'waiting_staff_decision',
  )
  const overdue = data.filter((c) => isOverdue(c)).length
  // The title is the building itself when the staff member looks after one.
  const title = (tree.data?.length === 1 && tree.data[0]?.name) || 'Your buildings'

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title={title}
        description={
          cases.data &&
          sentence(
            [
              [review.length, 'to review'],
              [assign.length, 'to assign'],
              [verify.length, 'to verify'],
              [overdue, 'past the deadline'],
            ],
            'Nothing waits for you.',
          )
        }
        actions={<LinkButton to={paths.cases}>Case queue</LinkButton>}
      />
      <CasesState cases={cases} />
      {cases.data && (
        <StatStrip
          className="md:grid-cols-3 xl:grid-cols-6"
          items={[
            {
              label: 'To review',
              value: review.length,
              to: paths.cases,
              tone: review.length > 0 ? 'action' : undefined,
            },
            {
              label: 'To assign',
              value: assign.length,
              to: paths.cases,
              tone: assign.length > 0 ? 'action' : undefined,
            },
            {
              label: 'To verify',
              value: verify.length,
              to: paths.cases,
              tone: verify.length > 0 ? 'action' : undefined,
            },
            {
              label: 'Emergencies',
              value: emergencies.length,
              hint: 'not acknowledged',
              to: `${paths.cases}?tab=all&urgency=emergency`,
              tone: emergencies.length > 0 ? 'alert' : undefined,
            },
            {
              label: 'Your decision',
              value: decisions.length,
              hint: 'repair or replace',
              to: `${paths.cases}?tab=all&status=on_hold`,
            },
            {
              label: 'On time, 30 days',
              value:
                overview.data?.current.onTimeRate === null || !overview.data
                  ? '—'
                  : `${Math.round(overview.data.current.onTimeRate * 100)}%`,
            },
          ]}
        />
      )}
      <div className="grid items-start gap-5 lg:grid-cols-[minmax(0,5fr)_minmax(0,7fr)]">
        {cases.data && <BuildingFloors cases={data} />}
        <AttentionList limit={8} />
      </div>
    </div>
  )
}

// ----- MGR-01 -----

const localDate = (offsetDays: number) => {
  const d = new Date(Date.now() + offsetDays * 86_400_000)
  return new Date(d.getTime() - d.getTimezoneOffset() * 60_000).toISOString().slice(0, 10)
}

/** Manager: "How is maintenance doing this week, and what needs attention?" Read only (BR-12). */
export function ManagerHome() {
  // The 7 days are fixed when the page opens (no clock reads while rendering).
  const [range] = useState(() => ({ from: localDate(-6), to: localDate(0) }))
  const week = useOverview(range)
  const { cases, data } = useHomeCases()
  const openNow = week.data?.byBuilding.reduce((n, b) => n + b.openNow, 0) ?? 0
  const overdueNow = week.data?.byBuilding.reduce((n, b) => n + b.overdueNow, 0) ?? 0

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title="This week"
        description={
          week.data &&
          `${week.data.current.reported} reported in the last 7 days; ${sentence(
            [
              [openNow, 'open now'],
              [overdueNow, 'past the deadline'],
            ],
            'nothing open now.',
          )}`
        }
        actions={<LinkButton to={paths.analytics}>See full dashboard</LinkButton>}
      />
      {week.isPending ? (
        <LoadingBlock rows={2} />
      ) : week.error ? (
        <ErrorBlock
          what="the numbers"
          message={week.error.message}
          onRetry={() => void week.refetch()}
        />
      ) : (
        <KpiTiles o={week.data} isManager />
      )}
      <div className="grid items-start gap-5 lg:grid-cols-[minmax(0,5fr)_minmax(0,7fr)]">
        {/* Floors replace the "Buildings now" table: the tabs carry each building's open count. */}
        {cases.data ? <BuildingFloors cases={data} /> : <CasesState cases={cases} />}
        <AttentionList limit={10} />
      </div>
    </div>
  )
}
