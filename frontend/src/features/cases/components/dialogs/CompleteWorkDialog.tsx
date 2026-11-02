import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useFieldArray, useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, TextField } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { completeWorkSchema, toCostLines, type CompleteWorkFormValues } from '../../schemas'
import { CostLinesField } from '../CostLinesField'
import { PhotoPicker } from '../PhotoPicker'

interface Props {
  caseId: number
  /** F5: an outside party replaced the asset → the new model is required. */
  needsReplacement?: boolean
  open: boolean
  onClose: () => void
}

/**
 * DLG-11 Report completed, with the repair record (F3 step ⑨, N21).
 * Completed is not Closed: the staff still has to verify (BR-13).
 */
export function CompleteWorkDialog({ caseId, needsReplacement = false, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Report work completed"
      wide
      description="The staff checks this record and the photos before closing the case."
    >
      <CompleteWorkForm caseId={caseId} needsReplacement={needsReplacement} onDone={onClose} />
    </Dialog>
  )
}

function CompleteWorkForm({
  caseId,
  needsReplacement,
  onDone,
}: {
  caseId: number
  needsReplacement: boolean
  onDone: () => void
}) {
  const complete = useCaseMutation(casesApi.complete)
  const {
    register,
    control,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<CompleteWorkFormValues>({
    resolver: zodResolver(completeWorkSchema),
    defaultValues: {
      parts: [],
      rootCause: '',
      partsUsed: '',
      laborHours: '',
      beforePhotos: [],
      afterPhotos: [],
      replacementModel: '',
      replacementSerial: '',
    },
  })
  const parts = useFieldArray({ control, name: 'parts' })

  const onSubmit = handleSubmit((values) => {
    if (needsReplacement && values.replacementModel.length < 2) {
      setError('replacementModel', { message: 'Write the model of the new asset.' })
      return
    }
    complete.mutate(
      {
        id: caseId,
        ...values,
        partsUsed: values.partsUsed || null,
        laborHours: values.laborHours === '' ? null : Number(values.laborHours),
        // The labour line (hours × your hourly rate) is added by the server.
        parts: toCostLines(values.parts),
        replacementModel: needsReplacement ? values.replacementModel : null,
        replacementSerial: needsReplacement ? values.replacementSerial || null : null,
      },
      { onSuccess: onDone },
    )
  })

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
      {needsReplacement && (
        <div className="grid gap-4 sm:grid-cols-2">
          <TextField
            label="New asset model *"
            placeholder="e.g. Daikin FTKF35"
            hint="Installed by the outside party; the Administrator registers it."
            error={errors.replacementModel?.message}
            {...register('replacementModel')}
          />
          <TextField label="Serial number" hint="Optional" {...register('replacementSerial')} />
        </div>
      )}
      <div className="grid gap-4 sm:grid-cols-[minmax(0,1fr)_200px]">
        <TextField
          label="Cause *"
          placeholder="e.g. Blocked condensate pipe"
          error={errors.rootCause?.message}
          {...register('rootCause')}
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
      <TextField
        label="Parts used"
        placeholder="Written by hand, e.g. 1× MCB 20A"
        hint="No stock module: parts are bought outside the system (N2)."
        {...register('partsUsed')}
      />
      <CostLinesField
        rows={parts.fields}
        field={(i, key) => register(`parts.${i}.${key}`)}
        errors={errors.parts}
        onAdd={() => parts.append({ name: '', amount: '' })}
        onRemove={parts.remove}
        hint="What you paid for parts. Your labour is added automatically from your hours and hourly rate."
      />
      <div className="grid gap-4 sm:grid-cols-2">
        <Controller
          control={control}
          name="beforePhotos"
          render={({ field }) => (
            <PhotoPicker
              label="Photos before repair"
              value={field.value}
              onChange={field.onChange}
            />
          )}
        />
        <Controller
          control={control}
          name="afterPhotos"
          render={({ field }) => (
            <PhotoPicker
              label="Photos after repair *"
              value={field.value}
              onChange={field.onChange}
              error={errors.afterPhotos?.message}
            />
          )}
        />
      </div>
      {complete.error && <Alert>{complete.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={complete.isPending}>
          Report completed
        </Button>
      </DialogActions>
    </form>
  )
}
