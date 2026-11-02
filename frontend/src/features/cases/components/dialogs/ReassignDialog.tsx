import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, Select, Spinner, TextArea } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation, useTechnicians } from '../../hooks'
import { reassignSchema, type ReassignFormValues } from '../../schemas'
import type { CaseDetail } from '../../types'

interface Props {
  caseDetail: CaseDetail
  open: boolean
  onClose: () => void
}

/**
 * Staff gives the work to another technician (not a decline). The case goes back to Assigned, the new technician
 * must accept, and the deadline keeps running.
 */
export function ReassignDialog({ caseDetail, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Reassign technician"
      description={`${caseDetail.technician?.fullName ?? 'The technician'} is told the work was taken back. The deadline does not restart.`}
    >
      <ReassignForm caseDetail={caseDetail} onDone={onClose} />
    </Dialog>
  )
}

function ReassignForm({ caseDetail, onDone }: { caseDetail: CaseDetail; onDone: () => void }) {
  const technicians = useTechnicians(true)
  const reassign = useCaseMutation(casesApi.reassign)
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ReassignFormValues>({
    resolver: zodResolver(reassignSchema),
    defaultValues: { technicianId: '', reason: '' },
  })

  if (technicians.isPending) return <Spinner />
  if (technicians.error) return <Alert>{technicians.error.message}</Alert>

  const onSubmit = handleSubmit((v) =>
    reassign.mutate(
      { id: caseDetail.id, technicianId: Number(v.technicianId), reason: v.reason },
      { onSuccess: onDone },
    ),
  )

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
      <Select
        label="New technician"
        placeholder="Choose…"
        error={errors.technicianId?.message}
        {...register('technicianId')}
      >
        {technicians.data
          .filter((t) => t.id !== caseDetail.technician?.id)
          .map((t) => (
            <option key={t.id} value={t.id}>
              {t.fullName} · {t.openCases} open
            </option>
          ))}
      </Select>
      <TextArea
        label="Why"
        placeholder="e.g. Needs an electrician, the current technician is HVAC"
        error={errors.reason?.message}
        {...register('reason')}
      />
      {reassign.error && <Alert>{reassign.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={reassign.isPending}>
          Reassign
        </Button>
      </DialogActions>
    </form>
  )
}
