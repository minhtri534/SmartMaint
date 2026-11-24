import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, TextField } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { costSchema, type AddCostFormValues } from '../../schemas'
import type { Cost } from '../../types'
import { amountOf, groupDigits } from '../../amount'
import { AmountField } from '../CostLinesField'
import { PhotoPicker } from '../PhotoPicker'

interface Props {
  caseId: number
  /** A line the technician added earlier; empty = a new cost. */
  cost?: Cost
  open: boolean
  onClose: () => void
}

/**
 * Meeting 07/11, "Add cost": a part or a service the technician paid for this case: title, description, amount and
 * the receipt (its number, a photo of it, or both). Meeting note 18/11: the technician can correct their own lines.
 */
export function AddCostDialog({ caseId, cost, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={cost ? 'Edit cost' : 'Add a cost'}
      description="Something you paid for this case, with the receipt."
    >
      {open && <CostForm caseId={caseId} cost={cost} onDone={onClose} />}
    </Dialog>
  )
}

function CostForm({ caseId, cost, onDone }: { caseId: number; cost?: Cost; onDone: () => void }) {
  const add = useCaseMutation(casesApi.addCost)
  const update = useCaseMutation(casesApi.editCost)
  const saving = add.isPending || update.isPending
  const error = add.error ?? update.error
  const {
    register,
    control,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<AddCostFormValues>({
    resolver: zodResolver(costSchema),
    defaultValues: {
      name: cost?.name ?? '',
      description: cost?.description ?? '',
      amount: cost ? groupDigits(String(cost.amount)) : '',
      receiptCode: cost?.receiptCode ?? '',
      receiptPhoto: [],
    },
  })

  return (
    <form
      onSubmit={handleSubmit((v) => {
        if (!v.receiptCode && v.receiptPhoto.length === 0 && !cost?.receiptUrl) {
          setError('receiptCode', { message: 'Add the receipt number or a photo of the receipt.' })
          return
        }
        const body = {
          id: caseId,
          name: v.name,
          description: v.description || null,
          amount: amountOf(v.amount),
          receiptCode: v.receiptCode || null,
          receiptPhoto: v.receiptPhoto[0] ?? null,
        }
        if (cost) update.mutate({ ...body, costId: cost.id }, { onSuccess: onDone })
        else add.mutate(body, { onSuccess: onDone })
      })}
      noValidate
      className="flex flex-col gap-4"
    >
      <TextField
        label="Title"
        placeholder="e.g. Capacitor 35uF"
        error={errors.name?.message}
        {...register('name')}
      />
      <TextField
        label="Description"
        placeholder="Optional, e.g. bought at the shop on Le Loi"
        error={errors.description?.message}
        {...register('description')}
      />
      <AmountField
        label="Amount (VND)"
        error={errors.amount?.message}
        registration={register('amount')}
      />
      <TextField
        label="Receipt number"
        placeholder="e.g. R-102"
        hint="The number, a photo of the receipt, or both."
        error={errors.receiptCode?.message}
        {...register('receiptCode')}
      />
      <Controller
        control={control}
        name="receiptPhoto"
        render={({ field }) => (
          <PhotoPicker
            label="Receipt photo"
            value={field.value}
            onChange={field.onChange}
            max={1}
            hint={
              cost?.receiptUrl ? 'The current photo stays unless you add a new one.' : undefined
            }
          />
        )}
      />
      {error && <Alert>{error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={saving}>
          {cost ? 'Save cost' : 'Add cost'}
        </Button>
      </DialogActions>
    </form>
  )
}
