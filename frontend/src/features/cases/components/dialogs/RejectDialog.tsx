import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, Select, TextArea } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { rejectSchema, type RejectFormValues } from '../../schemas'
import { REJECT_REASON_LABELS, REJECT_REASONS, type CaseDetail } from '../../types'

interface Props {
  caseDetail: CaseDetail
  open: boolean
  onClose: () => void
}

/** DLG-02 Reject case. No "Need more info" any more (N6): the customer presses "Submit again". */
export function RejectDialog({ caseDetail, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Reject case"
      description="The customer sees your note and can submit a new request with the same content filled in."
    >
      <RejectForm caseId={caseDetail.id} onDone={onClose} />
    </Dialog>
  )
}

function RejectForm({ caseId, onDone }: { caseId: number; onDone: () => void }) {
  const reject = useCaseMutation(casesApi.reject)
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RejectFormValues>({
    resolver: zodResolver(rejectSchema),
    defaultValues: { reason: 'missing_info', note: '' },
  })

  const onSubmit = handleSubmit((values) =>
    reject.mutate({ id: caseId, ...values }, { onSuccess: onDone }),
  )

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
      <Select label="Reason" {...register('reason')}>
        {REJECT_REASONS.map((r) => (
          <option key={r} value={r}>
            {REJECT_REASON_LABELS[r]}
          </option>
        ))}
      </Select>
      <TextArea
        label="Note to the customer"
        placeholder="e.g. Please add a photo of the leaking pipe and say which washbasin."
        error={errors.note?.message}
        {...register('note')}
      />
      <p className="text-muted text-xs">
        Same problem already reported? Choose "A case for this problem already exists" and say which
        case in the note.
      </p>
      {reject.error && <Alert>{reject.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="danger" loading={reject.isPending}>
          Reject
        </Button>
      </DialogActions>
    </form>
  )
}
