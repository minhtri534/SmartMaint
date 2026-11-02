import { zodResolver } from '@hookform/resolvers/zod'
import { useForm, useWatch } from 'react-hook-form'
import { useNavigate, useParams } from 'react-router'
import { paths, planDetailPath } from '@/app/paths'
import {
  Alert,
  Button,
  Card,
  ErrorBlock,
  LinkButton,
  LoadingBlock,
  PageHeader,
  Select,
  TextField,
} from '@/shared/ui'
import { maintenanceApi } from '../api'
import {
  useChecklists,
  useMaintenanceMutation,
  usePlan,
  usePlanAssets,
  useTechnicianOptions,
} from '../hooks'
import { planSchema, type PlanFormValues } from '../schemas'
import {
  FREQUENCY_LABELS,
  FREQUENCY_UNITS,
  type AssetOption,
  type Checklist,
  type PlanDetail,
  type PlanRequest,
  type TechnicianOption,
} from '../types'

const URGENCIES = [
  ['high', 'High'],
  ['normal', 'Normal'],
  ['low', 'Low'],
] as const

/**
 * PM-02 New / edit maintenance plan (Staff). One checklist on assets of one building; every asset gets the same
 * technician and first due day here. Saving sends the plan to the building manager for approval (E10), also after an
 * edit of a running plan.
 */
export function PlanFormPage() {
  const id = useParams().id
  const plan = usePlan(Number(id), id !== undefined)
  // The selects get their saved value only if their options exist when the form mounts: load the lists first.
  const checklists = useChecklists()
  const technicians = useTechnicianOptions()
  const failed = plan.error ?? checklists.error ?? technicians.error
  if (failed)
    return (
      <ErrorBlock
        what="the plan form"
        message={failed.message}
        onRetry={() =>
          void Promise.all([plan.refetch(), checklists.refetch(), technicians.refetch()])
        }
      />
    )
  if ((id !== undefined && !plan.data) || !checklists.data || !technicians.data)
    return <LoadingBlock rows={6} />
  return (
    <PlanForm
      plan={plan.data ?? null}
      checklists={checklists.data}
      technicians={technicians.data}
    />
  )
}

function PlanForm({
  plan,
  checklists,
  technicians,
}: {
  plan: PlanDetail | null
  checklists: Checklist[]
  technicians: TechnicianOption[]
}) {
  const navigate = useNavigate()
  const assets = usePlanAssets()
  const save = useMaintenanceMutation(({ id, ...body }: { id: number | null } & PlanRequest) =>
    id === null ? maintenanceApi.createPlan(body) : maintenanceApi.updatePlan({ id, ...body }),
  )
  const first = plan?.assets[0]

  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<PlanFormValues>({
    resolver: zodResolver(planSchema),
    defaultValues: {
      frequency: String(plan?.frequency ?? 1),
      frequencyType: plan?.frequencyType ?? 'month',
      leadDays: String(plan?.leadDays ?? 3),
      checklistId: plan ? String(plan.checklistId) : '',
      estimatedHours: plan?.estimatedHours ? String(plan.estimatedHours) : '',
      priority: plan?.priority ?? 'normal',
      startDate: first?.startDate ?? '',
      endDate: first?.endDate ?? '',
      technicianId: first?.technician ? String(first.technician.id) : '',
      assetIds: plan?.assets.map((a) => String(a.assetId)) ?? [],
    },
  })

  const picked = useWatch({ control, name: 'assetIds' })
  const options = assets.data ?? []
  const pickedBuilding = options.find((a) => picked.includes(String(a.id)))?.location.buildingId
  const byBuilding = groupByBuilding(options)

  const onSubmit = handleSubmit((v) => {
    const body = {
      frequency: Number(v.frequency),
      frequencyType: v.frequencyType,
      leadDays: Number(v.leadDays),
      checklistId: Number(v.checklistId),
      estimatedHours: v.estimatedHours === '' ? null : Number(v.estimatedHours),
      priority: v.priority,
      assets: v.assetIds.map((assetId) => ({
        assetId: Number(assetId),
        technicianId: v.technicianId === '' ? null : Number(v.technicianId),
        startDate: v.startDate,
        endDate: v.endDate === '' ? null : v.endDate,
      })),
    }
    save.mutate(
      { id: plan?.id ?? null, ...body },
      { onSuccess: (saved) => void navigate(planDetailPath(saved.id)) },
    )
  })

  return (
    <form onSubmit={onSubmit} noValidate className="flex max-w-[880px] flex-col gap-5">
      <PageHeader
        above={
          <LinkButton
            variant="ghost"
            to={plan ? planDetailPath(plan.id) : paths.plans}
            className="-ml-3"
          >
            {plan ? plan.name : 'Maintenance plans'}
          </LinkButton>
        }
        title={plan ? 'Edit plan' : 'New maintenance plan'}
        description="The building manager approves it before any work order is made."
      />

      <Card className="flex flex-col gap-4">
        <h2 className="text-section font-semibold">What and how often</h2>
        <Select
          label="Checklist *"
          placeholder="Choose…"
          error={errors.checklistId?.message}
          {...register('checklistId')}
        >
          {checklists.map((c) => (
            <option key={c.id} value={c.id}>
              {c.name} ({c.tasks.length} items)
            </option>
          ))}
        </Select>
        <div className="grid gap-4 sm:grid-cols-4">
          <TextField
            label="Every *"
            type="number"
            min="1"
            max="365"
            inputMode="numeric"
            error={errors.frequency?.message}
            {...register('frequency')}
          />
          <Select label="Unit" {...register('frequencyType')}>
            {FREQUENCY_UNITS.map((u) => (
              <option key={u} value={u}>
                {FREQUENCY_LABELS[u]}
              </option>
            ))}
          </Select>
          <TextField
            label="Make it ahead (days)"
            type="number"
            min="0"
            max="30"
            inputMode="numeric"
            error={errors.leadDays?.message}
            {...register('leadDays')}
          />
          <Select label="Urgency" {...register('priority')}>
            {URGENCIES.map(([value, label]) => (
              <option key={value} value={value}>
                {label}
              </option>
            ))}
          </Select>
        </div>
        <div className="grid gap-4 sm:grid-cols-4">
          <TextField
            label="First due day *"
            type="date"
            error={errors.startDate?.message}
            {...register('startDate')}
          />
          <TextField label="Last day" type="date" hint="Empty: no end." {...register('endDate')} />
          <TextField
            label="Hours per asset"
            type="number"
            step="0.25"
            min="0.25"
            inputMode="decimal"
            error={errors.estimatedHours?.message}
            {...register('estimatedHours')}
          />
          <Select
            label="Technician"
            hint="Empty: you assign each work order."
            {...register('technicianId')}
          >
            <option value="">I assign each time</option>
            {technicians.map((t) => (
              <option key={t.id} value={t.id}>
                {t.fullName} ({t.openCases} open)
              </option>
            ))}
          </Select>
        </div>
      </Card>

      <Card className="flex flex-col gap-4">
        <div className="flex items-baseline justify-between gap-3">
          <h2 className="text-section font-semibold">Assets *</h2>
          <span className="text-muted text-xs tabular-nums">{picked.length} chosen</span>
        </div>
        {assets.isPending ? (
          <LoadingBlock rows={4} />
        ) : assets.error ? (
          <ErrorBlock
            what="the assets"
            message={assets.error.message}
            onRetry={() => void assets.refetch()}
          />
        ) : (
          byBuilding.map(([building, rows]) => {
            const other =
              pickedBuilding !== undefined && rows[0]?.location.buildingId !== pickedBuilding
            return (
              <fieldset key={building} disabled={other} className="flex flex-col gap-1.5">
                <legend className="text-muted mb-1 text-xs">
                  {building}
                  {other && ' · one building per plan'}
                </legend>
                <div className="grid gap-x-6 gap-y-1.5 sm:grid-cols-2">
                  {rows.map((a) => (
                    <label
                      key={a.id}
                      className="flex items-center gap-2 text-sm has-disabled:opacity-50"
                    >
                      <input
                        type="checkbox"
                        value={a.id}
                        className="accent-primary size-4"
                        {...register('assetIds')}
                      />
                      <span className="min-w-0 truncate">
                        {a.name}
                        <span className="text-muted">
                          {' '}
                          · {a.location.floorName}, {a.location.roomName}
                        </span>
                      </span>
                    </label>
                  ))}
                </div>
              </fieldset>
            )
          })
        )}
        {errors.assetIds?.message && (
          <p className="text-negative text-xs">{errors.assetIds.message}</p>
        )}
      </Card>

      {save.error && <Alert>{save.error.message}</Alert>}
      <div className="flex justify-end gap-2">
        <LinkButton variant="ghost" to={plan ? planDetailPath(plan.id) : paths.plans}>
          Back
        </LinkButton>
        <Button type="submit" variant="primary" loading={save.isPending}>
          Send for approval
        </Button>
      </div>
    </form>
  )
}

function groupByBuilding(assets: AssetOption[]): [string, AssetOption[]][] {
  const groups = new Map<string, AssetOption[]>()
  for (const a of assets) {
    const key = a.location.buildingName
    groups.set(key, [...(groups.get(key) ?? []), a])
  }
  return [...groups.entries()]
}
