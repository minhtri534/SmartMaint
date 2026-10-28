import type { ReactNode } from 'react'
import { Link, useParams } from 'react-router'
import { paths, resubmitPath } from '@/app/paths'
import type { Role } from '@/shared/types/role'
import { Alert, Button, Card, CardHeader, Icon, LinkButton, PageHeader } from '@/shared/ui'
import { formatDateTime } from '@/shared/utils/format'
import { CaseActions } from '../components/CaseActions'
import { UrgencyNote } from '../components/CaseBadges'
import { AssignmentList, PhotoGallery, StatusTimeline } from '../components/CaseHistory'
import { MockDataBanner } from '../components/MockDataBanner'
import { SlaPanel } from '../components/SlaTrack'
import { StarsView } from '../components/Stars'
import { StatusPath } from '../components/StatusPath'
import { useActor, useCase } from '../hooks'
import { COST_BEARER_LABELS, URGENCY_LABELS } from '../types'

const BACK: Partial<Record<Role, { to: string; label: string }>> = {
  customer: { to: paths.myRequests, label: 'My requests' },
  officer: { to: paths.cases, label: 'Cases' },
  manager: { to: paths.cases, label: 'All cases' },
  technician: { to: paths.workOrders, label: 'My work orders' },
}

/**
 * COM-08 Case detail for every role; for the technician it is also TEC-03 (work order).
 * Question: "Where is this case, who has it, and is it on time?" — the road through F3 on top, the
 * deadline as the largest figure on the right. What each role can press comes from the state machine.
 */
export function CaseDetailPage() {
  const id = Number(useParams().id)
  const actor = useActor()
  const query = useCase(id)
  const back = actor ? BACK[actor.role] : undefined

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
  const seesAssignments = actor?.role === 'officer' || actor?.role === 'manager'
  const lastChange = c.history.at(-1)

  return (
    <div className="flex flex-col gap-5">
      <MockDataBanner />
      <PageHeader
        above={above}
        title={c.title}
        description={
          <>
            <span className="font-mono">{c.code}</span> · {c.serviceTypeName}
          </>
        }
        actions={actor && <CaseActions caseDetail={c} actor={actor} />}
      />

      <div className="border-border border-y py-4">
        <StatusPath c={c} />
      </div>

      {c.isEmergencyFlow && (
        <Alert>
          Emergency case: handled by the emergency flow (F4), so F3 actions are off here.
        </Alert>
      )}
      {c.status === 'rejected' && lastChange?.reason && (
        <Alert tone="info">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <span>
              Rejected by {lastChange.by.fullName}: {lastChange.reason}
            </span>
            {actor?.role === 'customer' && (
              <LinkButton to={resubmitPath(c.id)} variant="primary">
                Submit again
              </LinkButton>
            )}
          </div>
        </Alert>
      )}
      {c.status === 'reopened' && lastChange?.reason && (
        <Alert>
          Reopened by {lastChange.by.fullName}: {lastChange.reason}
        </Alert>
      )}

      <div className="grid items-start gap-6 lg:grid-cols-[minmax(0,1fr)_360px]">
        <div className="flex min-w-0 flex-col gap-6">
          <Card>
            <CardHeader title="Problem" aside={`Reported ${formatDateTime(c.submittedAt)}`} />
            <p className="max-w-[68ch] text-sm leading-6 whitespace-pre-line">{c.description}</p>
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

          {c.repairReport && (
            <Card>
              <CardHeader
                title="Repair record"
                aside={c.technician ? `by ${c.technician.fullName}` : undefined}
              />
              <p className="max-w-[68ch] text-sm leading-6">{c.repairReport.workDone}</p>
              <dl className="border-rule mt-5 grid gap-4 border-t pt-4 text-sm sm:grid-cols-[minmax(0,1fr)_minmax(0,1fr)_96px]">
                <Field label="Cause">{c.repairReport.rootCause}</Field>
                <Field label="Parts used">
                  {c.repairReport.partsUsed || <span className="text-muted">—</span>}
                </Field>
                <Field label="Hours worked">
                  <span className="tabular-nums">{c.repairReport.laborHours} h</span>
                </Field>
              </dl>
            </Card>
          )}

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
        </div>

        <div className="flex flex-col gap-6">
          <SlaPanel c={c} />

          <Card>
            <CardHeader title="People" />
            <dl className="flex flex-col gap-3 text-sm">
              <Field label="Reported by" inline>
                {c.requester.fullName}
              </Field>
              <Field label="Officer in charge" inline>
                {c.officer?.fullName ?? '—'}
              </Field>
              <Field label="Technician" inline>
                {c.technician?.fullName ?? <span className="text-muted">Not assigned</span>}
              </Field>
              <Field label="Urgency" inline>
                <span className="flex flex-col items-end">
                  {URGENCY_LABELS[c.urgency]}
                  <UrgencyNote urgency={c.urgency} />
                </span>
              </Field>
              <Field label="Who pays" inline>
                {COST_BEARER_LABELS[c.costBearer]}
              </Field>
            </dl>
          </Card>

          {seesAssignments && (
            <Card>
              <CardHeader title="Assignments" aside={`${c.assignments.length} in total`} />
              <AssignmentList assignments={c.assignments} />
            </Card>
          )}

          <Card>
            <CardHeader title="History" />
            <StatusTimeline history={c.history} />
          </Card>
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
