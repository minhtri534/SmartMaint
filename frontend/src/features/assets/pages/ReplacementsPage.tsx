import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  ErrorBlock,
  LoadingBlock,
  PageHeader,
  Select,
  TextArea,
  TextField,
} from '@/shared/ui'
import { assetsApi } from '../api'
import { ReplacementTable } from '../components/ReplacementTable'
import { useAssetMutation, useAssetTypes, useManufacturers, useReplacements } from '../hooks'
import {
  dismissReplacementSchema,
  replacementSchema,
  type DismissReplacementValues,
  type ReplacementFormValues,
} from '../schemas'
import type { AssetTypeOption, ManufacturerOption, ReplacementRequest } from '../types'

/**
 * ADM A06 (F5 → F1 page 4): an outside party replaced an asset and the case is closed. The Administrator registers
 * the new asset in the same room ("replaces" the old one) and the old one is retired in the same step.
 */
export function ReplacementsPage() {
  const list = useReplacements()
  const [registering, setRegistering] = useState<ReplacementRequest | null>(null)
  const [dismissing, setDismissing] = useState<ReplacementRequest | null>(null)
  const open = list.data?.filter((r) => r.status === 'open') ?? []
  const finished = list.data?.filter((r) => r.status !== 'open') ?? []

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title="Replacements"
        description={
          list.data &&
          `${open.length} replaced ${open.length === 1 ? 'asset waits' : 'assets wait'} to be registered.`
        }
      />
      {list.isPending ? (
        <LoadingBlock rows={4} />
      ) : list.error ? (
        <ErrorBlock
          what="the replacements"
          message={list.error.message}
          onRetry={() => void list.refetch()}
        />
      ) : (
        <>
          <ReplacementTable
            rows={open}
            emptyText="No replaced asset waits to be registered."
            actions={(r) => (
              <>
                <Button variant="ghost" onClick={() => setDismissing(r)}>
                  Dismiss
                </Button>
                <Button variant="primary" onClick={() => setRegistering(r)}>
                  Register new asset
                </Button>
              </>
            )}
          />
          {finished.length > 0 && (
            <div className="flex flex-col gap-2">
              <h2 className="text-section font-semibold">Done</h2>
              <ReplacementTable rows={finished} emptyText="" />
            </div>
          )}
        </>
      )}

      {registering && <RegisterDialog request={registering} onClose={() => setRegistering(null)} />}
      {dismissing && <DismissDialog request={dismissing} onClose={() => setDismissing(null)} />}
    </div>
  )
}

function RegisterDialog({
  request,
  onClose,
}: {
  request: ReplacementRequest
  onClose: () => void
}) {
  const types = useAssetTypes()
  const makers = useManufacturers(true)
  return (
    <Dialog
      open
      wide
      onClose={onClose}
      title="Register the new asset"
      description={`In ${request.location.floorName}, ${request.location.roomName}. "${request.oldAsset.name}" is retired at the same time.`}
    >
      {types.data && makers.data ? (
        <RegisterForm request={request} types={types.data} makers={makers.data} onDone={onClose} />
      ) : types.error || makers.error ? (
        <Alert>{(types.error ?? makers.error)!.message}</Alert>
      ) : (
        <LoadingBlock rows={3} />
      )}
    </Dialog>
  )
}

function RegisterForm({
  request,
  types,
  makers,
  onDone,
}: {
  request: ReplacementRequest
  types: AssetTypeOption[]
  makers: ManufacturerOption[]
  onDone: () => void
}) {
  const complete = useAssetMutation(assetsApi.completeReplacement)
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ReplacementFormValues>({
    resolver: zodResolver(replacementSchema),
    defaultValues: {
      name: request.newSerial ? `${request.newModel} (${request.newSerial})` : request.newModel,
      typeId: String(request.oldAssetTypeId),
      manufacturerId: request.oldManufacturerId ? String(request.oldManufacturerId) : '',
      hasCopyright: false,
      warrantyEndDate: '',
    },
  })

  return (
    <form
      onSubmit={handleSubmit((v) =>
        complete.mutate(
          {
            id: request.id,
            name: v.name,
            typeId: Number(v.typeId),
            manufacturerId: v.manufacturerId ? Number(v.manufacturerId) : null,
            hasCopyright: v.hasCopyright,
            warrantyEndDate: v.warrantyEndDate || null,
          },
          { onSuccess: onDone },
        ),
      )}
      noValidate
      className="flex flex-col gap-4"
    >
      <TextField label="Name *" error={errors.name?.message} {...register('name')} />
      <div className="grid gap-4 sm:grid-cols-2">
        <Select label="Type *" error={errors.typeId?.message} {...register('typeId')}>
          {types.map((t) => (
            <option key={t.id} value={t.id}>
              {t.name}
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
      </div>
      <div className="grid gap-4 sm:grid-cols-2">
        <TextField label="Warranty until" type="date" {...register('warrantyEndDate')} />
        <label className="flex items-center gap-2 self-end pb-2 text-sm">
          <input type="checkbox" className="accent-primary size-4" {...register('hasCopyright')} />
          Only the manufacturer may service it
        </label>
      </div>
      {complete.error && <Alert>{complete.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={complete.isPending}>
          Register and retire the old one
        </Button>
      </DialogActions>
    </form>
  )
}

function DismissDialog({ request, onClose }: { request: ReplacementRequest; onClose: () => void }) {
  const dismiss = useAssetMutation(assetsApi.dismissReplacement)
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<DismissReplacementValues>({
    resolver: zodResolver(dismissReplacementSchema),
    defaultValues: { reason: '' },
  })
  return (
    <Dialog
      open
      onClose={onClose}
      title="Dismiss the replacement"
      description={`The staff member who asked (${request.requestedBy}) reads your reason.`}
    >
      <form
        onSubmit={handleSubmit((v) =>
          dismiss.mutate({ id: request.id, reason: v.reason }, { onSuccess: onClose }),
        )}
        noValidate
        className="flex flex-col gap-4"
      >
        <TextArea
          label="Reason *"
          rows={3}
          placeholder="e.g. Already registered by hand as AC 501 (new)"
          error={errors.reason?.message}
          {...register('reason')}
        />
        {dismiss.error && <Alert>{dismiss.error.message}</Alert>}
        <DialogActions>
          <Button variant="ghost" onClick={onClose}>
            Back
          </Button>
          <Button type="submit" variant="danger" loading={dismiss.isPending}>
            Dismiss
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}
