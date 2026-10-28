import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, Select, Spinner } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation, useTechnicians } from '../../hooks'
import { assignSchema, type AssignFormValues } from '../../schemas'
import type { CaseDetail } from '../../types'

interface Props {
  caseDetail: CaseDetail
  open: boolean
  onClose: () => void
}

/** DLG-04 Assign technician (F3 step ⑤). The least busy technician is listed first. */
export function AssignDialog({ caseDetail, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Assign technician"
      description="The technician is notified and must accept the job."
    >
      <AssignForm caseDetail={caseDetail} onDone={onClose} />
    </Dialog>
  )
}

function AssignForm({ caseDetail, onDone }: { caseDetail: CaseDetail; onDone: () => void }) {
  const technicians = useTechnicians(true)
  const assign = useCaseMutation(casesApi.assign)
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<AssignFormValues>({
    resolver: zodResolver(assignSchema),
    defaultValues: { technicianId: '' },
  })

  if (technicians.isPending) return <Spinner />
  if (technicians.error) return <Alert>{technicians.error.message}</Alert>

  const declinedIds = new Set(
    caseDetail.assignments.filter((a) => a.response === 'declined').map((a) => a.technician.id),
  )
  // The server already lists the least busy technician first.
  const options = technicians.data

  const onSubmit = handleSubmit((values) =>
    assign.mutate(
      { id: caseDetail.id, technicianId: Number(values.technicianId) },
      { onSuccess: onDone },
    ),
  )

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
      <Select
        label="Technician"
        placeholder="Choose…"
        error={errors.technicianId?.message}
        {...register('technicianId')}
      >
        {options.map((t) => (
          <option key={t.id} value={t.id}>
            {t.fullName} · {t.openCases} open
            {declinedIds.has(t.id) ? ' · declined this case' : ''}
          </option>
        ))}
      </Select>
      <p className="text-muted text-xs">
        Nobody suitable today? Leave it Approved and assign later. The deadline starts when the
        technician starts work.
      </p>
      {assign.error && <Alert>{assign.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={assign.isPending}>
          Assign
        </Button>
      </DialogActions>
    </form>
  )
}
