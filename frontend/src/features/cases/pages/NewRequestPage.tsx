import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useRef, useState } from 'react'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { useNavigate, useSearchParams } from 'react-router'
import { caseDetailPath, paths } from '@/app/paths'
import {
  Alert,
  Button,
  Card,
  CardHeader,
  LinkButton,
  PageHeader,
  Select,
  TextArea,
  TextField,
} from '@/shared/ui'
import { applyFieldErrors } from '@/shared/utils/apply-field-errors'
import { formatAssetCode, parseAssetCode } from '@/shared/utils/asset-code'
import { useAssistantAction, type AssistantCard } from '@/features/assistant'
import { casesApi } from '../api'
import { AssetSearch } from '../components/AssetSearch'
import { PhotoPicker } from '../components/PhotoPicker'
import {
  useAssetLookup,
  useAssets,
  useBuildings,
  useCase,
  useCaseMutation,
  useServiceTypes,
  useServiceTypeSuggestion,
} from '../hooks'
import { newRequestSchema, type NewRequestFormValues } from '../schemas'
import { URGENCIES, URGENCY_LABELS, type AssetLookup, type Building, type Urgency } from '../types'

const EMPTY: NewRequestFormValues = {
  serviceTypeId: '',
  urgency: 'normal',
  buildingId: '',
  floorId: '',
  roomId: '',
  assetId: '',
  title: '',
  description: '',
  photos: [],
}

/** What each urgency means for the customer (F3 section 3.2). */
const URGENCY_HINTS: Record<Urgency, string> = {
  emergency: 'Danger to people or the building',
  high: 'A whole room or service is down',
  normal: 'Something is broken but work can go on',
  low: 'Cosmetic or can wait',
}

/**
 * EMP-02 New request (F3 step ①). The customer names the asset by its code or finds it by place + a keyword, in any
 * building (meeting note 18/11). The asset decides which fault categories are offered; the urgency starts at Normal.
 * The staff correct both on approval. With ?from=<id> it is the "Submit again" form after a rejection (N6): the old
 * content is filled in and a NEW case is created. With ?roomId=&assetId= (QR label, AST-07) the place and the asset are
 * filled in.
 */
export function NewRequestPage() {
  const navigate = useNavigate()
  const [params] = useSearchParams()
  const fromId = Number(params.get('from')) || 0
  const qrRoomId = Number(params.get('roomId')) || 0
  const qrAssetId = Number(params.get('assetId')) || 0
  const source = useCase(fromId, fromId > 0)
  const serviceTypes = useServiceTypes()
  const buildings = useBuildings()
  const create = useCaseMutation(casesApi.create)
  const suggestion = useServiceTypeSuggestion()
  const lookup = useAssetLookup()
  const [code, setCode] = useState('')
  const [codeError, setCodeError] = useState<string | null>(null)
  const [found, setFound] = useState<AssetLookup | null>(null)

  const {
    register,
    control,
    handleSubmit,
    setValue,
    setError,
    reset,
    getValues,
    formState: { errors },
  } = useForm<NewRequestFormValues>({
    resolver: zodResolver(newRequestSchema),
    defaultValues: EMPTY,
  })

  // "Submit again": copy the rejected case into the form once everything it needs has loaded.
  const prefilled = useRef(false)
  useEffect(() => {
    if (prefilled.current || !source.data || !buildings.data || !serviceTypes.data) return
    const c = source.data
    const place = findRoom(buildings.data, c.location.roomId)
    const service = serviceTypes.data.find((s) => s.id === c.serviceTypeId)
    reset({
      ...EMPTY,
      serviceTypeId: service ? String(service.id) : '',
      urgency: c.urgency,
      buildingId: place ? String(place.buildingId) : '',
      floorId: place ? String(place.floorId) : '',
      roomId: String(c.location.roomId),
      assetId: c.asset ? String(c.asset.id) : '',
      title: c.title,
      description: c.description,
    })
    prefilled.current = true
  }, [source.data, buildings.data, serviceTypes.data, reset])

  // QR label: the place (and asset) of the scanned label.
  useEffect(() => {
    if (prefilled.current || fromId > 0 || qrRoomId === 0 || !buildings.data) return
    const place = findRoom(buildings.data, qrRoomId)
    if (!place) return
    reset({
      ...EMPTY,
      buildingId: String(place.buildingId),
      floorId: String(place.floorId),
      roomId: String(qrRoomId),
      assetId: qrAssetId ? String(qrAssetId) : '',
    })
    prefilled.current = true
  }, [fromId, qrRoomId, qrAssetId, buildings.data, reset])

  // Selects whose options depend on another field also get value={...}: their options appear one render
  // later, so the DOM must re-apply the form value then (AI "Use this", "Submit again" pre-fill, asset code).
  const [serviceTypeId, urgency, buildingId, floorId, roomId, assetId] = useWatch({
    control,
    name: ['serviceTypeId', 'urgency', 'buildingId', 'floorId', 'roomId', 'assetId'],
  })
  const assets = useAssets(roomId ? Number(roomId) : null)

  // The faults of the chosen asset's type, plus the ones that apply to every type.
  const assetTypeId =
    assets.data?.find((a) => String(a.id) === assetId)?.typeId ??
    (found && String(found.id) === assetId ? found.typeId : undefined)
  const faults =
    assetTypeId === undefined
      ? []
      : (serviceTypes.data ?? []).filter(
          (s) => s.assetTypeId === null || s.assetTypeId === assetTypeId,
        )

  // Leader 16/11: the AI suggestion lives in the assistant chat. While this page is open the chat offers
  // "Suggest a category from my description"; its card fills the form only when the customer presses "Use this".
  useAssistantAction(
    'Suggest a category from my description',
    serviceTypes.data
      ? async (): Promise<AssistantCard> => {
          if (faults.length === 0) {
            return {
              title: 'Choose the asset first',
              detail: 'The fault categories depend on the asset that has the problem.',
              tips: [],
            }
          }
          const description = getValues('description').trim()
          if (!description) {
            return {
              title: 'Describe the problem first',
              detail: 'Write what is wrong in the Description box, then ask again.',
              tips: [],
            }
          }
          const result = await suggestion.mutateAsync({ description, serviceTypes: faults })
          const type = faults.find((t) => t.id === result.serviceTypeId)
          const note =
            result.source === 'rules'
              ? 'The AI is offline, so this was matched by keywords.'
              : 'Suggestion only: you decide, the staff confirm the urgency.'
          if (!type) {
            return {
              title: 'No matching problem',
              detail: 'Please choose the category yourself.',
              tips: result.tips,
              note,
            }
          }
          const urgency = result.urgency ?? 'normal'
          return {
            title: type.name,
            detail: `Urgency: ${URGENCY_LABELS[urgency]}`,
            tips: result.tips,
            note,
            action: {
              label: 'Use this',
              doneLabel: 'Filled in the form',
              run: () => {
                setValue('serviceTypeId', String(type.id), { shouldValidate: true })
                setValue('urgency', urgency, { shouldValidate: true })
              },
            },
          }
        }
      : null,
  )

  if (serviceTypes.isPending || buildings.isPending) return null
  if (serviceTypes.error || buildings.error) {
    return <Alert>{(serviceTypes.error ?? buildings.error)?.message}</Alert>
  }

  const building = buildings.data.find((b) => String(b.id) === buildingId)
  const floor = building?.floors.find((f) => String(f.id) === floorId)

  /** Another asset may not have the chosen fault. */
  const keepFaultFor = (typeId: number | undefined) => {
    const fault = serviceTypes.data.find((s) => String(s.id) === getValues('serviceTypeId'))
    if (fault?.assetTypeId && fault.assetTypeId !== typeId) setValue('serviceTypeId', '')
  }

  // Asset code: the place comes with the asset, so the customer does not look for the room.
  const findByCode = () => {
    setFound(null)
    const id = parseAssetCode(code)
    if (id === null) {
      setCodeError('Type the code as printed on the label, e.g. AS-000012.')
      return
    }
    setCodeError(null)
    lookup.mutate(id, {
      onSuccess: (asset) => {
        const place = findRoom(buildings.data, asset.location.roomId)
        if (asset.isRetired) setCodeError('This asset is no longer in service.')
        else if (!place) setCodeError('No asset has this code.')
        else {
          setValue('buildingId', String(place.buildingId))
          setValue('floorId', String(place.floorId))
          setValue('roomId', String(asset.location.roomId), { shouldValidate: !!errors.roomId })
          setValue('assetId', String(asset.id), { shouldValidate: !!errors.assetId })
          setFound(asset)
          keepFaultFor(asset.typeId)
        }
      },
      onError: (error) =>
        setCodeError(error.status === 404 ? 'No asset has this code.' : error.message),
    })
  }

  const clearPlace = (from: 'building' | 'floor' | 'room') => {
    if (from === 'building') setValue('floorId', '')
    if (from !== 'room') setValue('roomId', '')
    setValue('assetId', '')
    setValue('serviceTypeId', '')
    setFound(null)
  }

  const onSubmit = handleSubmit((values) => {
    create.mutate(
      {
        title: values.title,
        description: values.description,
        serviceTypeId: Number(values.serviceTypeId),
        urgency: values.urgency as Urgency,
        roomId: Number(values.roomId),
        assetId: Number(values.assetId),
        photos: values.photos,
      },
      {
        onSuccess: (created) => navigate(caseDetailPath(created.id), { replace: true }),
        onError: (error) => applyFieldErrors(error, setError),
      },
    )
  })

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title={fromId ? 'Submit again' : 'New request'}
        description={
          fromId
            ? `Filled in from ${source.data?.code ?? 'your rejected request'}. Add what was missing and send it again.`
            : "Your building's facility staff reviews it during office hours."
        }
      />

      <form
        onSubmit={onSubmit}
        noValidate
        className="grid items-start gap-6 lg:grid-cols-[minmax(0,1fr)_340px]"
      >
        <Card padding="none" className="flex flex-col">
          <section className="flex flex-col gap-4 p-5 sm:p-6">
            <h2 className="text-section font-semibold">Which asset, and where?</h2>
            <div className="flex flex-col gap-2">
              <div className="flex items-end gap-2">
                <TextField
                  label="Asset code"
                  placeholder="AS-000012"
                  className="min-w-0 flex-1 sm:max-w-72"
                  autoComplete="off"
                  value={code}
                  error={codeError ?? undefined}
                  onChange={(e) => {
                    setCode(e.target.value)
                    setCodeError(null)
                  }}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') {
                      e.preventDefault()
                      findByCode()
                    }
                  }}
                />
                {/* Keeps the button on the input's line when an error appears under the input. */}
                <Button
                  className={codeError ? 'mb-6' : undefined}
                  loading={lookup.isPending}
                  disabled={!code.trim()}
                  onClick={findByCode}
                >
                  Find
                </Button>
              </div>
              {found ? (
                <p className="text-positive text-xs">
                  {found.name} · {found.location.roomName}, {found.location.floorName}. The place
                  below is filled in.
                </p>
              ) : (
                !codeError && (
                  <p className="text-muted text-xs">
                    Printed under the QR label on the asset. No code? Find it by place below.
                  </p>
                )
              )}
            </div>

            <div className="border-rule flex flex-col gap-4 border-t pt-4">
              <div className="grid gap-4 sm:grid-cols-3">
                <Select
                  label="Building"
                  placeholder="Choose…"
                  error={errors.buildingId?.message}
                  {...register('buildingId', { onChange: () => clearPlace('building') })}
                  value={buildingId}
                >
                  {buildings.data.map((b) => (
                    <option key={b.id} value={b.id}>
                      {b.name}
                    </option>
                  ))}
                </Select>
                <Select
                  label="Floor"
                  placeholder="Choose…"
                  disabled={!building}
                  error={errors.floorId?.message}
                  {...register('floorId', { onChange: () => clearPlace('floor') })}
                  value={floorId}
                >
                  {building?.floors.map((f) => (
                    <option key={f.id} value={f.id}>
                      {f.name}
                    </option>
                  ))}
                </Select>
                <Select
                  label="Room"
                  placeholder="Choose…"
                  disabled={!floor}
                  error={errors.roomId?.message}
                  {...register('roomId', { onChange: () => clearPlace('room') })}
                  value={roomId}
                >
                  {floor?.rooms.map((r) => (
                    <option key={r.id} value={r.id}>
                      {r.name}
                    </option>
                  ))}
                </Select>
              </div>
              <AssetSearch
                assets={roomId ? assets.data : undefined}
                value={assetId}
                disabled={!roomId}
                error={errors.assetId?.message}
                onChange={(id) => {
                  setValue('assetId', id, { shouldValidate: !!errors.assetId })
                  if (!id) setFound(null)
                  else setCode(formatAssetCode(Number(id)))
                  keepFaultFor(assets.data?.find((a) => String(a.id) === id)?.typeId)
                }}
              />
            </div>
          </section>

          <section className="border-border flex flex-col gap-4 border-t p-5 sm:p-6">
            <h2 className="text-section font-semibold">What is wrong?</h2>
            <TextField
              label="Short title"
              placeholder="e.g. Air conditioner blows warm air"
              error={errors.title?.message}
              {...register('title')}
            />
            <TextArea
              label="Description"
              rows={5}
              placeholder="What happens, exactly where, and since when?"
              error={errors.description?.message}
              {...register('description')}
            />
            <div className="grid gap-4 sm:grid-cols-2">
              <Select
                label="Fault category"
                placeholder={assetId ? 'Choose…' : 'Choose the asset first'}
                disabled={!assetId}
                error={errors.serviceTypeId?.message}
                {...register('serviceTypeId')}
                value={serviceTypeId}
              >
                {faults.map((s) => (
                  <option key={s.id} value={s.id}>
                    {s.name}
                  </option>
                ))}
              </Select>
              <Select
                label="Urgency"
                hint={`${URGENCY_HINTS[urgency as Urgency]}. The staff checks it when approving.`}
                error={errors.urgency?.message}
                {...register('urgency')}
                value={urgency}
              >
                {URGENCIES.map((u) => (
                  <option key={u} value={u}>
                    {URGENCY_LABELS[u]}
                  </option>
                ))}
              </Select>
            </div>
            {urgency === 'emergency' && (
              <Alert tone="info">
                The staff confirms an emergency before work starts. If anyone is in danger, call the
                building hotline now.
              </Alert>
            )}
            <Controller
              control={control}
              name="photos"
              render={({ field }) => (
                <PhotoPicker
                  label="Photos"
                  value={field.value}
                  onChange={field.onChange}
                  error={errors.photos?.message}
                  hint="Optional, up to 5."
                />
              )}
            />
          </section>

          <div className="border-border flex flex-col gap-3 border-t px-5 py-4 sm:flex-row sm:items-center sm:justify-end sm:px-6">
            {create.error && Object.keys(create.error.fieldErrors).length === 0 && (
              <div className="sm:mr-auto">
                <Alert>{create.error.message}</Alert>
              </div>
            )}
            <LinkButton to={paths.myRequests} variant="ghost">
              Cancel
            </LinkButton>
            <Button type="submit" variant="primary" loading={create.isPending}>
              Send request
            </Button>
          </div>
        </Card>

        <div className="flex flex-col gap-6">
          <Card>
            <CardHeader title="What happens next" />
            <ol className="flex flex-col">
              {NEXT_STEPS.map((step, i) => (
                <li key={step.title} className="relative pb-4 pl-5 last:pb-0">
                  {i < NEXT_STEPS.length - 1 && (
                    <span
                      className="bg-rule absolute top-3 bottom-0 left-[3px] w-0.5"
                      aria-hidden="true"
                    />
                  )}
                  <span
                    className="bg-control absolute top-1.5 left-0 size-2 rounded-full"
                    aria-hidden="true"
                  />
                  <div className="text-sm font-medium">{step.title}</div>
                  <p className="text-muted text-sm">{step.text}</p>
                </li>
              ))}
            </ol>
          </Card>
        </div>
      </form>
    </div>
  )
}

/** F3 steps ② to ⑫ as the customer sees them. */
const NEXT_STEPS = [
  {
    title: 'The facility staff reviews it',
    text: 'Checks the fault and urgency, then approves it or sends it back with the reason.',
  },
  {
    title: 'A technician is assigned',
    text: 'The deadline depends on the urgency the staff confirmed.',
  },
  {
    title: 'The work is checked, then closed',
    text: 'You can rate it, or report "not fixed" within 7 days.',
  },
] as const

function findRoom(buildings: Building[], roomId: number) {
  for (const b of buildings) {
    for (const f of b.floors) {
      if (f.rooms.some((r) => r.id === roomId)) return { buildingId: b.id, floorId: f.id }
    }
  }
  return null
}
