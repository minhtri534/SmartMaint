import { useState, type ReactNode } from 'react'
import { Link, useParams } from 'react-router'
import { caseDetailPath, paths, planDetailPath, resubmitPath } from '@/app/paths'
import type { Role } from '@/shared/types/role'
import { Alert, Button, Card, CardHeader, Icon, LinkButton, PageHeader, Tabs } from '@/shared/ui'
import { formatDateTime, formatVnd } from '@/shared/utils/format'
import { useAuth } from '@/features/auth'
import { CaseActions } from '../components/CaseActions'
import { UrgencyText } from '../components/CaseBadges'
import { AddCostDialog } from '../components/dialogs/AddCostDialog'
import { EmergencyCard, IncidentReportCard } from '../components/EmergencyCard'
import { AssessmentCard, DecisionInfoCard, ThirdPartiesCard } from '../components/OutsideWorkCards'
import { PhotoGallery, RejectionList, StatusTimeline } from '../components/CaseHistory'
import { SlaPanel } from '../components/SlaTrack'
import { StarsView } from '../components/Stars'
import { StatusPath } from '../components/StatusPath'
import { useCase } from '../hooks'
import { workedHoursNow } from '../work-hours'
import { FINAL_STATUSES, HOLD_REASON_LABELS, type Cost } from '../types'

const BACK: Partial<Record<Role, { to: string; label: string }>> = {
  customer: { to: paths.myRequests, label: 'My requests' },
  staff: { to: paths.cases, label: 'Cases' },
  manager: { to: paths.cases, label: 'All cases' },
  technician: { to: paths.workOrders, label: 'My work orders' },
}

/**
 * COM-08 Case detail for every role; for the technician it is also TEC-03 (work order).
 * Question: "Where is this case, who has it, and is it on time?" — the road through F3 on top, the
 * deadline as the largest figure on the right. What each role can press comes from the server (allowedActions).
 */
export function CaseDetailPage() {
  const id = Number(useParams().id)
  const { user } = useAuth()
  const query = useCase(id)
  const [editingCost, setEditingCost] = useState<Cost | null>(null)
  const [tab, setTab] = useState<'details' | 'history'>('details')
  const back = user ? BACK[user.role] : undefined

  const above = back && (
    <Link to={back.to} className="hover:text-ink inline-flex items-center gap-1">
      <Icon name="back" className="size-3.5" />
      {back.label}
    </Link>
  )

  if (query.isPending) return <DetailSkeleton above={above} />
  if (query.error) {
    return (
      <div className="flex flex-col gap-5">
        <PageHeader above={above} title="Case" />
        <div className="border-border rounded-card flex items-center justify-between gap-3 border px-4 py-3 text-sm">
          <span>{query.error.message}</span>
          <Button variant="ghost" onClick={() => void query.refetch()}>
            Retry
          </Button>
        </div>
      </div>
    )
  }

  const c = query.data
  const seesRejections = user?.role === 'staff' || user?.role === 'manager'
  // Meeting 07/11: how the work was done (tasks, repair record, assessment, outside parties) is for the building's
  // people only; the customer follows the status, the photos and the history.
  const seesWork = user?.role !== 'customer'
  const lastChange = c.history.at(-1)
  // Meeting note 18/11: the customer reads the history in its own tab, so it never pushes the case down.
  const historyTab = user?.role === 'customer'
  const showDetails = !historyTab || tab === 'details'

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        above={above}
        title={c.title}
        description={
          <>
            <span className="font-mono">{c.code}</span> · {c.serviceTypeName}
            {c.isPreventive && ' · Preventive maintenance'}
          </>
        }
        actions={<CaseActions caseDetail={c} />}
      />

      <div className="border-border border-y py-4">
        <StatusPath c={c} />
      </div>

      {/* Meeting 07/11: a change of technician on taken work continues the case in a new one. */}
      {c.previousCase && (
        <Alert tone="info">
          Continues{' '}
          <Link
            to={caseDetailPath(c.previousCase.id)}
            className="font-mono underline underline-offset-2"
          >
            {c.previousCase.code}
          </Link>
          , where the previous technician's finished tasks stay.
        </Alert>
      )}
      {c.nextCase && (
        <Alert tone="info">
          Given to another technician: the work continues in{' '}
          <Link
            to={caseDetailPath(c.nextCase.id)}
            className="font-mono underline underline-offset-2"
          >
            {c.nextCase.code}
          </Link>
          .
        </Alert>
      )}
      {c.duplicateOf && (
        <Alert tone="info">
          This request repeats{' '}
          <Link
            to={caseDetailPath(c.duplicateOf.id)}
            className="font-mono underline underline-offset-2"
          >
            {c.duplicateOf.code}
          </Link>
          . It is kept for reference and is not counted in any figure.
        </Alert>
      )}
      {c.emergency?.mergedInto && !c.duplicateOf && (
        <Alert tone="info">
          Same incident as{' '}
          <Link
            to={caseDetailPath(c.emergency.mergedInto.id)}
            className="font-mono underline underline-offset-2"
          >
            {c.emergency.mergedInto.code}
          </Link>
          {user?.role === 'customer'
            ? ': it is being handled there and you will be told when it is fixed.'
            : ': this report was merged into it.'}
        </Alert>
      )}
      {c.isEmergencyFlow && user?.role === 'customer' && !FINAL_STATUSES.includes(c.status) && (
        <Alert>
          If anyone is in danger, call 114 (fire) or 115 (ambulance) and the building hotline first.
          The building staff were alerted the moment you reported.
        </Alert>
      )}
      {c.isEmergencyFlow &&
        c.emergency &&
        !c.emergency.acknowledgedAt &&
        user?.role !== 'customer' && (
          <Alert tone="notice">
            Emergency: nobody has acknowledged it yet. The backup staff are alerted after 10
            minutes, the manager after 30.
          </Alert>
        )}
      {c.status === 'rejected' && lastChange?.reason && (
        <Alert tone="info">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <span>
              Rejected by {lastChange.by.fullName}: {lastChange.reason}
            </span>
            {user?.role === 'customer' && (
              <LinkButton to={resubmitPath(c.id)} variant="primary">
                Submit again
              </LinkButton>
            )}
          </div>
        </Alert>
      )}
      {c.hold && (
        <Alert tone="info">
          On hold: {HOLD_REASON_LABELS[c.hold.reason]}.
          {c.hold.expectedResumeOn && ` Expected to continue on ${c.hold.expectedResumeOn}.`}
        </Alert>
      )}
      {c.isSurvey && (
        <Alert tone="info">
          Asset survey opened by the staff: the technician assesses whether to repair or replace it.
          Not counted in the KPIs.
        </Alert>
      )}
      {c.notFixed && (
        <Alert tone="notice">
          The customer says it is still not fixed ({formatDateTime(c.notFixed.reportedAt)}): “
          {c.notFixed.reason}”
        </Alert>
      )}
      {c.status === 'reopened' && lastChange?.reason && (
        <Alert>
          Reopened by {lastChange.by.fullName}: {lastChange.reason}
        </Alert>
      )}

      <div className="grid items-start gap-6 lg:grid-cols-[minmax(0,1fr)_360px]">
        <div className="flex min-w-0 flex-col gap-6">
          {historyTab && (
            <Tabs
              items={[
                { value: 'details', label: 'Details' },
                { value: 'history', label: 'History', count: c.history.length },
              ]}
              value={tab}
              onChange={setTab}
            />
          )}
          {showDetails && (
            <>
              <Card>
                <CardHeader
                  title={c.isPreventive ? 'Work order' : 'Problem'}
                  aside={`${c.isPreventive ? 'Made' : 'Reported'} ${formatDateTime(c.submittedAt)}`}
                />
                <p className="max-w-[68ch] text-sm leading-6 whitespace-pre-line">
                  {c.description}
                </p>
                <dl className="border-rule mt-5 grid gap-4 border-t pt-4 text-sm sm:grid-cols-2">
                  <Field label="Location">
                    {c.location.roomName}
                    <div className="text-muted text-xs">
                      {c.location.buildingName} · {c.location.floorName}
                    </div>
                  </Field>
                  <Field label="Asset">
                    {c.asset?.name ?? <span className="text-muted">Not specified</span>}
                  </Field>
                </dl>
                <div className="mt-5">
                  <PhotoGallery photos={c.photos} />
                </div>
              </Card>

              {seesWork && c.assessment && <AssessmentCard assessment={c.assessment} />}
              {seesWork && c.thirdParties.length > 0 && (
                <ThirdPartiesCard parties={c.thirdParties} />
              )}

              {seesWork && c.repairReport && (
                <Card>
                  <CardHeader
                    title={c.isPreventive ? 'Maintenance report' : 'Repair record'}
                    aside={c.technician ? `by ${c.technician.fullName}` : undefined}
                  />
                  <dl className="grid gap-4 text-sm sm:grid-cols-[minmax(0,1fr)_minmax(0,1fr)_96px]">
                    <Field label={c.isPreventive ? 'Note' : 'Cause'}>
                      {c.repairReport.rootCause}
                    </Field>
                    <Field label="Parts used">
                      {c.repairReport.partsUsed || <span className="text-muted">—</span>}
                    </Field>
                    <Field label="Hours worked">
                      <span className="tabular-nums">{c.repairReport.laborHours} h</span>
                    </Field>
                  </dl>
                </Card>
              )}

              {c.costs && c.costs.length > 0 && (
                <Card>
                  <CardHeader
                    title="Costs"
                    aside={
                      <span className="tabular-nums">{formatVnd(c.totalCost ?? 0)} total</span>
                    }
                  />
                  <ul className="flex flex-col text-sm">
                    {c.costs.map((x) => (
                      <li
                        key={x.id}
                        className="border-rule flex justify-between gap-4 border-b py-2 first:pt-0 last:border-0"
                      >
                        <span>
                          {x.name}
                          {x.description && <span className="text-muted"> · {x.description}</span>}
                          {(x.receiptCode || x.receiptUrl) && (
                            <span className="text-muted block text-xs">
                              Receipt {x.receiptCode}
                              {x.receiptUrl && (
                                <>
                                  {' '}
                                  <a
                                    href={x.receiptUrl}
                                    target="_blank"
                                    rel="noreferrer"
                                    className="underline underline-offset-2"
                                  >
                                    photo
                                  </a>
                                </>
                              )}
                            </span>
                          )}
                        </span>
                        <span className="flex flex-none items-baseline gap-1 tabular-nums">
                          {formatVnd(x.amount)}
                          {c.allowedActions.includes('edit_own_cost') &&
                            x.addedById === user?.id && (
                              <Button
                                variant="ghost"
                                aria-label={`Edit ${x.name}`}
                                onClick={() => setEditingCost(x)}
                              >
                                Edit
                              </Button>
                            )}
                        </span>
                      </li>
                    ))}
                  </ul>
                  {editingCost && (
                    <AddCostDialog
                      caseId={c.id}
                      cost={editingCost}
                      open
                      onClose={() => setEditingCost(null)}
                    />
                  )}
                </Card>
              )}

              {c.incidentReport && <IncidentReportCard report={c.incidentReport} />}

              {c.rating && (
                <Card>
                  <CardHeader title="Customer rating" aside={formatDateTime(c.rating.ratedAt)} />
                  <div className="flex items-center gap-3">
                    <StarsView stars={c.rating.stars} />
                    <span className="text-muted text-sm tabular-nums">{c.rating.stars} / 5</span>
                  </div>
                  {c.rating.comment && <p className="mt-2 text-sm">“{c.rating.comment}”</p>}
                </Card>
              )}
            </>
          )}

          {historyTab ? (
            tab === 'history' && (
              <Card>
                <CardHeader
                  title="History"
                  aside={`${c.history.length} ${c.history.length === 1 ? 'change' : 'changes'}`}
                />
                <StatusTimeline history={c.history} />
              </Card>
            )
          ) : (
            <>
              {/* 16/11: the history is the case's own story, so it reads in the main column (the right rail
              keeps the deadline and the people); before, the main column ended early beside a long rail. */}
              <Card>
                <CardHeader
                  title="History"
                  aside={`${c.history.length} ${c.history.length === 1 ? 'change' : 'changes'}`}
                />
                {/* Meeting 07/11: a long history scrolls inside its card instead of stretching the page. */}
                <div
                  tabIndex={0}
                  aria-label="Status history"
                  className="rounded-control focus-visible:ring-accent/20 -mx-1 max-h-96 overflow-y-auto px-1 focus-visible:ring-2 focus-visible:outline-none"
                >
                  <StatusTimeline history={c.history} />
                </div>
              </Card>
            </>
          )}
        </div>

        <div className="flex flex-col gap-6">
          <EmergencyCard c={c} now={query.dataUpdatedAt} />
          {c.decisionInfo && <DecisionInfoCard info={c.decisionInfo} />}
          <SlaPanel c={c} />

          <Card>
            <CardHeader title="People" />
            <dl className="flex flex-col gap-3 text-sm">
              {c.isPreventive ? (
                <Field label="Made by" inline>
                  {c.planId && seesRejections ? (
                    <Link to={planDetailPath(c.planId)} className="underline underline-offset-2">
                      Maintenance plan
                    </Link>
                  ) : (
                    'Maintenance plan'
                  )}
                </Field>
              ) : (
                <Field label="Reported by" inline>
                  {c.requester?.fullName ?? '—'}
                </Field>
              )}
              <Field label="Staff in charge" inline>
                {c.staff?.fullName ?? '—'}
              </Field>
              <Field label="Technician" inline>
                {c.technician?.fullName ?? <span className="text-muted">Not assigned</span>}
              </Field>
              {seesWork && (c.workedHours !== null || c.clockStartedAt) && (
                <Field label="Work hours" inline>
                  <span className="tabular-nums">
                    {workedHoursNow(c, query.dataUpdatedAt).toFixed(2)} h
                    {c.clockStartedAt && <span className="text-muted"> · running</span>}
                  </span>
                </Field>
              )}
              <Field label="Urgency" inline>
                {/* one word, coloured above Normal (was the word twice: "High" over "High urgency") */}
                <UrgencyText urgency={c.urgency} />
              </Field>
              {(c.plannedStartAt || c.plannedEndAt) && (
                <Field label="Planned" inline>
                  <span className="flex flex-col items-end tabular-nums">
                    {c.plannedStartAt && <span>from {formatDateTime(c.plannedStartAt)}</span>}
                    {c.plannedEndAt && <span>to {formatDateTime(c.plannedEndAt)}</span>}
                  </span>
                </Field>
              )}
              {c.followers.length > 0 && (
                <Field label="Also followed by" inline>
                  {c.followers.map((f) => f.fullName).join(', ')}
                </Field>
              )}
            </dl>
          </Card>

          {seesRejections && c.rejections.length > 0 && (
            <Card>
              <CardHeader title="Declined by" aside={`${c.rejections.length} in total`} />
              <RejectionList rejections={c.rejections} />
            </Card>
          )}
        </div>
      </div>
    </div>
  )
}

function Field({
  label,
  inline,
  children,
}: {
  label: string
  inline?: boolean
  children: ReactNode
}) {
  if (inline) {
    return (
      <div className="flex items-baseline justify-between gap-3">
        <dt className="text-muted">{label}</dt>
        <dd className="text-right">{children}</dd>
      </div>
    )
  }
  return (
    <div>
      <dt className="text-muted mb-0.5 text-xs">{label}</dt>
      <dd>{children}</dd>
    </div>
  )
}

/** Loading: the final layout with grey bars at their real sizes. */
function DetailSkeleton({ above }: { above: ReactNode }) {
  return (
    <div className="flex flex-col gap-5" aria-busy="true" aria-label="Loading case">
      <div>
        {above && <div className="text-muted mb-1 text-sm">{above}</div>}
        <div className="bg-raised h-7 w-80 rounded" />
        <div className="bg-raised mt-2 h-4 w-56 rounded" />
      </div>
      <div className="border-border h-[78px] border-y" />
      <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_360px]">
        <div className="border-border rounded-card h-72 border" />
        <div className="border-border rounded-card h-48 border" />
      </div>
    </div>
  )
}
