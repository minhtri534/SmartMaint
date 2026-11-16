import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, TextField } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { addCostSchema, type AddCostFormValues } from '../../schemas'
import { PhotoPicker } from '../PhotoPicker'

interface Props {
  caseId: number
  open: boolean
  onClose: () => void
}

/**
 * Meeting 07/11, "Add cost": a part or a service the technician paid for this case — title, description, amount and
 * the receipt (its number, a photo of it, or both). The staff sees it and can correct it.
 */
export function AddCostDialog({ caseId, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Add a cost"
      description="Something you paid for this case, with the receipt."
    >
      <CostForm caseId={caseId} onDone={onClose} />
    </Dialog>
  )
}

function CostForm({ caseId, onDone }: { caseId: number; onDone: () => void }) {
  const add = useCaseMutation(casesApi.addCost)
  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<AddCostFormValues>({
    resolver: zodResolver(addCostSchema),
    defaultValues: { name: '', description: '', amount: '', receiptCode: '', receiptPhoto: [] },
  })

  return (
    <form
      onSubmit={handleSubmit((v) =>
        add.mutate(
          {
            id: caseId,
            name: v.name,
            description: v.description || null,
            amount: Number(v.amount),
            receiptCode: v.receiptCode || null,
            receiptPhoto: v.receiptPhoto[0] ?? null,
          },
          { onSuccess: onDone },
        ),
      )}
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
      <TextField
        label="Amount (VND)"
        inputMode="numeric"
        error={errors.amount?.message}
        {...register('amount')}
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
          />
        )}
      />
      {add.error && <Alert>{add.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={add.isPending}>
          Add cost
        </Button>
      </DialogActions>
    </form>
  )
}
