import { useState } from 'react'
import { Link } from 'react-router'
import { caseDetailPath, paths } from '@/app/paths'
import { AttentionList, KpiTiles, useOverview } from '@/features/analytics'
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
  Gauge,
  KpiCard,
  LinkButton,
  LoadingBlock,
  MiniStat,
  PageHeader,
  PanelHeader,
  Rise,
} from '@/shared/ui'
import { formatDate, formatDateTime, formatRelative } from '@/shared/utils/format'
import { BuildingFloors } from '../components/BuildingFloors'
import { BreakdownDonut, RequestTracker } from '../components/CustomerViz'
import { BuildingLeague, EmergencyResponse, ThroughputArea } from '../components/ManagerViz'
import { CasePipeline, UrgencyMix } from '../components/StaffViz'
import { DeadlineRings, WeekPlanner } from '../components/TechnicianViz'

/*
 * Home pages per role (Screen catalog rule 15, F6), ui/mercato-style. 17/11, Leader: one home per role, each
 * built around the question that role asks every day:
 *   Customer   — "where is my request?"            → a parcel-style tracker per open request, what breaks most
 *   Technician — "what do I do this week?"         → a 7-day planner and the deadlines running out
 *   Staff      — "where is the work stuck?"        → the case pipeline by stage, the urgency mix, the floor map
 *   Manager    — "is the team keeping up?"         → KPIs, throughput in vs out, building league, emergency speed
 * Every number links to the list behind it. Counts come from the cases the role may see; trends and on-time rates
 * from the F6 analytics (same definitions). The page title stays a noun with a finding under it.
 */

const DAY = 86_400_000
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

/** yyyy-MM-dd in local time, `offsetDays` from today (read once, when the page opens). */
const localDate = (offsetDays: number) => {
  const d = new Date(Date.now() + offsetDays * DAY)
  return new Date(d.getTime() - d.getTimezoneOffset() * 60_000).toISOString().slice(0, 10)
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

/** Customer: "Where is my request?" */
export function CustomerHome() {
  const { cases, data } = useHomeCases()
  const open = data.filter(isOpen)
  const done = data.filter((c) => c.status === 'completed')
  const notFixed = open.filter((c) => c.status !== 'completed')
  const now = cases.dataUpdatedAt
  // Fixed = reported done by the technician (completed or already closed), counted by when it was fixed.
  const fixed30 = data.filter(
    (c) =>
      (c.status === 'completed' || c.status === 'closed') &&
      c.completedAt !== null &&
      Date.parse(c.completedAt) >= now - 30 * DAY,
  )
  const finished = data
    .filter((c) => c.status === 'closed')
    .sort((a, b) => (b.completedAt ?? '').localeCompare(a.completedAt ?? ''))

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title="Overview"
        description={
          cases.data &&
          sentence(
            [
              [notFixed.length, notFixed.length === 1 ? 'request open' : 'requests open'],
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
          <div className="grid grid-cols-2 gap-3 sm:gap-5 xl:grid-cols-4">
            <KpiCard
              index={0}
              label="Open requests"
              value={notFixed.length}
              icon="requests"
              hue="orange"
              hint="reported, not fixed yet"
              to={`${paths.myRequests}?tab=open`}
            />
            <KpiCard
              index={1}
              label="Being checked"
              value={done.length}
              icon="checklist"
              hue="green"
              hint="fixed, the building checks it"
              to={`${paths.myRequests}?tab=open`}
            />
            <KpiCard
              index={2}
              label="Fixed in 30 days"
              value={fixed30.length}
              icon="clock"
              hue="blue"
              hint="fixed in the last 30 days"
              to={`${paths.myRequests}?tab=finished`}
            />
            <KpiCard
              index={3}
              label="All requests"
              value={data.length}
              icon="list"
              hue="violet"
              hint="since you joined"
              to={paths.myRequests}
            />
          </div>
          <div className="grid items-start gap-5 xl:grid-cols-[minmax(0,2fr)_minmax(0,1fr)]">
            <Rise index={4}>
              <Card>
                <PanelHeader
                  title="Where your requests are"
                  aside={
                    <Link to={paths.myRequests} className="text-muted text-xs underline">
                      All requests
                    </Link>
                  }
                />
                <RequestTracker cases={open} />
              </Card>
            </Rise>
            <Rise index={5}>
              <Card>
                <PanelHeader title="What you reported" />
                <BreakdownDonut cases={data} />
              </Card>
            </Rise>
          </div>
          <Rise index={6}>
            <Card>
              <CardHeader title="Recently closed" />
              <CaseList
                rows={finished.slice(0, 5)}
                note={(c) => (c.completedAt ? `fixed ${formatRelative(c.completedAt)}` : 'closed')}
                empty="Nothing closed yet."
              />
            </Card>
          </Rise>
        </>
      )}
    </div>
  )
}

// ----- TEC-01 -----

/** Technician: "What do I do this week?" — the planner, the deadlines running out, then the list. */
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
          <div className="grid grid-cols-2 gap-3 sm:gap-5 xl:grid-cols-4">
            <KpiCard
              index={0}
              label="New jobs"
              value={newJobs.length}
              icon="bell"
              hue="orange"
              hint="accept or decline"
              to={paths.workOrders}
            />
            <KpiCard
              index={1}
              label="Working on"
              value={working.length}
              icon="wrench"
              hue="violet"
              hint="accepted or in progress"
              to={paths.workOrders}
            />
            <KpiCard
              index={2}
              label="On hold"
              value={onHold.length}
              icon="clock"
              hue="blue"
              hint="paused for now"
              to={paths.workOrders}
            />
            <KpiCard
              index={3}
              label="Waiting for check"
              value={waitingCheck.length}
              icon="checklist"
              hue="green"
              hint="the staff check your work"
              to={`${paths.workOrders}?tab=done`}
            />
          </div>
          <div className="grid grid-cols-[minmax(0,1fr)] items-start gap-5 xl:grid-cols-[minmax(0,2fr)_minmax(0,1fr)]">
            <Rise index={4}>
              <Card>
                <PanelHeader title="Your next 7 days" />
                <WeekPlanner cases={data} now={cases.dataUpdatedAt} />
              </Card>
            </Rise>
            <Rise index={5}>
              <Card>
                <PanelHeader title="Deadlines running out" />
                <DeadlineRings cases={data} />
              </Card>
            </Rise>
          </div>
          <Rise index={6}>
            <Card>
              <CardHeader title="By deadline" aside="soonest first" />
              <CaseList
                rows={byDeadline.slice(0, 8)}
                // Meeting 07/11 and note 18/11: every job shows when it starts, then where its deadline stands.
                note={(c) =>
                  [
                    c.plannedStartAt
                      ? `starts ${formatDateTime(c.plannedStartAt)}`
                      : 'start not set',
                    isOverdue(c)
                      ? 'overdue'
                      : c.dueAt
                        ? `due ${formatDateTime(c.dueAt)}`
                        : c.expectedDueAt
                          ? `expected due ${formatDateTime(c.expectedDueAt)}`
                          : c.isEmergencyFlow
                            ? 'emergency'
                            : 'not scheduled yet',
                  ]
                    .filter(Boolean)
                    .join(' · ')
                }
                empty="Nothing to do right now."
              />
            </Card>
          </Rise>
        </>
      )}
    </div>
  )
}

// ----- OFF-01 -----

/** Staff: "Where is the work stuck in my buildings?" */
export function StaffHome() {
  const { cases, data } = useHomeCases()
  const tree = useFacilityTree()
  const review = data.filter((c) => c.status === 'pending')
  // Approved with nobody: a technician declined, or an emergency came in on its own.
  const assign = data.filter(
    (c) => c.status === 'approved' && !c.technician && !c.emergency?.mergedInto,
  )
  const verify = data.filter((c) => c.status === 'completed')
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
        <>
          {/* Meeting note 18/11: what needs the staff member comes first. */}
          <Rise index={0}>
            <AttentionList limit={6} />
          </Rise>
          <Rise index={1}>
            <CasePipeline cases={data} />
          </Rise>
          <div className="grid items-start gap-5 lg:grid-cols-[minmax(0,7fr)_minmax(0,5fr)]">
            <Rise index={3}>
              <BuildingFloors cases={data} />
            </Rise>
            <Rise index={4}>
              <Card>
                <PanelHeader title="Urgency of open cases" />
                <UrgencyMix cases={data} now={cases.dataUpdatedAt} />
              </Card>
            </Rise>
          </div>
        </>
      )}
    </div>
  )
}

// ----- MGR-01 -----

/** Manager: "Is the team keeping up, and where not?" Read only (BR-12). */
export function ManagerHome() {
  // Both ranges are fixed when the page opens (no clock reads while rendering).
  const [week] = useState(() => ({ from: localDate(-6), to: localDate(0) }))
  // The weekly points start on a Monday: begin 11 Mondays before this week's, so the chart holds exactly 12 weeks.
  const [quarter] = useState(() => ({
    from: localDate(-(((new Date().getDay() + 6) % 7) + 77)),
    to: localDate(0),
  }))
  const last7 = useOverview(week)
  const last12w = useOverview(quarter)
  const openNow = last7.data?.byBuilding.reduce((n, b) => n + b.openNow, 0) ?? 0
  const overdueNow = last7.data?.byBuilding.reduce((n, b) => n + b.overdueNow, 0) ?? 0

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title="This week"
        description={
          last7.data &&
          `${last7.data.current.reported} reported in the last 7 days; ${sentence(
            [
              [openNow, 'open now'],
              [overdueNow, 'past the deadline'],
            ],
            'nothing open now.',
          )}`
        }
        actions={<LinkButton to={paths.analytics}>See full dashboard</LinkButton>}
      />
      {last7.isPending ? (
        <LoadingBlock rows={2} />
      ) : last7.error ? (
        <ErrorBlock
          what="the numbers"
          message={last7.error.message}
          onRetry={() => void last7.refetch()}
        />
      ) : (
        <KpiTiles o={last7.data} isManager compact />
      )}
      <div className="grid items-stretch gap-5 xl:grid-cols-[minmax(0,2fr)_minmax(0,1fr)]">
        <Rise index={4}>
          <Card className="h-full">
            <PanelHeader
              title="Keeping up"
              subtitle={`Last 12 weeks · ${formatDate(quarter.from)} – ${formatDate(quarter.to)}`}
              help="Cases reported each week (came in) against cases reported done (finished). When more come in than are finished, the backlog grows."
            />
            {last12w.data ? (
              <ThroughputArea weekly={last12w.data.weekly} />
            ) : (
              <LoadingBlock rows={4} />
            )}
          </Card>
        </Rise>
        <Rise index={5}>
          <Card className="flex h-full flex-col">
            <PanelHeader
              title="On time"
              subtitle="Last 7 days"
              help="Share of deadlines met: cases reported done by their deadline, out of those done in the 7 days plus those still open whose deadline passed in them. Open now and past deadline are counted at this moment."
            />
            <div className="flex flex-1 items-center justify-center">
              <Gauge
                value={last7.data?.current.onTimeRate ?? null}
                caption="of the deadlines met"
              />
            </div>
            <div className="mt-4 grid grid-cols-2 gap-3">
              <MiniStat label="Open now" value={openNow} />
              <MiniStat
                label="Past deadline"
                value={<span className={overdueNow > 0 ? 'text-negative' : ''}>{overdueNow}</span>}
              />
            </div>
          </Card>
        </Rise>
      </div>
      <div className="grid items-start gap-5 lg:grid-cols-2">
        <Rise index={6}>
          <Card>
            <PanelHeader
              title="Buildings"
              subtitle={`Ranked by on time · last 12 weeks, ${formatDate(quarter.from)} – ${formatDate(quarter.to)}`}
              help="Each building's share of deadlines met in the last 12 weeks. Open and late are counted now. A long list scrolls inside the card."
            />
            {last12w.data ? (
              <div className="-mr-2 max-h-[360px] overflow-y-auto pr-2">
                <BuildingLeague buildings={last12w.data.byBuilding} />
              </div>
            ) : (
              <LoadingBlock rows={3} />
            )}
          </Card>
        </Rise>
        <Rise index={7}>
          <Card>
            <PanelHeader
              title="Emergency response"
              subtitle={`Last 12 weeks · ${formatDate(quarter.from)} – ${formatDate(quarter.to)}`}
              help="Average time from when a case became an emergency to the staff acknowledging it, and to the technician reporting it fixed."
            />
            {last12w.data ? (
              <EmergencyResponse e={last12w.data.emergency} />
            ) : (
              <LoadingBlock rows={3} />
            )}
          </Card>
        </Rise>
      </div>
      <Rise index={8}>
        <AttentionList limit={8} viewAll />
      </Rise>
    </div>
  )
}
