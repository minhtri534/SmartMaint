import { useState } from 'react'
import { Link } from 'react-router'
import { caseDetailPath, paths } from '@/app/paths'
import { AttentionList, KpiTiles, StatTile, useOverview } from '@/features/analytics'
import { useAuth } from '@/features/auth'
import {
  FINAL_STATUSES,
  isOverdue,
  StatusBadge,
  useCases,
  type CaseSummary,
} from '@/features/cases'
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

/*
 * Home pages per role (Screen catalog rule 15, F6): number tiles and "what to do now". Every number is a link to
 * the list behind it. Counts come from the cases the role may see; KPIs from the F6 analytics (same definitions).
 */

const isOpen = (c: CaseSummary) => !FINAL_STATUSES.includes(c.status)

function useHomeCases() {
  const cases = useCases(60_000)
  return { cases, data: cases.data ?? [] }
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

const firstName = (name?: string) => name?.split(' ').at(-1) ?? ''

// ----- EMP-01 -----

/** Customer: "Where are my requests?" */
export function CustomerHome() {
  const { user } = useAuth()
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
        title={`Hello, ${firstName(user?.fullName)}`}
        description="Your requests to the building management."
        actions={
          <LinkButton variant="primary" to={paths.newRequest}>
            New request
          </LinkButton>
        }
      />
      <CasesState cases={cases} />
      {cases.data && (
        <>
          <div className="grid grid-cols-2 gap-2 md:grid-cols-3">
            <StatTile
              label="Open requests"
              value={open.length}
              to={`${paths.myRequests}?tab=open`}
            />
            <StatTile
              label="Fixed, being checked"
              value={done.length}
              to={`${paths.myRequests}?tab=open`}
            />
            <StatTile
              label="Closed in 30 days"
              value={closed.length}
              to={`${paths.myRequests}?tab=finished`}
            />
          </div>
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
  const { user } = useAuth()
  const { cases, data } = useHomeCases()
  const newJobs = data.filter((c) => c.status === 'assigned' || c.status === 'reopened')
  const working = data.filter((c) => c.status === 'accepted' || c.status === 'in_progress')
  const onHold = data.filter((c) => c.status === 'on_hold')
  const waitingCheck = data.filter((c) => c.status === 'completed')
  const byDeadline = [...newJobs, ...working, ...onHold].sort((a, b) =>
    (a.dueAt ?? '9999').localeCompare(b.dueAt ?? '9999'),
  )

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title={`Hello, ${firstName(user?.fullName)}`}
        description={
          cases.data &&
          `${newJobs.length} new ${newJobs.length === 1 ? 'job waits' : 'jobs wait'} for your answer, ${byDeadline.filter((c) => isOverdue(c)).length} overdue.`
        }
        actions={<LinkButton to={paths.workOrders}>My work orders</LinkButton>}
      />
      <CasesState cases={cases} />
      {cases.data && (
        <>
          <div className="grid grid-cols-2 gap-2 md:grid-cols-4">
            <StatTile
              label="New jobs"
              value={newJobs.length}
              to={paths.workOrders}
              hint="accept or decline"
            />
            <StatTile label="Working on" value={working.length} to={paths.workOrders} />
            <StatTile label="On hold" value={onHold.length} to={paths.workOrders} />
            <StatTile
              label="Waiting for check"
              value={waitingCheck.length}
              to={`${paths.workOrders}?tab=done`}
            />
          </div>
          <Card>
            <CardHeader title="By deadline" aside="soonest first" />
            <CaseList
              rows={byDeadline.slice(0, 8)}
              note={(c) =>
                isOverdue(c)
                  ? 'overdue'
                  : c.dueAt
                    ? `due ${formatDateTime(c.dueAt)}`
                    : c.isEmergencyFlow
                      ? 'emergency'
                      : 'deadline starts with the work'
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
  const { user } = useAuth()
  const { cases, data } = useHomeCases()
  const overview = useOverview({})
  const review = data.filter((c) => c.status === 'pending')
  const assign = data.filter((c) => c.status === 'approved' && !c.emergency?.mergedInto)
  const verify = data.filter((c) => c.status === 'completed')
  const emergencies = data.filter(
    (c) => c.isEmergencyFlow && isOpen(c) && !c.emergency?.acknowledgedAt,
  )
  const decisions = data.filter(
    (c) => c.status === 'on_hold' && c.holdReason === 'waiting_staff_decision',
  )

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title={`Hello, ${firstName(user?.fullName)}`}
        description="What waits for you in your buildings."
        actions={<LinkButton to={paths.cases}>Case queue</LinkButton>}
      />
      <CasesState cases={cases} />
      {cases.data && (
        <div className="grid grid-cols-2 gap-2 md:grid-cols-3 xl:grid-cols-6">
          <StatTile label="To review" value={review.length} to={paths.cases} />
          <StatTile label="To assign" value={assign.length} to={paths.cases} />
          <StatTile label="To verify" value={verify.length} to={paths.cases} />
          <StatTile
            label="Emergencies"
            value={emergencies.length}
            hint="not acknowledged"
            to={paths.emergencies}
          />
          <StatTile
            label="Your decision"
            value={decisions.length}
            hint="repair or replace"
            to={paths.outsideWork}
          />
          <StatTile
            label="On time, 30 days"
            value={
              overview.data?.current.onTimeRate === null || !overview.data
                ? '—'
                : `${Math.round(overview.data.current.onTimeRate * 100)}%`
            }
            to={paths.analytics}
          />
        </div>
      )}
      <AttentionList limit={8} />
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
  const { user } = useAuth()
  // The 7 days are fixed when the page opens (no clock reads while rendering).
  const [range] = useState(() => ({ from: localDate(-6), to: localDate(0) }))
  const week = useOverview(range)

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title={`Hello, ${firstName(user?.fullName)}`}
        description="The last 7 days in your buildings, compared with the 7 days before."
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
      <div className="grid items-start gap-5 lg:grid-cols-[minmax(0,1fr)_minmax(0,1fr)]">
        <AttentionList limit={10} />
        {week.data && (
          <Card>
            <CardHeader title="Buildings now" />
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-border text-muted border-b text-xs">
                  <th className="h-9 pr-3 font-normal">Building</th>
                  <th className="h-9 pr-3 text-right font-normal">Open</th>
                  <th className="h-9 text-right font-normal">Overdue</th>
                </tr>
              </thead>
              <tbody className="tabular-nums">
                {week.data.byBuilding.map((b) => (
                  <tr key={b.buildingId} className="border-rule border-b last:border-0">
                    <td className="py-2.5 pr-3">
                      <Link
                        to={`${paths.analytics}?building=${b.buildingId}`}
                        className="underline-offset-2 hover:underline"
                      >
                        {b.name}
                      </Link>
                    </td>
                    <td className="py-2.5 pr-3 text-right">{b.openNow}</td>
                    <td
                      className={
                        b.overdueNow > 0 ? 'text-negative py-2.5 text-right' : 'py-2.5 text-right'
                      }
                    >
                      {b.overdueNow}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </Card>
        )}
      </div>
    </div>
  )
}
