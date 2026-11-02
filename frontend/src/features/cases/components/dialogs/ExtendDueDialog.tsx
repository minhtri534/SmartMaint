import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, Select, TextField } from '@/shared/ui'
import { formatDateTime } from '@/shared/utils/format'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { extendDueSchema, type ExtendDueFormValues } from '../../schemas'
import { EXTEND_REASON_LABELS, EXTEND_REASONS, type CaseDetail } from '../../types'

interface Props {
  caseDetail: CaseDetail
  open: boolean
  onClose: () => void
}

/** O09: a later deadline, only before the current one passes; the reason comes from a fixed list. */
export function ExtendDueDialog({ caseDetail, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Move the deadline"
      description={`Now due ${formatDateTime(caseDetail.dueAt)}. The customer and the technician are told.`}
    >
      <ExtendDueForm caseDetail={caseDetail} onDone={onClose} />
    </Dialog>
  )
}

/** "yyyy-MM-ddTHH:mm" in local time, the format of <input type="datetime-local">. */
const toLocalInput = (iso: string) => {
  const d = new Date(iso)
  return new Date(d.getTime() - d.getTimezoneOffset() * 60_000).toISOString().slice(0, 16)
}

function ExtendDueForm({ caseDetail, onDone }: { caseDetail: CaseDetail; onDone: () => void }) {
  const extend = useCaseMutation(casesApi.extendDue)
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ExtendDueFormValues>({
    resolver: zodResolver(extendDueSchema),
    defaultValues: {
      newDueAt: caseDetail.dueAt ? toLocalInput(caseDetail.dueAt) : '',
      reason: 'waiting_parts',
      note: '',
    },
  })

  const onSubmit = handleSubmit((v) =>
    extend.mutate(
      {
        id: caseDetail.id,
        // The input is local time; the API takes an ISO date-time.
        newDueAt: new Date(v.newDueAt).toISOString(),
        reason: v.reason,
        note: v.note || null,
      },
      { onSuccess: onDone },
    ),
  )

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
      <TextField
        label="New deadline"
        type="datetime-local"
        className="max-w-64"
        error={errors.newDueAt?.message}
        {...register('newDueAt')}
      />
      <Select label="Why" error={errors.reason?.message} {...register('reason')}>
        {EXTEND_REASONS.map((r) => (
          <option key={r} value={r}>
            {EXTEND_REASON_LABELS[r]}
          </option>
        ))}
      </Select>
      <TextField
        label="Note"
        hint="Required for Other reason"
        error={errors.note?.message}
        {...register('note')}
      />
      {extend.error && <Alert>{extend.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={extend.isPending}>
          Move deadline
        </Button>
      </DialogActions>
    </form>
  )
}
