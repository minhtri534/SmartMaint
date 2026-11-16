import { zodResolver } from '@hookform/resolvers/zod'
import { useForm, useWatch } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, Select, TextArea } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation, useCases } from '../../hooks'
import { rejectSchema, type RejectFormValues } from '../../schemas'
import {
  FINAL_STATUSES,
  REJECT_REASON_LABELS,
  REJECT_REASONS,
  STATUS_LABELS,
  type CaseDetail,
  type CaseSummary,
} from '../../types'

interface Props {
  caseDetail: CaseDetail
  open: boolean
  onClose: () => void
}

/**
 * DLG-02 Reject case. No "Need more info" any more (N6): the customer presses "Submit again".
 * Meeting 07/11 (replaces merge): a request that repeats an open case can hand its content over as a task, copy its
 * photos there, and let its customer follow that case.
 */
export function RejectDialog({ caseDetail, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Reject case"
      description="The customer sees your note and can submit a new request with the same content filled in."
    >
      <RejectForm caseDetail={caseDetail} onDone={onClose} />
    </Dialog>
  )
}

/** Open cases of the same building the request may repeat; the same asset first. */
function candidatesFor(c: CaseDetail, all: CaseSummary[]) {
  return all
    .filter(
      (x) =>
        x.id !== c.id &&
        !x.isPreventive &&
        x.status !== 'pending' &&
        !FINAL_STATUSES.includes(x.status) &&
        x.location.buildingId === c.location.buildingId,
    )
    .sort(
      (a, b) =>
        Number(b.location.roomId === c.location.roomId) -
        Number(a.location.roomId === c.location.roomId),
    )
}

function RejectForm({ caseDetail, onDone }: { caseDetail: CaseDetail; onDone: () => void }) {
  const reject = useCaseMutation(casesApi.reject)
  const cases = useCases()
  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<RejectFormValues>({
    resolver: zodResolver(rejectSchema),
    defaultValues: {
      reason: 'missing_info',
      note: '',
      duplicateOf: '',
      addAsTask: true,
      copyPhotos: true,
      addFollower: true,
    },
  })
  const reason = useWatch({ control, name: 'reason' })
  const candidates = candidatesFor(caseDetail, cases.data ?? [])

  const onSubmit = handleSubmit((v) =>
    reject.mutate(
      {
        id: caseDetail.id,
        reason: v.reason,
        note: v.note,
        duplicate:
          v.reason === 'duplicate'
            ? {
                caseId: Number(v.duplicateOf),
                addAsTask: v.addAsTask,
                copyPhotos: v.copyPhotos,
                addFollower: v.addFollower,
              }
            : null,
      },
      { onSuccess: onDone },
    ),
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
      {reason === 'duplicate' && (
        <>
          <Select
            label="The case it repeats"
            placeholder={candidates.length ? 'Choose…' : 'No open case in this building'}
            disabled={candidates.length === 0}
            error={errors.duplicateOf?.message}
            {...register('duplicateOf')}
          >
            {candidates.map((x) => (
              <option key={x.id} value={x.id}>
                {x.code} · {x.title} ({x.location.roomName}, {STATUS_LABELS[x.status]})
              </option>
            ))}
          </Select>
          <fieldset className="flex flex-col gap-2">
            <legend className="mb-1 text-sm font-medium">Hand over to that case</legend>
            <Check
              label="Add this request as a task (small extra problem)"
              {...register('addAsTask')}
            />
            <Check label="Copy its photos" {...register('copyPhotos')} />
            <Check label="Let this customer follow that case" {...register('addFollower')} />
          </fieldset>
        </>
      )}
      <TextArea
        label="Note to the customer"
        placeholder={
          reason === 'duplicate'
            ? 'e.g. The same AC is already being repaired; you can follow that case.'
            : 'e.g. Please add a photo of the leaking pipe and say which washbasin.'
        }
        error={errors.note?.message}
        {...register('note')}
      />
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

function Check({ label, ...rest }: { label: string } & React.ComponentProps<'input'>) {
  return (
    <label className="flex items-center gap-2 text-sm">
      <input type="checkbox" className="accent-primary size-4" {...rest} />
      {label}
    </label>
  )
}
