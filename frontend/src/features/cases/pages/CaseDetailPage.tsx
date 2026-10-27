import type { ReactNode } from 'react'
import { Link, useParams } from 'react-router'
import { paths, resubmitPath } from '@/app/paths'
import type { Role } from '@/shared/types/role'
import { Alert, Card, LinkButton, Spinner } from '@/shared/ui'
import { formatDateTime } from '@/shared/utils/format'
import { CaseActions } from '../components/CaseActions'
import { StatusBadge, UrgencyBadge } from '../components/CaseBadges'
import { AssignmentList, PhotoGallery, StatusTimeline } from '../components/CaseHistory'
import { MockDataBanner } from '../components/MockDataBanner'
import { useActor, useCase } from '../hooks'
import { isOverdue } from '../state-machine'
import { COST_BEARER_LABELS, type CaseDetail } from '../types'

const BACK: Partial<Record<Role, { to: string; label: string }>> = {
  customer: { to: paths.myRequests, label: 'My requests' },
  officer: { to: paths.cases, label: 'Cases' },
  manager: { to: paths.cases, label: 'All cases' },
  technician: { to: paths.workOrders, label: 'My work orders' },
}

/**
 * COM-08 Case detail for every role; for the technician it is also TEC-03 (work order).
 * What each role can press comes from the state machine (CaseActions).
 */
export function CaseDetailPage() {
  const id = Number(useParams().id)
  const actor = useActor()
  const query = useCase(id)

  if (query.isPending) {
    return (
      <div className="text-muted flex justify-center py-16">
        <Spinner className="size-8" />
      </div>
    )
  }
  if (query.error) return <Alert>{query.error.message}</Alert>

  const c = query.data
  const back = actor ? BACK[actor.role] : undefined
  const seesAssignments = actor?.role === 'officer' || actor?.role === 'manager'
  const lastChange = c.history.at(-1)

  return (
    <div className="flex flex-col gap-4">
      <MockDataBanner />
      {back && (
        <Link to={back.to} className="text-muted hover:text-ink text-sm">
          ← {back.label}
        </Link>
      )}

      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="flex flex-col gap-2">
          <div className="text-muted text-sm">
            {c.code} · {c.serviceTypeName}
          </div>
          <h1 className="text-2xl font-semibold">{c.title}</h1>
          <div className="flex flex-wrap gap-2">
            <StatusBadge status={c.status} />
            <UrgencyBadge urgency={c.urgency} />
          </div>
        </div>
        {actor && <CaseActions caseDetail={c} actor={actor} />}
      </div>

      {c.isEmergencyFlow && (
        <Alert>
          Emergency case — handled by the emergency flow (F4). F3 actions are not available here.
        </Alert>
      )}
      {c.status === 'rejected' && lastChange?.reason && (
        <Alert tone="info">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <span>
              Rejected by {lastChange.by.fullName}: {lastChange.reason}
            </span>
            {actor?.role === 'customer' && (
              <LinkButton to={resubmitPath(c.id)} variant="secondary" className="h-8">
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

      <div className="grid gap-4 lg:grid-cols-3">
        <div className="flex flex-col gap-4 lg:col-span-2">
          <Section title="Problem">
            <p className="text-sm whitespace-pre-line">{c.description}</p>
            <dl className="mt-4 grid gap-3 text-sm sm:grid-cols-2">
              <Field label="Location">
                {c.location.roomName}
                <div className="text-muted text-xs">
                  {c.location.buildingName} · {c.location.floorName}
                </div>
              </Field>
              <Field label="Asset">{c.asset?.name ?? 'Not specified'}</Field>
            </dl>
            <div className="mt-4">
              <PhotoGallery photos={c.photos} />
            </div>
          </Section>

          {c.repairReport && (
            <Section title="Repair record">
              <dl className="grid gap-3 text-sm sm:grid-cols-2">
                <Field label="Work done" wide>
                  {c.repairReport.workDone}
                </Field>
                <Field label="Cause">{c.repairReport.rootCause}</Field>
                <Field label="Hours worked">{c.repairReport.laborHours} h</Field>
                <Field label="Parts used" wide>
                  {c.repairReport.partsUsed || 'None'}
                </Field>
              </dl>
            </Section>
          )}

          {c.rating && (
            <Section title="Customer rating">
              <div className="text-warning text-lg">
                {'★'.repeat(c.rating.stars)}
                <span className="text-line">{'★'.repeat(5 - c.rating.stars)}</span>
              </div>
              {c.rating.comment && <p className="mt-1 text-sm">“{c.rating.comment}”</p>}
            </Section>
          )}
        </div>

        <div className="flex flex-col gap-4">
          <Section title="Details">
            <dl className="flex flex-col gap-3 text-sm">
              <Field label="Reported by">{c.requester.fullName}</Field>
              <Field label="Officer in charge">{c.officer?.fullName ?? '—'}</Field>
              <Field label="Technician">{c.technician?.fullName ?? 'Not assigned'}</Field>
              <Field label="Who pays">{COST_BEARER_LABELS[c.costBearer]}</Field>
              <Field label="Submitted">{formatDateTime(c.submittedAt)}</Field>
              <Field label="Due">
                <DueText c={c} />
              </Field>
              {c.completedAt && <Field label="Completed">{formatDateTime(c.completedAt)}</Field>}
            </dl>
          </Section>

          {seesAssignments && (
            <Section title="Assignments">
              <AssignmentList assignments={c.assignments} />
            </Section>
          )}

          <Section title="History">
            <StatusTimeline history={c.history} />
          </Section>
        </div>
      </div>
    </div>
  )
}

function DueText({ c }: { c: CaseDetail }) {
  if (!c.dueAt) return <span className="text-muted">Starts when the case is approved</span>
  const extended = c.slaDueAt !== null && c.slaDueAt !== c.dueAt
  return (
    <>
      <span className={isOverdue(c) ? 'text-danger font-medium' : undefined}>
        {formatDateTime(c.dueAt)}
        {isOverdue(c) && ' · overdue'}
      </span>
      {extended && (
        <div className="text-muted text-xs">SLA deadline was {formatDateTime(c.slaDueAt)}</div>
      )}
    </>
  )
}

function Section({ title, children }: { title: string; children: ReactNode }) {
  return (
    <Card className="p-4">
      <h2 className="mb-3 text-sm font-semibold">{title}</h2>
      {children}
    </Card>
  )
}

function Field({ label, wide, children }: { label: string; wide?: boolean; children: ReactNode }) {
  return (
    <div className={wide ? 'sm:col-span-2' : undefined}>
      <dt className="text-muted text-xs">{label}</dt>
      <dd>{children}</dd>
    </div>
  )
}
