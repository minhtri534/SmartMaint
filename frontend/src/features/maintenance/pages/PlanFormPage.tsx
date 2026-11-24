import { zodResolver } from '@hookform/resolvers/zod'
import {
  useFieldArray,
  useForm,
  useWatch,
  type Control,
  type FieldErrors,
  type UseFormRegister,
} from 'react-hook-form'
import { Link, useNavigate, useParams } from 'react-router'
import { paths, planDetailPath } from '@/app/paths'
import {
  Alert,
  Button,
  ErrorBlock,
  FormActions,
  FormSection,
  Icon,
  LinkButton,
  LoadingBlock,
  PageHeader,
  Select,
  TextArea,
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
import { planSchema, type PlanFormValues, type TriggerFormValues } from '../schemas'
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

const NEW_TRIGGER: TriggerFormValues = {
  triggerId: '',
  frequency: '1',
  frequencyType: 'month',
  leadDays: '3',
  startDate: '',
  endDate: '',
  technicianId: '',
  assetIds: [],
}

/**
 * PM-02 New / edit maintenance plan (Staff), like UpKeep (meeting note 18/11): one work order template, then one or
 * more triggers, each with its schedule, its end day and its assets (one building per plan). Saving sends the plan to
 * the building manager for approval (E10); on a running plan, moving only end days keeps it running (extending it).
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
  const approved = plan?.status === 'active' || plan?.status === 'paused'

  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<PlanFormValues>({
    resolver: zodResolver(planSchema),
    defaultValues: {
      name: plan?.name ?? '',
      description: plan?.description ?? '',
      checklistId: plan ? String(plan.checklistId) : '',
      estimatedHours: plan?.estimatedHours ? String(plan.estimatedHours) : '',
      priority: plan?.priority ?? 'normal',
      triggers: plan?.triggers.map((t) => ({
        triggerId: String(t.id),
        frequency: String(t.frequency),
        frequencyType: t.frequencyType,
        leadDays: String(t.leadDays),
        startDate: t.startDate,
        endDate: t.endDate ?? '',
        technicianId: t.assets[0]?.technician ? String(t.assets[0].technician.id) : '',
        assetIds: t.assets.map((a) => String(a.assetId)),
      })) ?? [NEW_TRIGGER],
    },
  })
  const triggers = useFieldArray({ control, name: 'triggers' })

  const onSubmit = handleSubmit((v) => {
    const body: PlanRequest = {
      name: v.name.trim(),
      description: v.description.trim() === '' ? null : v.description.trim(),
      checklistId: Number(v.checklistId),
      estimatedHours: v.estimatedHours === '' ? null : Number(v.estimatedHours),
      priority: v.priority,
      triggers: v.triggers.map((t) => ({
        id: t.triggerId === '' ? null : Number(t.triggerId),
        frequency: Number(t.frequency),
        frequencyType: t.frequencyType,
        leadDays: Number(t.leadDays),
        startDate: t.startDate,
        endDate: t.endDate === '' ? null : t.endDate,
        assets: t.assetIds.map((assetId) => ({
          assetId: Number(assetId),
          technicianId: t.technicianId === '' ? null : Number(t.technicianId),
        })),
      })),
    }
    save.mutate(
      { id: plan?.id ?? null, ...body },
      { onSuccess: (saved) => void navigate(planDetailPath(saved.id)) },
    )
  })

  return (
    <form onSubmit={onSubmit} noValidate className="flex max-w-[1040px] flex-col gap-6">
      <PageHeader
        // the same back link as every other page ("‹ Cases"), not a bold ghost button
        above={
          <Link
            to={plan ? planDetailPath(plan.id) : paths.plans}
            className="hover:text-ink inline-flex items-center gap-1"
          >
            <Icon name="back" className="size-3.5" />
            {plan ? plan.name : 'Maintenance plans'}
          </Link>
        }
        title={plan ? (plan.expired ? 'Extend or edit plan' : 'Edit plan') : 'New maintenance plan'}
        description={
          approved
            ? 'Moving only end days keeps the plan running. Any other change sends it back to the building manager.'
            : 'The building manager approves it before any work order is made.'
        }
      />

      <FormSection
        title="Work order"
        description="What every work order of this plan gets, whichever trigger makes it."
      >
        <div className="flex flex-col gap-4">
          <TextField
            label="Plan name *"
            placeholder="e.g. Split AC service · Tower A"
            hint="Work orders are titled “Preventive: name”."
            error={errors.name?.message}
            {...register('name')}
          />
          <TextArea
            label="Note for the technician"
            placeholder="e.g. Ask the tenant before entering; bring spare filters."
            rows={2}
            error={errors.description?.message}
            {...register('description')}
          />
          <div className="grid gap-4 sm:grid-cols-[minmax(0,2fr)_minmax(0,1fr)_minmax(0,1fr)]">
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
            <Select label="Urgency" {...register('priority')}>
              {URGENCIES.map(([value, label]) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </Select>
            <TextField
              label="Hours per asset"
              type="number"
              step="0.25"
              min="0.25"
              inputMode="decimal"
              error={errors.estimatedHours?.message}
              {...register('estimatedHours')}
            />
          </div>
        </div>
      </FormSection>

      {triggers.fields.map((field, index) => (
        <TriggerSection
          key={field.id}
          index={index}
          saved={field.triggerId !== ''}
          control={control}
          register={register}
          errors={errors.triggers?.[index]}
          assets={assets.data}
          assetsState={assets}
          technicians={technicians}
          onRemove={triggers.fields.length > 1 ? () => triggers.remove(index) : undefined}
        />
      ))}

      <div className="lg:pl-[280px]">
        <Button
          variant="secondary"
          onClick={() => triggers.append(NEW_TRIGGER)}
          disabled={triggers.fields.length >= 10}
        >
          <Icon name="plus" className="size-4" />
          Add trigger
        </Button>
        {errors.triggers?.message && (
          <p className="text-negative mt-2 text-xs">{errors.triggers.message}</p>
        )}
      </div>

      <FormActions error={save.error && <Alert>{save.error.message}</Alert>}>
        <LinkButton variant="ghost" to={plan ? planDetailPath(plan.id) : paths.plans}>
          Back
        </LinkButton>
        <Button type="submit" variant="primary" loading={save.isPending}>
          {approved ? 'Save' : 'Send for approval'}
        </Button>
      </FormActions>
    </form>
  )
}

function TriggerSection({
  index,
  saved,
  control,
  register,
  errors,
  assets,
  assetsState,
  technicians,
  onRemove,
}: {
  index: number
  saved: boolean | undefined
  control: Control<PlanFormValues>
  register: UseFormRegister<PlanFormValues>
  errors: FieldErrors<TriggerFormValues> | undefined
  assets: AssetOption[] | undefined
  assetsState: { isPending: boolean; error: Error | null; refetch: () => unknown }
  technicians: TechnicianOption[]
  onRemove: (() => void) | undefined
}) {
  const all = useWatch({ control, name: 'triggers' })
  const picked = all[index]?.assetIds ?? []
  // An asset belongs to one trigger of the plan; the plan covers one building.
  const elsewhere = new Set(all.flatMap((t, i) => (i === index ? [] : t.assetIds)))
  const options = assets ?? []
  const allPicked = all.flatMap((t) => t.assetIds)
  const pickedBuilding = options.find((a) => allPicked.includes(String(a.id)))?.location.buildingId
  const name = `triggers.${index}` as const

  return (
    <FormSection
      title={`Trigger ${index + 1}`}
      aside={`${picked.length} ${picked.length === 1 ? 'asset' : 'assets'}`}
      description={
        <>
          When work orders are made: one per asset at each due day, until the end day.
          {onRemove && (
            <button
              type="button"
              onClick={onRemove}
              className="text-negative mt-2 flex items-center gap-1 text-xs font-medium hover:underline"
            >
              <Icon name="close" className="size-3.5" />
              Remove this trigger
              {saved && ' (only if it made no work order)'}
            </button>
          )}
        </>
      }
    >
      <div className="flex flex-col gap-4">
        <div className="grid gap-4 sm:grid-cols-3">
          <TextField
            label="Every *"
            type="number"
            min="1"
            max="365"
            inputMode="numeric"
            error={errors?.frequency?.message}
            {...register(`${name}.frequency`)}
          />
          <Select label="Unit" {...register(`${name}.frequencyType`)}>
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
            error={errors?.leadDays?.message}
            {...register(`${name}.leadDays`)}
          />
        </div>
        <div className="grid gap-4 sm:grid-cols-3">
          <TextField
            label="First due day *"
            type="date"
            error={errors?.startDate?.message}
            {...register(`${name}.startDate`)}
          />
          <TextField
            label="End day"
            type="date"
            hint="Empty: no end. Move it later to extend."
            error={errors?.endDate?.message}
            {...register(`${name}.endDate`)}
          />
          <Select
            label="Technician"
            hint="Empty: you assign each work order."
            {...register(`${name}.technicianId`)}
          >
            <option value="">I assign each time</option>
            {technicians.map((t) => (
              <option key={t.id} value={t.id}>
                {t.fullName} ({t.openCases} open)
              </option>
            ))}
          </Select>
        </div>

        <div className="border-rule flex flex-col gap-4 border-t pt-4">
          <div className="text-muted text-xs">Assets *</div>
          {assetsState.isPending ? (
            <LoadingBlock rows={3} />
          ) : assetsState.error ? (
            <ErrorBlock
              what="the assets"
              message={assetsState.error.message}
              onRetry={() => void assetsState.refetch()}
            />
          ) : (
            groupByBuilding(options).map(([building, rows]) => {
              const other =
                pickedBuilding !== undefined && rows[0]?.location.buildingId !== pickedBuilding
              return (
                <fieldset
                  key={building}
                  disabled={other}
                  // a fieldset is min-content wide by default: let the long asset names truncate instead
                  className="flex min-w-0 flex-col gap-1.5"
                >
                  <legend className="text-muted mb-1 text-xs">
                    {building}
                    {other && ' · one building per plan'}
                  </legend>
                  <div className="grid grid-cols-[minmax(0,1fr)] gap-x-6 gap-y-1.5 sm:grid-cols-2">
                    {rows.map((a) => {
                      const taken = elsewhere.has(String(a.id))
                      return (
                        <label
                          key={a.id}
                          className="flex items-center gap-2 text-sm has-disabled:opacity-50"
                        >
                          <input
                            type="checkbox"
                            value={a.id}
                            disabled={taken}
                            className="accent-primary size-4"
                            {...register(`${name}.assetIds`)}
                          />
                          <span className="min-w-0 truncate">
                            {a.name}
                            <span className="text-muted">
                              {' '}
                              · {a.location.floorName}, {a.location.roomName}
                              {taken && ' · in another trigger'}
                            </span>
                          </span>
                        </label>
                      )
                    })}
                  </div>
                </fieldset>
              )
            })
          )}
          {errors?.assetIds?.message && (
            <p className="text-negative text-xs">{errors.assetIds.message}</p>
          )}
        </div>
      </div>
    </FormSection>
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
