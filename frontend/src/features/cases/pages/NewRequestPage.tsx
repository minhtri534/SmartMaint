import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useRef } from 'react'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { useNavigate, useSearchParams } from 'react-router'
import { caseDetailPath, paths } from '@/app/paths'
import {
  Alert,
  Button,
  Card,
  LinkButton,
  PageHeader,
  Select,
  TextArea,
  TextField,
} from '@/shared/ui'
import { applyFieldErrors } from '@/shared/utils/apply-field-errors'
import { casesApi } from '../api'
import { MockDataBanner } from '../components/MockDataBanner'
import { PhotoPicker } from '../components/PhotoPicker'
import {
  useAssets,
  useBuildings,
  useCase,
  useCaseMutation,
  useServiceTypes,
  useServiceTypeSuggestion,
} from '../hooks'
import { newRequestSchema, type NewRequestFormValues } from '../schemas'
import { URGENCY_LABELS, type Building } from '../types'

const EMPTY: NewRequestFormValues = {
  serviceGroup: '',
  serviceTypeId: '',
  buildingId: '',
  floorId: '',
  roomId: '',
  assetId: '',
  title: '',
  description: '',
  photos: [],
}

/**
 * EMP-02 New request (F3 step ①). With ?from=<id> it is the "Submit again" form after a rejection (N6):
 * the old content is filled in and a NEW case is created.
 * TODO: limit buildings / rooms to the customer's leased area (ERD review #11).
 */
export function NewRequestPage() {
  const navigate = useNavigate()
  const fromId = Number(useSearchParams()[0].get('from')) || 0
  const source = useCase(fromId, fromId > 0)
  const serviceTypes = useServiceTypes()
  const buildings = useBuildings()
  const create = useCaseMutation(casesApi.create)
  const suggestion = useServiceTypeSuggestion()

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
      buildingId: place ? String(place.buildingId) : '',
      floorId: place ? String(place.floorId) : '',
      roomId: String(c.location.roomId),
      assetId: c.asset ? String(c.asset.id) : '',
      title: c.title,
      description: c.description,
    })
    prefilled.current = true
  }, [source.data, buildings.data, serviceTypes.data, reset])

  // Selects whose options depend on another field also get value={...}: their options appear one render
  // later, so the DOM must re-apply the form value then (AI "Use this", "Submit again" pre-fill).
  const [group, serviceTypeId, buildingId, floorId, roomId, assetId] = useWatch({
    control,
    name: ['serviceGroup', 'serviceTypeId', 'buildingId', 'floorId', 'roomId', 'assetId'],
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

  const applySuggestion = () => {
    if (!suggested) return
    setValue('serviceGroup', suggested.groupName)
    setValue('serviceTypeId', String(suggested.id), { shouldValidate: true })
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
    <div className="flex flex-col gap-4">
      <MockDataBanner />
      <PageHeader
        title={fromId ? 'Submit again' : 'New request'}
        description={
          fromId
            ? `Filled in from ${source.data?.code ?? 'your rejected request'}. Fix what was missing and send it again.`
            : 'Tell the building management what is wrong. Requests are handled during office hours.'
        }
        actions={
          <LinkButton to={paths.myRequests} variant="secondary">
            Back to my requests
          </LinkButton>
        }
      />

      <form onSubmit={onSubmit} noValidate className="grid gap-4 lg:grid-cols-3">
        <Card className="flex flex-col gap-4 lg:col-span-2">
          <h2 className="text-sm font-semibold">What is wrong?</h2>
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

          <div className="border-primary/20 bg-primary-soft flex flex-col gap-2 rounded-md border p-3 text-sm">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <span className="text-primary font-medium">AI assistant (suggestion only)</span>
              <Button
                variant="secondary"
                className="h-8"
                loading={suggestion.isPending}
                onClick={() => suggestion.mutate(getValues('description'))}
              >
                Suggest type from description
              </Button>
            </div>
            {suggestion.data && (
              <>
                {suggested ? (
                  <div className="flex flex-wrap items-center gap-2">
                    <span>
                      Looks like{' '}
                      <strong>
                        {suggested.groupName} · {suggested.name}
                      </strong>{' '}
                      ({URGENCY_LABELS[suggested.defaultUrgency]}).
                    </span>
                    {String(suggested.id) !== serviceTypeId && (
                      <Button variant="ghost" className="h-7 px-2" onClick={applySuggestion}>
                        Use this
                      </Button>
                    )}
                  </div>
                ) : (
                  <span>No suggestion — please pick the type yourself.</span>
                )}
                {suggestion.data.tips.map((tip) => (
                  <span key={tip} className="text-muted">
                    • {tip}
                  </span>
                ))}
              </>
            )}
            {suggestion.error && <span className="text-muted">AI is not available right now.</span>}
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <Select
              label="Group"
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
              label="Type"
              placeholder={group ? 'Choose…' : 'Choose a group first'}
              disabled={!group}
              error={errors.serviceTypeId?.message}
              {...register('serviceTypeId')}
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
          {service?.isEmergency && (
            <Alert>
              <strong>Emergency.</strong> This type goes straight to the emergency team (F4), not
              the normal queue. If anyone is in danger, also call the building hotline now.
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
                hint={service?.photoRequired ? 'This type needs at least one photo.' : 'Optional.'}
              />
            )}
          />
        </Card>

        <div className="flex flex-col gap-4">
          <Card className="flex flex-col gap-4">
            <h2 className="text-sm font-semibold">Where?</h2>
            <Select
              label="Building"
              placeholder="Choose…"
              error={errors.buildingId?.message}
              {...register('buildingId', {
                onChange: () => {
                  setValue('floorId', '')
                  setValue('roomId', '')
                  setValue('assetId', '')
                },
              })}
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
              {...register('floorId', {
                onChange: () => {
                  setValue('roomId', '')
                  setValue('assetId', '')
                },
              })}
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
              {...register('roomId', { onChange: () => setValue('assetId', '') })}
              value={roomId}
            >
              {floor?.rooms.map((r) => (
                <option key={r.id} value={r.id}>
                  {r.name}
                </option>
              ))}
            </Select>
            <Select
              label="Asset (if you know it)"
              placeholder="Not sure"
              disabled={!roomId || !assets.data?.length}
              hint="Scanning the QR label on the asset fills this in (AST-07, later)."
              {...register('assetId')}
              value={assetId}
            >
              {assets.data?.map((a) => (
                <option key={a.id} value={a.id}>
                  {a.name}
                </option>
              ))}
            </Select>
          </Card>

          {create.error && Object.keys(create.error.fieldErrors).length === 0 && (
            <Alert>{create.error.message}</Alert>
          )}
          <Button type="submit" loading={create.isPending}>
            Send request
          </Button>
        </div>
      </form>
    </div>
  )
}

function findRoom(buildings: Building[], roomId: number) {
  for (const b of buildings) {
    for (const f of b.floors) {
      if (f.rooms.some((r) => r.id === roomId)) return { buildingId: b.id, floorId: f.id }
    }
  }
  return null
}
