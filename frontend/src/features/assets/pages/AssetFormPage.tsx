import { zodResolver } from '@hookform/resolvers/zod'
import { useForm, useWatch } from 'react-hook-form'
import { useNavigate, useParams } from 'react-router'
import { assetDetailPath, paths } from '@/app/paths'
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
import { applyFieldErrors } from '@/shared/utils/apply-field-errors'
import { assetsApi } from '../api'
import { useAsset, useAssetMutation, useAssetTypes, useManufacturers, usePlaces } from '../hooks'
import { assetSchema, toAssetRequest, type AssetFormValues } from '../schemas'
import type { AssetDetail, AssetTypeOption, ManufacturerOption, PlaceOption } from '../types'

/** AST-04 Create / Edit asset (Administrator only, Leader 02/11). */
export function AssetFormPage({ mode }: { mode: 'create' | 'edit' }) {
  const { id } = useParams()
  const asset = useAsset(Number(id), mode === 'edit')
  const types = useAssetTypes()
  const places = usePlaces()
  const makers = useManufacturers(true)

  const loading =
    (mode === 'edit' && asset.isPending) || types.isPending || places.isPending || makers.isPending
  if (loading) return <LoadingBlock rows={6} />
  const error =
    (mode === 'edit' ? asset.error : null) ?? types.error ?? places.error ?? makers.error
  if (error)
    return (
      <ErrorBlock
        what="the form"
        message={error.message}
        onRetry={() =>
          void Promise.all([asset.refetch(), types.refetch(), places.refetch(), makers.refetch()])
        }
      />
    )

  return (
    <AssetForm
      asset={mode === 'edit' ? asset.data! : null}
      types={types.data!}
      places={places.data!}
      makers={makers.data!}
    />
  )
}

function AssetForm({
  asset,
  types,
  places,
  makers,
}: {
  asset: AssetDetail | null
  types: AssetTypeOption[]
  places: PlaceOption[]
  makers: ManufacturerOption[]
}) {
  const navigate = useNavigate()
  const save = useAssetMutation((v: AssetFormValues) =>
    asset
      ? assetsApi.update({ id: asset.id, ...toAssetRequest(v) })
      : assetsApi.create(toAssetRequest(v)),
  )
  const {
    register,
    handleSubmit,
    setError,
    setValue,
    control,
    formState: { errors },
  } = useForm<AssetFormValues>({
    resolver: zodResolver(assetSchema),
    defaultValues: {
      name: asset?.name ?? '',
      buildingId: asset ? String(asset.location.buildingId) : '',
      roomId: asset ? String(asset.location.roomId) : '',
      typeId: asset ? String(asset.typeId) : '',
      manufacturerId: asset?.manufacturerId ? String(asset.manufacturerId) : '',
      hasCopyright: asset?.hasCopyright ?? false,
      warrantyEndDate: asset?.warrantyEndDate ?? '',
    },
  })
  const [buildingId, roomId] = useWatch({ control, name: ['buildingId', 'roomId'] })
  const floors = places.find((b) => String(b.id) === buildingId)?.floors ?? []

  const onSubmit = handleSubmit((v) =>
    save.mutate(v, {
      onSuccess: (saved) => navigate(assetDetailPath(saved.id), { replace: true }),
      onError: (e) => applyFieldErrors(e, setError),
    }),
  )
  const showError = save.error && Object.keys(save.error.fieldErrors).length === 0

  return (
    <div className="flex max-w-[720px] flex-col gap-5">
      <PageHeader
        title={asset ? 'Edit asset' : 'New asset'}
        description={
          asset
            ? `${asset.name} · ${asset.location.buildingName}`
            : 'One asset in one room. For many, use Import.'
        }
      />
      <form onSubmit={onSubmit} noValidate className="flex flex-col gap-5">
        <Card>
          <div className="grid gap-4 sm:grid-cols-2">
            <TextField
              label="Name"
              hint="e.g. Split AC Daikin 2HP (#AC-501)"
              className="sm:col-span-2"
              error={errors.name?.message}
              {...register('name')}
            />
            <Select
              label="Type"
              placeholder="Choose a type"
              error={errors.typeId?.message}
              {...register('typeId')}
            >
              {types.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.categoryName} · {t.name}
                </option>
              ))}
            </Select>
            <Select label="Manufacturer" placeholder="Unknown" {...register('manufacturerId')}>
              {makers.map((m) => (
                <option key={m.id} value={m.id}>
                  {m.name}
                </option>
              ))}
            </Select>
            <Select
              label="Building"
              placeholder="Choose a building"
              error={errors.buildingId?.message}
              {...register('buildingId', { onChange: () => setValue('roomId', '') })}
            >
              {places.map((b) => (
                <option key={b.id} value={b.id}>
                  {b.name}
                </option>
              ))}
            </Select>
            <Select
              label="Room"
              placeholder={buildingId ? 'Choose a room' : 'Choose a building first'}
              value={roomId}
              error={errors.roomId?.message}
              {...register('roomId')}
            >
              {floors.flatMap((f) =>
                f.rooms.map((r) => (
                  <option key={r.id} value={r.id}>
                    {f.name} · {r.name}
                  </option>
                )),
              )}
            </Select>
            <TextField
              label="Warranty until"
              type="date"
              hint="Optional"
              {...register('warrantyEndDate')}
            />
            <label className="flex items-center gap-2 self-end pb-2 text-sm">
              <input
                type="checkbox"
                className="accent-primary size-4"
                {...register('hasCopyright')}
              />
              Only the manufacturer may service it
            </label>
          </div>
        </Card>
        {showError && <Alert>{save.error.message}</Alert>}
        <div className="flex justify-end gap-2">
          <LinkButton variant="ghost" to={asset ? assetDetailPath(asset.id) : paths.assets}>
            Cancel
          </LinkButton>
          <Button type="submit" variant="primary" loading={save.isPending}>
            {asset ? 'Save changes' : 'Add asset'}
          </Button>
        </div>
      </form>
    </div>
  )
}
