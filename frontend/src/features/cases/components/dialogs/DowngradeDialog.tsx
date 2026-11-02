import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, Select, TextArea } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { downgradeSchema, type DowngradeFormValues } from '../../schemas'
import { COST_BEARERS, COST_BEARER_LABELS, URGENCY_LABELS, type CaseDetail } from '../../types'

interface Props {
  caseDetail: CaseDetail
  open: boolean
  onClose: () => void
}

/** F4 step ④ (K04): not an emergency after all. Back to F3 before a technician accepts; counted separately in F6. */
export function DowngradeDialog({ caseDetail, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Not an emergency"
      description="The case goes back to the normal flow (F3). Its deadline then starts when the technician starts work."
    >
      <DowngradeForm caseDetail={caseDetail} onDone={onClose} />
    </Dialog>
  )
}

function DowngradeForm({ caseDetail, onDone }: { caseDetail: CaseDetail; onDone: () => void }) {
  const downgrade = useCaseMutation(casesApi.downgrade)
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<DowngradeFormValues>({
    resolver: zodResolver(downgradeSchema),
    defaultValues: { urgency: 'high', costBearer: caseDetail.costBearer, reason: '' },
  })

  return (
    <form
      onSubmit={handleSubmit((v) =>
        downgrade.mutate({ id: caseDetail.id, ...v }, { onSuccess: onDone }),
      )}
      noValidate
      className="flex flex-col gap-4"
    >
      <div className="grid gap-4 sm:grid-cols-2">
        <Select label="New urgency" {...register('urgency')}>
          {(['high', 'normal', 'low'] as const).map((u) => (
            <option key={u} value={u}>
              {URGENCY_LABELS[u]}
            </option>
          ))}
        </Select>
        <Select label="Who pays" {...register('costBearer')}>
          {COST_BEARERS.map((b) => (
            <option key={b} value={b}>
              {COST_BEARER_LABELS[b]}
            </option>
          ))}
        </Select>
      </div>
      <TextArea
        label="Why it is not an emergency *"
        placeholder="e.g. Only a dripping tap, the floor is dry"
        rows={3}
        error={errors.reason?.message}
        {...register('reason')}
      />
      {downgrade.error && <Alert>{downgrade.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={downgrade.isPending}>
          Move to normal flow
        </Button>
      </DialogActions>
    </form>
  )
}
