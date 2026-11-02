import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, Select, TextArea } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { urgencySchema, type UrgencyFormValues } from '../../schemas'
import { URGENCIES, URGENCY_LABELS, type CaseDetail } from '../../types'

interface Props {
  caseDetail: CaseDetail
  open: boolean
  onClose: () => void
}

/** O08: the problem turned out more (or less) urgent. The deadline follows the new level; Emergency goes to F4. */
export function UrgencyDialog({ caseDetail, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Change urgency"
      description="If work has started, the deadline is counted again from the start with the new level."
    >
      <UrgencyForm caseDetail={caseDetail} onDone={onClose} />
    </Dialog>
  )
}

function UrgencyForm({ caseDetail, onDone }: { caseDetail: CaseDetail; onDone: () => void }) {
  const change = useCaseMutation(casesApi.changeUrgency)
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<UrgencyFormValues>({
    resolver: zodResolver(urgencySchema),
    defaultValues: { urgency: URGENCIES.find((u) => u !== caseDetail.urgency), reason: '' },
  })

  const onSubmit = handleSubmit((v) =>
    change.mutate({ id: caseDetail.id, ...v }, { onSuccess: onDone }),
  )

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
      <Select label="New urgency" error={errors.urgency?.message} {...register('urgency')}>
        {URGENCIES.filter((u) => u !== caseDetail.urgency).map((u) => (
          <option key={u} value={u}>
            {URGENCY_LABELS[u]}
            {u === 'emergency' ? ' (hands the case to the emergency flow)' : ''}
          </option>
        ))}
      </Select>
      <TextArea
        label="Why"
        placeholder="e.g. Water now reaches the server room"
        error={errors.reason?.message}
        {...register('reason')}
      />
      {change.error && <Alert>{change.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={change.isPending}>
          Change urgency
        </Button>
      </DialogActions>
    </form>
  )
}
