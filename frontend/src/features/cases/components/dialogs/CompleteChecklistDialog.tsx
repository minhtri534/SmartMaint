import { zodResolver } from '@hookform/resolvers/zod'
import { useFieldArray, useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, TextArea, TextField } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import {
  completeChecklistSchema,
  toCostLines,
  type CompleteChecklistFormValues,
} from '../../schemas'
import { CostLinesField } from '../CostLinesField'

interface Props {
  caseId: number
  open: boolean
  onClose: () => void
}

/** F2 Report a preventive work order done: every checklist item has a result. The staff still verifies (BR-13). */
export function CompleteChecklistDialog({ caseId, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Report maintenance done"
      wide
      description="The staff checks the checklist results before closing the work order."
    >
      <CompleteChecklistForm caseId={caseId} onDone={onClose} />
    </Dialog>
  )
}

function CompleteChecklistForm({ caseId, onDone }: { caseId: number; onDone: () => void }) {
  const complete = useCaseMutation(casesApi.completeChecklist)
  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<CompleteChecklistFormValues>({
    resolver: zodResolver(completeChecklistSchema),
    defaultValues: { note: '', laborHours: '', parts: [] },
  })
  const parts = useFieldArray({ control, name: 'parts' })

  const onSubmit = handleSubmit((v) =>
    complete.mutate(
      {
        id: caseId,
        note: v.note || null,
        laborHours: v.laborHours === '' ? null : Number(v.laborHours),
        parts: toCostLines(v.parts),
      },
      { onSuccess: onDone },
    ),
  )

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
      <div className="grid gap-4 sm:grid-cols-[minmax(0,1fr)_200px]">
        <TextArea
          label="Note"
          placeholder="e.g. All units serviced, filter of AC-502 replaced."
          rows={2}
          error={errors.note?.message}
          {...register('note')}
        />
        <TextField
          label="Hours worked"
          hint="Leave empty: counted from when you started."
          type="number"
          step="0.25"
          min="0.25"
          inputMode="decimal"
          error={errors.laborHours?.message}
          {...register('laborHours')}
        />
      </div>
      <CostLinesField
        rows={parts.fields}
        field={(i, key) => register(`parts.${i}.${key}`)}
        errors={errors.parts}
        onAdd={() => parts.append({ name: '', amount: '' })}
        onRemove={parts.remove}
        hint="What you paid for parts. Your labour is added automatically from your hours and hourly rate."
      />
      {complete.error && <Alert>{complete.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={complete.isPending}>
          Report done
        </Button>
      </DialogActions>
    </form>
  )
}
