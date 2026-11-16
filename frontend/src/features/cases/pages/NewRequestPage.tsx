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
import {
  URGENCIES,
  URGENCY_LABELS,
  type AssetLookup,
  type Building,
  type ServiceType,
  type Urgency,
} from '../types'

const EMPTY: NewRequestFormValues = {
  serviceGroup: '',
  serviceTypeId: '',
  urgency: '',
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

/** An emergency problem type is always an emergency; otherwise its default urgency is only a starting point. */
const urgencyOf = (s: ServiceType): Urgency => (s.isEmergency ? 'emergency' : s.defaultUrgency)

/**
 * EMP-02 New request (F3 step ①). Meeting 07/11: the customer chooses the category and the urgency (the staff
 * corrects them on approval), and names the asset by its code or finds it by place + a keyword.
 * With ?from=<id> it is the "Submit again" form after a rejection (N6): the old content is filled in and a NEW case
 * is created. With ?roomId=&assetId= (QR label, AST-07) the place and the asset are filled in. The server only offers
 * the customer's own room and the common areas of their building.
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
      serviceGroup: service?.groupName ?? '',
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
  const [group, serviceTypeId, urgency, buildingId, floorId, roomId, assetId] = useWatch({
    control,
    name: [
      'serviceGroup',
      'serviceTypeId',
      'urgency',
      'buildingId',
      'floorId',
      'roomId',
      'assetId',
    ],
  })
  const assets = useAssets(roomId ? Number(roomId) : null)

  if (serviceTypes.isPending || buildings.isPending) return null
  if (serviceTypes.error || buildings.error) {
    return <Alert>{(serviceTypes.error ?? buildings.error)?.message}</Alert>
  }

  const groups = [...new Set(serviceTypes.data.map((s) => s.groupName))]
  const service = serviceTypes.data.find((s) => String(s.id) === serviceTypeId)
  const building = buildings.data.find((b) => String(b.id) === buildingId)
  const floor = building?.floors.find((f) => String(f.id) === floorId)
  const suggested = serviceTypes.data.find((s) => s.id === suggestion.data?.serviceTypeId)

  const chooseType = (s: ServiceType | undefined) => {
    if (s) setValue('urgency', urgencyOf(s), { shouldValidate: !!errors.urgency })
  }

  const applySuggestion = () => {
    if (!suggested) return
    setValue('serviceGroup', suggested.groupName)
    setValue('serviceTypeId', String(suggested.id), { shouldValidate: true })
    chooseType(suggested)
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
        else if (!asset.canReport || !place)
          setCodeError('This asset is not in your room or a common area of your building.')
        else {
          setValue('buildingId', String(place.buildingId))
          setValue('floorId', String(place.floorId))
          setValue('roomId', String(asset.location.roomId), { shouldValidate: !!errors.roomId })
          setValue('assetId', String(asset.id))
          setFound(asset)
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
    setFound(null)
  }

  const onSubmit = handleSubmit((values) => {
    if (service?.photoRequired && values.photos.length === 0) {
      setError('photos', { message: `"${service.name}" needs at least one photo.` })
      return
    }
    create.mutate(
      {
        title: values.title,
        description: values.description,
        serviceTypeId: Number(values.serviceTypeId),
        urgency: values.urgency as Urgency,
        roomId: Number(values.roomId),
        assetId: values.assetId ? Number(values.assetId) : null,
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
                label="Category"
                placeholder="Choose…"
                hint="Electrical, water, air conditioning…"
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
                placeholder={group ? 'Choose…' : 'Choose a category first'}
                disabled={!group}
                error={errors.serviceTypeId?.message}
                {...register('serviceTypeId', {
                  onChange: (e) =>
                    chooseType(serviceTypes.data.find((s) => String(s.id) === e.target.value)),
                })}
                value={serviceTypeId}
              >
                {serviceTypes.data
                  .filter((s) => s.groupName === group)
                  .map((s) => (
                    <option key={s.id} value={s.id}>
                      {s.name}
                    </option>
                  ))}
              </Select>
            </div>
            {service?.isEmergency ? (
              <Alert>
                Emergency problem: it goes straight to the emergency team (F4), not the normal
                queue. If anyone is in danger, also call the building hotline now.
              </Alert>
            ) : (
              <Select
                label="Urgency"
                placeholder="Choose…"
                hint={
                  urgency
                    ? `${URGENCY_HINTS[urgency as Urgency]}. The staff checks it when approving.`
                    : 'Filled in from the problem; change it if needed. The staff checks it when approving.'
                }
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
            )}
            {!service?.isEmergency && urgency === 'emergency' && (
              <Alert tone="info">
                The staff confirms an emergency before it goes to the emergency team. If anyone is
                in danger, call the building hotline now.
              </Alert>
            )}
            <Controller
              control={control}
              name="photos"
              render={({ field }) => (
                <PhotoPicker
                  label={service?.photoRequired ? 'Photos *' : 'Photos'}
                  value={field.value}
                  onChange={field.onChange}
                  error={errors.photos?.message}
                  hint={
                    service?.photoRequired
                      ? `"${service.name}" needs at least one photo.`
                      : 'Optional, up to 5.'
                  }
                />
              )}
            />
          </section>

          <section className="border-border flex flex-col gap-4 border-t p-5 sm:p-6">
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
                  label="Room or area"
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
                onChange={(id) => {
                  setValue('assetId', id)
                  if (!id) setFound(null)
                  else setCode(formatAssetCode(Number(id)))
                }}
              />
            </div>
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
            <CardHeader title="Suggested problem" aside="AI, suggestion only" />
            <p className="text-muted text-sm">
              Reads your description and proposes a category and problem. You decide; nothing is
              sent.
            </p>
            <Button
              className="mt-3"
              loading={suggestion.isPending}
              onClick={() =>
                suggestion.mutate({
                  description: getValues('description'),
                  serviceTypes: serviceTypes.data,
                })
              }
            >
              Suggest from description
            </Button>
            {suggestion.data && (
              <div className="border-rule mt-4 flex flex-col gap-2 border-t pt-4 text-sm">
                {suggested ? (
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <span>
                      <span className="font-medium">
                        {suggested.groupName} · {suggested.name}
                      </span>
                      <span className="text-muted"> · {URGENCY_LABELS[urgencyOf(suggested)]}</span>
                    </span>
                    {String(suggested.id) !== serviceTypeId ? (
                      <Button variant="ghost" className="-mr-3" onClick={applySuggestion}>
                        Use this
                      </Button>
                    ) : (
                      <span className="text-positive text-xs">Selected</span>
                    )}
                  </div>
                ) : (
                  <span className="text-muted">No match. Please choose the category yourself.</span>
                )}
                {suggestion.data.tips.map((tip) => (
                  <p key={tip} className="text-muted">
                    {tip}
                  </p>
                ))}
              </div>
            )}
            {suggestion.error && (
              <p className="text-muted mt-3 text-sm">The assistant is not available right now.</p>
            )}
          </Card>

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
    text: 'Checks the category and urgency, then approves it or sends it back with the reason.',
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
