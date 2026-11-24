import { zodResolver } from '@hookform/resolvers/zod'
import { useState, type ReactNode } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useParams, useSearchParams } from 'react-router'
import { caseDetailPath, paths, planEditPath } from '@/app/paths'
import { FINAL_STATUSES, StatusBadge } from '@/features/cases'
import {
  Alert,
  Badge,
  Button,
  Card,
  CardHeader,
  ConfirmDialog,
  Dialog,
  DialogActions,
  EmptyBlock,
  ErrorBlock,
  FilterSelect,
  LinkButton,
  LoadingBlock,
  PageHeader,
  SearchInput,
  SortHeader,
  sortRows,
  Tabs,
  TextArea,
  useTableSort,
} from '@/shared/ui'
import { formatDate, formatDateTime } from '@/shared/utils/format'
import { maintenanceApi } from '../api'
import { PlanStatusBadge } from '../components/PlanStatusBadge'
import { useMaintenanceMutation, usePlan, usePlanWorkOrders } from '../hooks'
import { rejectPlanSchema, type RejectPlanValues } from '../schemas'
import {
  everyListText,
  everyText,
  type PlanAction,
  type PlanDetail,
  type PlanTrigger,
} from '../types'

const URGENCY_LABELS = { emergency: 'Emergency', high: 'High', normal: 'Normal', low: 'Low' }

type Tab = 'details' | 'triggers' | 'work-orders'

/**
 * PM-03 One plan, like UpKeep (meeting note 18/11): its work-order template, its triggers (schedule, end day, assets)
 * and every work order it made, each on its own tab.
 */
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

type Confirm = Extract<PlanAction, 'approve' | 'pause' | 'resume' | 'archive'>

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
  archive: {
    title: 'Archive plan',
    text: 'For good: the plan can no longer be edited, extended or resumed, and makes no more work orders. Work orders already made go on. To stop it for a while, pause it instead.',
    label: 'Archive',
  },
}

function PlanView({ p }: { p: PlanDetail }) {
  const [params, setParams] = useSearchParams()
  const tab: Tab =
    (['triggers', 'work-orders'] as const).find((t) => t === params.get('tab')) ?? 'details'
  const [confirm, setConfirm] = useState<Confirm | null>(null)
  const [rejecting, setRejecting] = useState(false)
  const approve = useMaintenanceMutation(maintenanceApi.approve)
  const pause = useMaintenanceMutation(maintenanceApi.pause)
  const resume = useMaintenanceMutation(maintenanceApi.resume)
  const archive = useMaintenanceMutation(maintenanceApi.archive)
  const mutations = { approve, pause, resume, archive }
  const can = (a: PlanAction) => p.allowedActions.includes(a)
  const running = confirm ? mutations[confirm] : null
  const open = (c: Confirm) => (mutations[c].reset(), setConfirm(c))

  const finding = [
    everyListText(p.every).toLowerCase(),
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
            {can('archive') && (
              <Button variant="danger" onClick={() => open('archive')}>
                Archive
              </Button>
            )}
            {can('reject') && (
              <Button variant="secondary" onClick={() => setRejecting(true)}>
                Reject
              </Button>
            )}
            {can('pause') && (
              <Button variant="secondary" onClick={() => open('pause')}>
                Pause
              </Button>
            )}
            {can('edit') && (
              <LinkButton variant={p.expired ? 'primary' : 'secondary'} to={planEditPath(p.id)}>
                {p.expired ? 'Extend' : 'Edit'}
              </LinkButton>
            )}
            {can('resume') && (
              <Button variant="primary" onClick={() => open('resume')}>
                Resume
              </Button>
            )}
            {can('approve') && (
              <Button variant="primary" onClick={() => open('approve')}>
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
      {p.expired && (
        <Alert tone="info">
          Every trigger has passed its end day, so no work order is made.{' '}
          {can('edit')
            ? 'Extend it by moving an end day later: it keeps running without a new approval.'
            : 'The staff member can extend it by moving an end day later.'}
        </Alert>
      )}
      {p.status === 'archived' && (
        <Alert tone="info">
          Archived {formatDateTime(p.archivedAt)}. Read only: it makes no more work orders.
        </Alert>
      )}

      <Tabs
        items={[
          { value: 'details', label: 'Details' },
          { value: 'triggers', label: 'Triggers', count: p.triggers.length },
          { value: 'work-orders', label: 'Work orders', count: p.workOrderCount },
        ]}
        value={tab}
        onChange={(v) => setParams(v === 'details' ? {} : { tab: v }, { replace: true })}
      />

      {tab === 'details' && <DetailsTab p={p} />}
      {tab === 'triggers' && (
        <div className="flex flex-col gap-5">
          {p.triggers.map((t, i) => (
            <TriggerCard key={t.id} trigger={t} index={i} />
          ))}
        </div>
      )}
      {tab === 'work-orders' && <WorkOrdersTab p={p} />}

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

function DetailsTab({ p }: { p: PlanDetail }) {
  return (
    <div className="grid items-start gap-6 lg:grid-cols-[minmax(0,1fr)_340px]">
      <Card>
        <CardHeader title="Work order" aside="the same for every trigger" />
        <dl className="flex flex-col gap-3 text-sm">
          <Row label="Title">Preventive: {p.name}</Row>
          <Row label="Checklist">
            {p.checklistName} ({p.checklistItems} {p.checklistItems === 1 ? 'item' : 'items'})
          </Row>
          <Row label="Urgency">{URGENCY_LABELS[p.priority]}</Row>
          <Row label="Estimated">{p.estimatedHours ? `${p.estimatedHours} h per asset` : '—'}</Row>
          <Row label="Repeats">{everyListText(p.every)}</Row>
        </dl>
        {p.description && (
          <div className="border-rule mt-4 border-t pt-4 text-sm">
            <div className="text-muted mb-1 text-xs">Note for the technician</div>
            <p className="whitespace-pre-line">{p.description}</p>
          </div>
        )}
      </Card>
      <Card>
        <CardHeader
          title="Status"
          aside={<PlanStatusBadge status={p.status} expired={p.expired} />}
        />
        <dl className="flex flex-col gap-3 text-sm">
          <Row label="Made by">{p.staff.fullName}</Row>
          <Row label={p.status === 'rejected' ? 'Rejected by' : 'Approved by'}>
            {p.approvingManager?.fullName ?? '—'}
          </Row>
          <Row label="Created">{formatDate(p.createdAt)}</Row>
          <Row label="Next due">{formatDate(p.nextDueDate)}</Row>
          <Row label="Last run">{formatDateTime(p.lastTriggered)}</Row>
          {p.lastPaused && <Row label="Last paused">{formatDateTime(p.lastPaused)}</Row>}
          {p.archivedAt && <Row label="Archived">{formatDateTime(p.archivedAt)}</Row>}
        </dl>
      </Card>
    </div>
  )
}

function TriggerCard({ trigger: t, index }: { trigger: PlanTrigger; index: number }) {
  return (
    <Card>
      <CardHeader
        title={`Trigger ${index + 1} · ${everyText(t.frequency, t.frequencyType)}`}
        aside={
          t.expired ? (
            <Badge tone="warning">Past its end day</Badge>
          ) : (
            t.nextDueDate && `next due ${formatDate(t.nextDueDate)}`
          )
        }
      />
      <dl className="mb-4 grid gap-3 text-sm sm:grid-cols-3">
        <Fact label="First due day">{formatDate(t.startDate)}</Fact>
        <Fact label="End day">{t.endDate ? formatDate(t.endDate) : 'No end'}</Fact>
        <Fact label="Made ahead">
          {t.leadDays} {t.leadDays === 1 ? 'day' : 'days'} before due
        </Fact>
      </dl>
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
          {t.assets.map((a) => (
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
  )
}

type WorkOrderSort = 'code' | 'asset' | 'status' | 'due'

/** The whole history of the work orders the plan made (UpKeep's "Work Orders" tab), with search, filters and sort. */
function WorkOrdersTab({ p }: { p: PlanDetail }) {
  const workOrders = usePlanWorkOrders(p.id)
  const [search, setSearch] = useState('')
  const [state, setState] = useState<'' | 'open' | 'finished'>('')
  const [trigger, setTrigger] = useState('')
  const { sort, toggle } = useTableSort<WorkOrderSort>({ key: 'due', dir: 'desc' })
  const triggerNo = new Map(p.triggers.map((t, i) => [t.id, i + 1]))
  const words = search.trim().toLowerCase()
  const rows = sortRows(
    (workOrders.data ?? []).filter(
      (w) =>
        (!words ||
          w.code.toLowerCase().includes(words) ||
          w.assetName.toLowerCase().includes(words)) &&
        (state === '' || FINAL_STATUSES.includes(w.status) === (state === 'finished')) &&
        (trigger === '' || String(w.triggerId) === trigger),
    ),
    sort,
    {
      code: (w) => w.id,
      asset: (w) => w.assetName,
      status: (w) => w.status,
      due: (w) => w.dueAt,
    },
  )

  if (workOrders.isPending) return <LoadingBlock rows={5} />
  if (workOrders.error)
    return (
      <ErrorBlock
        what="the work orders"
        message={workOrders.error.message}
        onRetry={() => void workOrders.refetch()}
      />
    )

  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-wrap items-center gap-2">
        <SearchInput
          label="Search work orders"
          placeholder="Code or asset"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <FilterSelect
          label="State"
          value={state}
          onChange={(e) => setState(e.target.value as typeof state)}
        >
          <option value="">All</option>
          <option value="open">Open</option>
          <option value="finished">Finished</option>
        </FilterSelect>
        {p.triggers.length > 1 && (
          <FilterSelect
            label="Trigger"
            value={trigger}
            onChange={(e) => setTrigger(e.target.value)}
          >
            <option value="">All</option>
            {p.triggers.map((t, i) => (
              <option key={t.id} value={t.id}>
                {i + 1} · {everyText(t.frequency, t.frequencyType)}
              </option>
            ))}
          </FilterSelect>
        )}
      </div>
      {rows.length === 0 ? (
        <EmptyBlock>
          {workOrders.data.length === 0 ? 'No work order made yet.' : 'No work order matches.'}
        </EmptyBlock>
      ) : (
        <table className="w-full text-left text-sm">
          <thead>
            <tr className="border-border text-muted border-b text-xs">
              <SortHeader sortKey="code" sort={sort} onSort={toggle}>
                Work order
              </SortHeader>
              <SortHeader sortKey="asset" sort={sort} onSort={toggle}>
                Asset
              </SortHeader>
              <th className="hidden h-9 pr-4 font-normal md:table-cell">Technician</th>
              <SortHeader sortKey="status" sort={sort} onSort={toggle}>
                Status
              </SortHeader>
              <SortHeader sortKey="due" sort={sort} onSort={toggle} align="right" className="pr-0">
                Due
              </SortHeader>
            </tr>
          </thead>
          <tbody>
            {rows.map((w) => (
              <tr key={w.id} className="border-rule border-b align-top last:border-0">
                <td className="py-3 pr-4 whitespace-nowrap">
                  <Link
                    to={caseDetailPath(w.id)}
                    className="font-mono text-xs underline underline-offset-2"
                  >
                    {w.code}
                  </Link>
                  {p.triggers.length > 1 && (
                    <div className="text-muted text-[13px]">
                      trigger {triggerNo.get(w.triggerId)}
                    </div>
                  )}
                </td>
                <td className="py-3 pr-4">{w.assetName}</td>
                <td className="hidden py-3 pr-4 md:table-cell">
                  {w.technicianName ?? <span className="text-muted">Not assigned</span>}
                </td>
                <td className="py-3 pr-4">
                  <StatusBadge status={w.status} />
                </td>
                <td className="py-3 text-right whitespace-nowrap tabular-nums">
                  {formatDate(w.dueAt)}
                  {w.closedAt && (
                    <div className="text-muted text-[13px]">closed {formatDate(w.closedAt)}</div>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
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

function Fact({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div>
      <dt className="text-muted text-xs">{label}</dt>
      <dd className="mt-0.5 tabular-nums">{children}</dd>
    </div>
  )
}
