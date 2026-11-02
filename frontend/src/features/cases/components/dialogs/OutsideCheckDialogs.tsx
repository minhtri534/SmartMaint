import { zodResolver } from '@hookform/resolvers/zod'
import { useForm, useWatch } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, TextArea, TextField } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import {
  checkOutsideWorkSchema,
  redoOutsideWorkSchema,
  type CheckOutsideWorkFormValues,
  type RedoOutsideWorkFormValues,
} from '../../schemas'

interface Props {
  caseId: number
  open: boolean
  onClose: () => void
}

/** F5 step ⑧: the technician checks the outside party's work. OK → you finish the case with the repair record. */
export function CheckOutsideWorkDialog({ caseId, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Check the outside work"
      description="OK: the case is back with you to write the repair record. Not OK: the staff ask them to redo it."
    >
      <CheckForm caseId={caseId} onDone={onClose} />
    </Dialog>
  )
}

function CheckForm({ caseId, onDone }: { caseId: number; onDone: () => void }) {
  const check = useCaseMutation(casesApi.checkOutsideWork)
  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<CheckOutsideWorkFormValues>({
    resolver: zodResolver(checkOutsideWorkSchema),
    defaultValues: { passed: 'yes', note: '' },
  })
  const passed = useWatch({ control, name: 'passed' }) === 'yes'

  return (
    <form
      onSubmit={handleSubmit((v) =>
        check.mutate(
          { id: caseId, passed: v.passed === 'yes', note: v.note || null },
          { onSuccess: onDone },
        ),
      )}
      noValidate
      className="flex flex-col gap-4"
    >
      <fieldset className="flex flex-col gap-2 text-sm">
        <legend className="mb-1 font-medium">Result</legend>
        <label className="flex items-center gap-2">
          <input
            type="radio"
            value="yes"
            className="accent-primary size-4"
            {...register('passed')}
          />
          OK, it works
        </label>
        <label className="flex items-center gap-2">
          <input
            type="radio"
            value="no"
            className="accent-primary size-4"
            {...register('passed')}
          />
          Not OK
        </label>
      </fieldset>
      <TextArea
        label={passed ? 'Note' : 'What is not OK *'}
        rows={2}
        placeholder={passed ? 'Optional' : 'e.g. Still trips after ten minutes'}
        error={errors.note?.message}
        {...register('note')}
      />
      {check.error && <Alert>{check.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={check.isPending}>
          Save result
        </Button>
      </DialogActions>
    </form>
  )
}

/** The technician's check failed: the same party comes back. To change party, hand it to another one instead. */
export function RedoOutsideWorkDialog({ caseId, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Ask to redo it"
      description="The technician is told the new appointment."
    >
      <RedoForm caseId={caseId} onDone={onClose} />
    </Dialog>
  )
}

function RedoForm({ caseId, onDone }: { caseId: number; onDone: () => void }) {
  const redo = useCaseMutation(casesApi.redoOutsideWork)
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RedoOutsideWorkFormValues>({
    resolver: zodResolver(redoOutsideWorkSchema),
    defaultValues: { appointmentAt: '', reason: '' },
  })

  return (
    <form
      onSubmit={handleSubmit((v) =>
        redo.mutate(
          {
            id: caseId,
            appointmentAt: v.appointmentAt ? new Date(v.appointmentAt).toISOString() : null,
            reason: v.reason,
          },
          { onSuccess: onDone },
        ),
      )}
      noValidate
      className="flex flex-col gap-4"
    >
      <TextField label="New appointment" type="datetime-local" {...register('appointmentAt')} />
      <TextArea
        label="What to redo *"
        rows={2}
        placeholder="e.g. Check the capacitor as well"
        error={errors.reason?.message}
        {...register('reason')}
      />
      {redo.error && <Alert>{redo.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={redo.isPending}>
          Ask to redo
        </Button>
      </DialogActions>
    </form>
  )
}
