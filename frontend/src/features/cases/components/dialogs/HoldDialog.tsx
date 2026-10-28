import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, Select, TextField } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { holdSchema, type HoldFormValues } from '../../schemas'
import { HOLD_REASON_LABELS, HOLD_REASONS } from '../../types'

interface Props {
  caseId: number
  open: boolean
  onClose: () => void
}

/** DLG-10 Put work on hold: a fixed reason and the day it should continue (N13). */
export function HoldDialog({ caseId, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Put work on hold"
      description="The staff are told why and when you expect to continue."
    >
      <HoldForm caseId={caseId} onDone={onClose} />
    </Dialog>
  )
}

function HoldForm({ caseId, onDone }: { caseId: number; onDone: () => void }) {
  const hold = useCaseMutation(casesApi.hold)
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<HoldFormValues>({
    resolver: zodResolver(holdSchema),
    defaultValues: { reason: 'waiting_parts', expectedResumeOn: '' },
  })

  const onSubmit = handleSubmit((values) =>
    hold.mutate({ id: caseId, ...values }, { onSuccess: onDone }),
  )

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
      <Select label="Why" error={errors.reason?.message} {...register('reason')}>
        {HOLD_REASONS.map((r) => (
          <option key={r} value={r}>
            {HOLD_REASON_LABELS[r]}
          </option>
        ))}
      </Select>
      <TextField
        label="Expected to continue on"
        type="date"
        className="max-w-52"
        error={errors.expectedResumeOn?.message}
        {...register('expectedResumeOn')}
      />
      {hold.error && <Alert>{hold.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={hold.isPending}>
          Put on hold
        </Button>
      </DialogActions>
    </form>
  )
}
