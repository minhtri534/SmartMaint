import { zodResolver } from '@hookform/resolvers/zod'
import { useState, type ReactNode } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useParams } from 'react-router'
import { caseDetailPath, paths, planEditPath } from '@/app/paths'
import { StatusBadge } from '@/features/cases'
import {
  Alert,
  Button,
  Card,
  CardHeader,
  ConfirmDialog,
  Dialog,
  DialogActions,
  EmptyBlock,
  ErrorBlock,
  LinkButton,
  LoadingBlock,
  PageHeader,
  TextArea,
} from '@/shared/ui'
import { formatDate, formatDateTime } from '@/shared/utils/format'
import { maintenanceApi } from '../api'
import { PlanStatusBadge } from '../components/PlanStatusBadge'
import { useMaintenanceMutation, usePlan } from '../hooks'
import { rejectPlanSchema, type RejectPlanValues } from '../schemas'
import { everyText, type PlanAction, type PlanDetail } from '../types'

const URGENCY_LABELS = { emergency: 'Emergency', high: 'High', normal: 'Normal', low: 'Low' }

/** PM-03 One plan: its schedule, its assets with their next due day, and the work orders it made. */
export function PlanDetailPage() {
  const id = Number(useParams().id)
  const plan = usePlan(id)
  if (plan.isPending) return <LoadingBlock rows={6} />
  if (plan.error)
    return (
      <ErrorBlock
        what="the plan"
        message={plan.error.message}
        onRetry={() => void plan.refetch()}
      />
    )
  return <PlanView p={plan.data} />
}

type Confirm = Extract<PlanAction, 'approve' | 'pause' | 'resume' | 'end'>

const CONFIRMS: Record<Confirm, { title: string; text: string; label: string }> = {
  approve: {
    title: 'Approve plan',
    text: 'Work orders are made from now on, a few days before each due day.',
    label: 'Approve',
  },
  pause: {
    title: 'Pause plan',
    text: 'No work order is made while it is paused. Work orders already made stay.',
    label: 'Pause',
  },
  resume: {
    title: 'Resume plan',
    text: 'Due days missed while paused are skipped: the next one is in the future.',
    label: 'Resume',
  },
  end: {
    title: 'End plan',
    text: 'The plan stops for good. Work orders already made stay.',
    label: 'End plan',
  },
}

function PlanView({ p }: { p: PlanDetail }) {
  const [confirm, setConfirm] = useState<Confirm | null>(null)
  const [rejecting, setRejecting] = useState(false)
  const approve = useMaintenanceMutation(maintenanceApi.approve)
  const pause = useMaintenanceMutation(maintenanceApi.pause)
  const resume = useMaintenanceMutation(maintenanceApi.resume)
  const end = useMaintenanceMutation(maintenanceApi.end)
  const mutations = { approve, pause, resume, end }
  const can = (a: PlanAction) => p.allowedActions.includes(a)
  const running = confirm ? mutations[confirm] : null

  const finding = [
    everyText(p.frequency, p.frequencyType).toLowerCase(),
    `${p.assetCount} ${p.assetCount === 1 ? 'asset' : 'assets'}`,
    p.nextDueDate && `next due ${formatDate(p.nextDueDate)}`,
  ].filter(Boolean)

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        above={
          <LinkButton variant="ghost" to={paths.plans} className="-ml-3">
            Maintenance plans
          </LinkButton>
        }
        title={p.name}
        description={`${p.buildingName}: ${finding.join(', ')}.`}
        actions={
          <>
            {can('end') && (
              <Button variant="danger" onClick={() => (end.reset(), setConfirm('end'))}>
                End plan
              </Button>
            )}
            {can('reject') && (
              <Button variant="secondary" onClick={() => setRejecting(true)}>
                Reject
              </Button>
            )}
            {can('pause') && (
              <Button variant="secondary" onClick={() => (pause.reset(), setConfirm('pause'))}>
                Pause
              </Button>
            )}
            {can('edit') && (
              <LinkButton variant="secondary" to={planEditPath(p.id)}>
                Edit
              </LinkButton>
            )}
            {can('resume') && (
              <Button variant="primary" onClick={() => (resume.reset(), setConfirm('resume'))}>
                Resume
              </Button>
            )}
            {can('approve') && (
              <Button variant="primary" onClick={() => (approve.reset(), setConfirm('approve'))}>
                Approve
              </Button>
            )}
          </>
        }
      />

      {p.status === 'rejected' && p.rejectReason && (
        <Alert tone="info">
          Rejected{p.approvingManager && ` by ${p.approvingManager.fullName}`}: {p.rejectReason}.
          Edit the plan to send it again.
        </Alert>
      )}
      {p.status === 'pending_approval' && (
        <Alert tone="info">
          Waiting for the building manager. No work order is made until the plan is approved.
        </Alert>
      )}

      <div className="grid items-start gap-6 lg:grid-cols-[minmax(0,1fr)_340px]">
        <div className="flex min-w-0 flex-col gap-6">
          <Card>
            <CardHeader title="Assets" aside={`${p.assets.length} in this plan`} />
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-border text-muted border-b text-xs">
                  <th className="h-9 pr-4 font-normal">Asset</th>
                  <th className="hidden h-9 pr-4 font-normal sm:table-cell">Technician</th>
                  <th className="h-9 pr-4 font-normal">Last made</th>
                  <th className="h-9 text-right font-normal">Next due</th>
                </tr>
              </thead>
              <tbody>
                {p.assets.map((a) => (
                  <tr key={a.assetId} className="border-rule border-b align-top last:border-0">
                    <td className="py-3 pr-4">
                      <div className="font-medium">{a.assetName}</div>
                      <div className="text-muted text-[13px]">
                        {a.roomName}
                        {a.isRetired && ' · retired, skipped'}
                      </div>
                    </td>
                    <td className="hidden py-3 pr-4 sm:table-cell">
                      {a.technician?.fullName ?? <span className="text-muted">Staff assigns</span>}
                    </td>
                    <td className="py-3 pr-4 whitespace-nowrap tabular-nums">
                      {formatDate(a.lastDueDate)}
                    </td>
                    <td className="py-3 text-right whitespace-nowrap tabular-nums">
                      {formatDate(a.nextDueDate)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </Card>

          <Card>
            <CardHeader title="Work orders" aside={`${p.workOrders.length} made so far`} />
            {p.workOrders.length === 0 ? (
              <EmptyBlock>No work order made yet.</EmptyBlock>
            ) : (
              <ul className="flex flex-col text-sm">
                {p.workOrders.map((w) => (
                  <li
                    key={w.id}
                    className="border-rule flex items-start justify-between gap-3 border-b py-2.5 first:pt-0 last:border-0"
                  >
                    <div className="min-w-0">
                      <Link
                        to={caseDetailPath(w.id)}
                        className="font-mono text-xs underline underline-offset-2"
                      >
                        {w.code}
                      </Link>{' '}
                      {w.assetName}
                      <div className="text-muted text-[13px]">
                        {w.closedAt
                          ? `closed ${formatDateTime(w.closedAt)}`
                          : `due ${formatDateTime(w.dueAt)}`}
                      </div>
                    </div>
                    <StatusBadge status={w.status} />
                  </li>
                ))}
              </ul>
            )}
          </Card>
        </div>

        <div className="flex flex-col gap-6">
          <Card>
            <CardHeader title="Schedule" aside={<PlanStatusBadge status={p.status} />} />
            <dl className="flex flex-col gap-3 text-sm">
              <Row label="Repeats">{everyText(p.frequency, p.frequencyType)}</Row>
              <Row label="Made ahead">
                {p.leadDays} {p.leadDays === 1 ? 'day' : 'days'} before due
              </Row>
              <Row label="Checklist">{p.checklistName}</Row>
              <Row label="Urgency">{URGENCY_LABELS[p.priority]}</Row>
              <Row label="Estimated">
                {p.estimatedHours ? `${p.estimatedHours} h per asset` : '—'}
              </Row>
              <Row label="Last run">{formatDateTime(p.lastTriggered)}</Row>
              {p.lastPaused && <Row label="Last paused">{formatDateTime(p.lastPaused)}</Row>}
            </dl>
          </Card>
          <Card>
            <CardHeader title="People" />
            <dl className="flex flex-col gap-3 text-sm">
              <Row label="Made by">{p.staff.fullName}</Row>
              <Row label={p.status === 'rejected' ? 'Rejected by' : 'Approved by'}>
                {p.approvingManager?.fullName ?? '—'}
              </Row>
            </dl>
          </Card>
        </div>
      </div>

      {confirm && running && (
        <ConfirmDialog
          open
          onClose={() => setConfirm(null)}
          title={CONFIRMS[confirm].title}
          description={CONFIRMS[confirm].text}
          confirmLabel={CONFIRMS[confirm].label}
          loading={running.isPending}
          error={running.error?.message}
          onConfirm={() => running.mutate({ id: p.id }, { onSuccess: () => setConfirm(null) })}
        />
      )}
      {rejecting && (
        <Dialog
          open
          onClose={() => setRejecting(false)}
          title="Reject plan"
          description="The staff member reads your reason, edits the plan and sends it again."
        >
          <RejectForm planId={p.id} onDone={() => setRejecting(false)} />
        </Dialog>
      )}
    </div>
  )
}

function RejectForm({ planId, onDone }: { planId: number; onDone: () => void }) {
  const reject = useMaintenanceMutation(maintenanceApi.reject)
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RejectPlanValues>({
    resolver: zodResolver(rejectPlanSchema),
    defaultValues: { reason: '' },
  })
  return (
    <form
      onSubmit={handleSubmit((v) => reject.mutate({ id: planId, ...v }, { onSuccess: onDone }))}
      noValidate
      className="flex flex-col gap-4"
    >
      <TextArea
        label="Reason *"
        placeholder="e.g. Monthly is too often for these units, make it quarterly."
        rows={3}
        error={errors.reason?.message}
        {...register('reason')}
      />
      {reject.error && <Alert>{reject.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="danger" loading={reject.isPending}>
          Reject
        </Button>
      </DialogActions>
    </form>
  )
}

function Row({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="flex items-baseline justify-between gap-3">
      <dt className="text-muted">{label}</dt>
      <dd className="text-right">{children}</dd>
    </div>
  )
}
