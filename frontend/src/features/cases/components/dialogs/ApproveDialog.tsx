import { zodResolver } from '@hookform/resolvers/zod'
import { useForm, useWatch } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, Select } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { approveSchema, type ApproveFormValues } from '../../schemas'
import {
  COST_BEARER_LABELS,
  COST_BEARERS,
  URGENCIES,
  URGENCY_LABELS,
  type CaseDetail,
} from '../../types'

interface Props {
  caseDetail: CaseDetail
  open: boolean
  onClose: () => void
}

/** DLG-01 Approve case (F3 step ④): the officer confirms the urgency and who pays. */
export function ApproveDialog({ caseDetail, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Approve case"
      description="Check the urgency and who pays. Values come from the service type; change them if needed."
    >
      <ApproveForm caseDetail={caseDetail} onDone={onClose} />
    </Dialog>
  )
}

function ApproveForm({ caseDetail, onDone }: { caseDetail: CaseDetail; onDone: () => void }) {
  const approve = useCaseMutation(casesApi.approve)
  const { register, control, handleSubmit } = useForm<ApproveFormValues>({
    resolver: zodResolver(approveSchema),
    defaultValues: { urgency: caseDetail.urgency, costBearer: caseDetail.costBearer },
  })

  const urgency = useWatch({ control, name: 'urgency' })

  const onSubmit = handleSubmit((values) =>
    approve.mutate({ id: caseDetail.id, ...values }, { onSuccess: onDone }),
  )

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
      <Select label="Urgency" {...register('urgency')}>
        {URGENCIES.map((u) => (
          <option key={u} value={u}>
            {URGENCY_LABELS[u]}
          </option>
        ))}
      </Select>
      {urgency === 'emergency' && (
        <Alert tone="info">
          Emergency cases leave this flow and are handled by the emergency flow (F4).
        </Alert>
      )}
      <Select
        label="Who pays"
        hint="Building = common systems up to the tenant's area. Customer = the tenant's own fit-out or damage they caused."
        {...register('costBearer')}
      >
        {COST_BEARERS.map((b) => (
          <option key={b} value={b}>
            {COST_BEARER_LABELS[b]}
          </option>
        ))}
      </Select>
      <p className="text-muted text-xs">The deadline (SLA) starts counting when you approve.</p>
      {approve.error && <Alert>{approve.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={approve.isPending}>
          Approve
        </Button>
      </DialogActions>
    </form>
  )
}
