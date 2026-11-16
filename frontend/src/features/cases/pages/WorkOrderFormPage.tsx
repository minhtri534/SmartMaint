import { zodResolver } from '@hookform/resolvers/zod'
import type { ReactNode } from 'react'
import { useFieldArray, useForm, useWatch } from 'react-hook-form'
import { Link, useLocation, useNavigate, useParams } from 'react-router'
import { caseDetailPath } from '@/app/paths'
import {
  Alert,
  Button,
  Card,
  CardHeader,
  ErrorBlock,
  Icon,
  LinkButton,
  LoadingBlock,
  PageHeader,
  Select,
  TextArea,
  TextField,
} from '@/shared/ui'
import { formatAssetCode } from '@/shared/utils/asset-code'
import { formatDateTime } from '@/shared/utils/format'
import { casesApi } from '../api'
import { TechnicianPicker } from '../components/TechnicianPicker'
import {
  useCase,
  useCaseMutation,
  useFollowerOptions,
  useServiceTypes,
  useTechnicians,
} from '../hooks'
import { workOrderSchema, type WorkOrderFormValues } from '../schemas'
import {
  DONE_BY_LABELS,
  STATUS_CHANGE_LABELS,
  STATUS_LABELS,
  TASK_STATUS_LABELS,
  URGENCIES,
  URGENCY_LABELS,
  type CaseDetail,
  type CaseStatus,
  type PersonRef,
  type ServiceType,
  type TaskLine,
  type TechnicianOption,
} from '../types'

/** The technician already took the job: a new one means a new case (meeting 07/11, Leader 09/11). */
const TAKEN: readonly CaseStatus[] = ['accepted', 'in_progress', 'on_hold']

/**
 * The staff's work order form (meeting 07/11), at /cases/:id/approve and /cases/:id/edit. It looks like the customer's
 * request form, plus the staff's fields: technician (with their schedule and specialties), planned start / end and
 * the tasks. Edit also has the followers and the status change (continue / close / reopen), which replace the old
 * separate buttons.
 */
export function WorkOrderFormPage() {
  const id = Number(useParams().id)
  const mode = useLocation().pathname.endsWith('/approve') ? 'approve' : 'edit'
  const query = useCase(id)
  const serviceTypes = useServiceTypes()
  const technicians = useTechnicians(true)
  const followers = useFollowerOptions(id, mode === 'edit')

  const above = (
    <Link to={caseDetailPath(id)} className="hover:text-ink inline-flex items-center gap-1">
      <Icon name="back" className="size-3.5" />
      {query.data?.code ?? 'Case'}
    </Link>
  )
  const title = mode === 'approve' ? 'Approve and assign' : 'Edit case'

  const failed = query.error ?? serviceTypes.error ?? technicians.error ?? followers.error
  if (failed) {
    return (
      <div className="flex flex-col gap-5">
        <PageHeader above={above} title={title} />
        <ErrorBlock what="the case" message={failed.message} onRetry={() => void query.refetch()} />
      </div>
    )
  }
  if (
    !query.data ||
    !serviceTypes.data ||
    !technicians.data ||
    (mode === 'edit' && !followers.data)
  ) {
    return (
      <div className="flex flex-col gap-5">
        <PageHeader above={above} title={title} />
        <LoadingBlock rows={6} />
      </div>
    )
  }

  const c = query.data
  const allowed =
    mode === 'approve' ? c.allowedActions.includes('approve') : c.allowedActions.includes('edit')
  if (!allowed) {
    return (
      <div className="flex flex-col gap-5">
        <PageHeader above={above} title={title} />
        <Alert tone="info">
          This case can no longer be {mode === 'approve' ? 'approved' : 'edited'}. Go back to the
          case to see where it is.
        </Alert>
      </div>
    )
  }

  return (
    <WorkOrderForm
      mode={mode}
      caseDetail={c}
      above={above}
      serviceTypes={serviceTypes.data}
      technicians={technicians.data}
      followerOptions={followers.data ?? []}
    />
  )
}

interface FormProps {
  mode: 'approve' | 'edit'
  caseDetail: CaseDetail
  above: ReactNode
  serviceTypes: ServiceType[]
  technicians: TechnicianOption[]
  followerOptions: PersonRef[]
}

function WorkOrderForm({
  mode,
  caseDetail: c,
  above,
  serviceTypes,
  technicians,
  followerOptions,
}: FormProps) {
  const navigate = useNavigate()
  const approve = useCaseMutation(casesApi.approve)
  const edit = useCaseMutation(casesApi.edit)
  const saving = approve.isPending || edit.isPending
  const error = approve.error ?? edit.error
  const current = serviceTypes.find((s) => s.id === c.serviceTypeId)
  const finished = new Map(
    c.tasks.filter((t) => t.status !== 'pending' || t.doneBy).map((t) => [t.id, t]),
  )

  const {
    register,
    control,
    handleSubmit,
    setValue,
    setError,
    formState: { errors },
  } = useForm<WorkOrderFormValues>({
    resolver: zodResolver(workOrderSchema),
    defaultValues: {
      title: c.title,
      description: c.description,
      serviceGroup: current?.groupName ?? '',
      serviceTypeId: current ? String(current.id) : '',
      urgency: c.urgency,
      technicianId: c.technician ? String(c.technician.id) : '',
      plannedStartAt: toLocalInput(c.plannedStartAt),
      plannedEndAt: toLocalInput(c.plannedEndAt),
      tasks: c.tasks.map((t) => ({
        id: String(t.id),
        name: t.name,
        description: t.description ?? '',
      })),
      followerIds: c.followers.map((f) => String(f.id)),
      statusChange: '',
      reason: '',
    },
  })
  // keyName 'key': the rows have their own "id" (the task id).
  const tasks = useFieldArray({ control, name: 'tasks', keyName: 'key' })
  const [group, serviceTypeId, urgency, technicianId, statusChange] = useWatch({
    control,
    name: ['serviceGroup', 'serviceTypeId', 'urgency', 'technicianId', 'statusChange'],
  })
  const groups = [...new Set(serviceTypes.map((s) => s.groupName))]
  const taken = TAKEN.includes(c.status)
  const newCase = mode === 'edit' && taken && technicianId !== String(c.technician?.id ?? '')

  const onSubmit = handleSubmit((v) => {
    if (mode === 'approve' && !v.technicianId) {
      setError('technicianId', { message: 'Choose a technician.' })
      return
    }
    if (taken && !v.technicianId) {
      setError('technicianId', { message: 'The job was taken: choose who continues it.' })
      return
    }
    const body = {
      id: c.id,
      title: v.title,
      description: v.description,
      serviceTypeId: Number(v.serviceTypeId),
      urgency: v.urgency,
      plannedStartAt: fromLocalInput(v.plannedStartAt),
      plannedEndAt: fromLocalInput(v.plannedEndAt),
      tasks: v.tasks.map((t): TaskLine => ({
        id: t.id ? Number(t.id) : null,
        name: t.name,
        description: t.description || null,
      })),
    }
    if (mode === 'approve') {
      approve.mutate(
        { ...body, technicianId: Number(v.technicianId) },
        { onSuccess: () => navigate(caseDetailPath(c.id), { replace: true }) },
      )
      return
    }
    edit.mutate(
      {
        ...body,
        technicianId: v.technicianId ? Number(v.technicianId) : null,
        followerIds: v.followerIds.map(Number),
        statusChange: v.statusChange || null,
        reason: v.reason || null,
      },
      // A new technician on taken work opens the case that continues this one.
      { onSuccess: (r) => navigate(caseDetailPath(r.nextCase?.id ?? c.id), { replace: true }) },
    )
  })

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        above={above}
        title={mode === 'approve' ? 'Approve and assign' : `Edit ${c.code}`}
        description={
          mode === 'approve'
            ? 'Check the request, choose the technician and plan the work. The technician is told at once.'
            : 'Change what is needed. Only status changes are kept in the history.'
        }
      />

      <form
        onSubmit={onSubmit}
        noValidate
        className="grid items-start gap-6 lg:grid-cols-[minmax(0,1fr)_340px]"
      >
        <Card padding="none" className="flex flex-col">
          <Section title="The request">
            <TextField label="Title" error={errors.title?.message} {...register('title')} />
            <TextArea
              label="Description"
              rows={4}
              error={errors.description?.message}
              {...register('description')}
            />
            <div className="grid gap-4 sm:grid-cols-2">
              <Select
                label="Category"
                placeholder="Choose…"
                error={errors.serviceGroup?.message}
                {...register('serviceGroup', { onChange: () => setValue('serviceTypeId', '') })}
              >
                {groups.map((g) => (
                  <option key={g} value={g}>
                    {g}
                  </option>
                ))}
              </Select>
              <Select
                label="Problem"
                placeholder="Choose…"
                disabled={!group}
                error={errors.serviceTypeId?.message}
                {...register('serviceTypeId')}
                value={serviceTypeId}
              >
                {serviceTypes
                  .filter((s) => s.groupName === group)
                  .map((s) => (
                    <option key={s.id} value={s.id}>
                      {s.name}
                    </option>
                  ))}
              </Select>
            </div>
            <Select
              label="Urgency"
              hint={
                mode === 'approve' ? `The customer chose ${URGENCY_LABELS[c.urgency]}.` : undefined
              }
              {...register('urgency')}
            >
              {URGENCIES.map((u) => (
                <option key={u} value={u}>
                  {URGENCY_LABELS[u]}
                </option>
              ))}
            </Select>
            {urgency === 'emergency' && !c.isEmergencyFlow && (
              <Alert tone="info">
                Emergency: the case goes to the emergency flow (F4) with a 24/7 deadline from now.
              </Alert>
            )}
            {urgency !== 'emergency' && c.isEmergencyFlow && (
              <Alert tone="info">
                Not an emergency: the case goes back to the normal flow; the deadline starts with
                the work.
              </Alert>
            )}
            <dl className="border-rule grid gap-3 border-t pt-4 text-sm sm:grid-cols-2">
              <div>
                <dt className="text-muted text-xs">Location</dt>
                <dd>
                  {c.location.roomName}
                  <span className="text-muted">
                    {' '}
                    · {c.location.buildingName} · {c.location.floorName}
                  </span>
                </dd>
              </div>
              <div>
                <dt className="text-muted text-xs">Asset</dt>
                <dd>
                  {c.asset ? (
                    <>
                      {c.asset.name}{' '}
                      <span className="text-muted font-mono text-xs">
                        {formatAssetCode(c.asset.id)}
                      </span>
                    </>
                  ) : (
                    <span className="text-muted">Not specified</span>
                  )}
                </dd>
              </div>
            </dl>
          </Section>

          <Section title="Who and when">
            <TechnicianPicker
              technicians={technicians}
              value={technicianId}
              onChange={(id) =>
                setValue('technicianId', id, { shouldValidate: !!errors.technicianId })
              }
              caseId={c.id}
              error={errors.technicianId?.message}
              allowNone={mode === 'edit' && !taken}
            />
            {newCase && (
              <Alert tone="notice">
                {c.technician?.fullName} already took this job. Saving opens a new case for the new
                technician with the unfinished tasks; this case closes with the tasks{' '}
                {c.technician?.fullName} finished.
              </Alert>
            )}
            <div className="grid gap-4 sm:grid-cols-2">
              <TextField
                label="Planned start"
                type="datetime-local"
                hint="Optional. Shown in the technician's schedule."
                {...register('plannedStartAt')}
              />
              <TextField
                label="Planned end"
                type="datetime-local"
                error={errors.plannedEndAt?.message}
                hint={
                  errors.plannedEndAt ? undefined : 'Not the deadline: that starts with the work.'
                }
                {...register('plannedEndAt')}
              />
            </div>
          </Section>

          <Section title="Tasks">
            <p className="text-muted -mt-2 text-sm">
              Each task is one piece of work the technician reports on. No task: one named after the
              case.
            </p>
            {errors.tasks?.message && (
              <p className="text-negative text-xs">{errors.tasks.message}</p>
            )}
            <ol className="flex flex-col gap-3">
              {tasks.fields.map((row, i) => {
                const done = row.id ? finished.get(Number(row.id)) : undefined
                return (
                  <li
                    key={row.key}
                    className="border-rule grid items-start gap-2 border-b pb-3 last:border-0 sm:grid-cols-[minmax(0,1fr)_minmax(0,1fr)_auto] sm:border-0 sm:pb-0"
                  >
                    <TextField
                      label={`Task ${i + 1}`}
                      error={errors.tasks?.[i]?.name?.message}
                      hint={
                        done
                          ? `${TASK_STATUS_LABELS[done.status]}${done.doneBy ? ` · ${DONE_BY_LABELS[done.doneBy]}` : ''}`
                          : undefined
                      }
                      {...register(`tasks.${i}.name`)}
                    />
                    <TextField
                      label="Details"
                      placeholder="Optional"
                      error={errors.tasks?.[i]?.description?.message}
                      {...register(`tasks.${i}.description`)}
                    />
                    {done ? (
                      <span className="text-muted text-xs sm:mt-8">Has a result</span>
                    ) : (
                      <Button
                        variant="ghost"
                        className="justify-self-start sm:mt-6"
                        onClick={() => tasks.remove(i)}
                      >
                        Remove
                      </Button>
                    )}
                  </li>
                )
              })}
            </ol>
            <div>
              <Button
                variant="ghost"
                className="-ml-3"
                onClick={() => tasks.append({ id: '', name: '', description: '' })}
              >
                Add a task
              </Button>
            </div>
          </Section>

          {mode === 'edit' && (
            <Section title="Followers">
              <p className="text-muted -mt-2 text-sm">
                Other customers of the building who follow this case. They see it and are told about
                it, but cannot act on it.
              </p>
              {followerOptions.length === 0 ? (
                <p className="text-muted text-sm">No other customer in this building.</p>
              ) : (
                <div className="grid gap-2 sm:grid-cols-2">
                  {followerOptions.map((p) => (
                    <label key={p.id} className="flex items-center gap-2 text-sm">
                      <input
                        type="checkbox"
                        value={String(p.id)}
                        className="accent-primary size-4"
                        {...register('followerIds')}
                      />
                      {p.fullName}
                    </label>
                  ))}
                </div>
              )}
            </Section>
          )}

          {mode === 'edit' && c.statusChanges.length > 0 && (
            <Section title="Status">
              <fieldset className="flex flex-col gap-2">
                <legend className="sr-only">Status change</legend>
                <label className="flex items-center gap-2 text-sm">
                  <input
                    type="radio"
                    value=""
                    className="accent-primary size-4"
                    {...register('statusChange')}
                  />
                  Keep it {STATUS_LABELS[c.status]}
                </label>
                {c.statusChanges.map((s) => (
                  <label key={s} className="flex items-center gap-2 text-sm">
                    <input
                      type="radio"
                      value={s}
                      className="accent-primary size-4"
                      {...register('statusChange')}
                    />
                    {STATUS_CHANGE_LABELS[s]}
                  </label>
                ))}
              </fieldset>
              {statusChange === 'reopen' && (
                <TextArea
                  label="What is not OK?"
                  rows={3}
                  placeholder="e.g. The AC cools again but still drips water."
                  error={errors.reason?.message}
                  {...register('reason')}
                />
              )}
              {statusChange === 'close' && (
                <p className="text-muted text-sm">
                  Close only after checking the tasks, the photos and the costs. The customer is
                  asked to rate the repair.
                </p>
              )}
            </Section>
          )}

          <div className="border-border flex flex-col gap-3 border-t px-5 py-4 sm:flex-row sm:items-center sm:justify-end sm:px-6">
            {error && Object.keys(error.fieldErrors).length === 0 && (
              <div className="sm:mr-auto">
                <Alert>{error.message}</Alert>
              </div>
            )}
            <LinkButton to={caseDetailPath(c.id)} variant="ghost">
              Cancel
            </LinkButton>
            <Button type="submit" variant="primary" loading={saving}>
              {mode === 'approve'
                ? 'Approve and assign'
                : newCase
                  ? 'Save and continue in a new case'
                  : 'Save changes'}
            </Button>
          </div>
        </Card>

        <div className="flex flex-col gap-6">
          <Card>
            <CardHeader title="Reported" aside={formatDateTime(c.submittedAt)} />
            <p className="text-sm">
              {c.requester?.fullName ?? (c.isPreventive ? 'Maintenance plan' : '—')}
            </p>
            {c.photos.length > 0 && (
              <div className="mt-3 flex flex-wrap gap-2">
                {c.photos
                  .filter((p) => p.kind === 'report')
                  .map((p) => (
                    <a key={p.id} href={p.url} target="_blank" rel="noreferrer">
                      <img
                        src={p.url}
                        alt="Reported"
                        className="border-border h-20 w-28 rounded-lg border object-cover"
                      />
                    </a>
                  ))}
              </div>
            )}
          </Card>
          <Card>
            <CardHeader title="What happens next" />
            <p className="text-muted text-sm">
              {mode === 'approve'
                ? 'The technician is told and accepts or declines the job. The deadline starts when the work starts.'
                : 'The technician is told about the changes. A new technician on a job that was already taken continues it in a new case linked to this one.'}
            </p>
          </Card>
        </div>
      </form>
    </div>
  )
}

function Section({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="border-border flex flex-col gap-4 border-t p-5 first:border-t-0 sm:p-6">
      <h2 className="text-section font-semibold">{title}</h2>
      {children}
    </section>
  )
}

/** ISO (UTC) -> the value of a datetime-local input in the viewer's time zone. */
function toLocalInput(iso: string | null): string {
  if (!iso) return ''
  const d = new Date(iso)
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`
}

/** datetime-local value (viewer's time zone) -> ISO, or null when empty. */
function fromLocalInput(value: string): string | null {
  return value ? new Date(value).toISOString() : null
}
