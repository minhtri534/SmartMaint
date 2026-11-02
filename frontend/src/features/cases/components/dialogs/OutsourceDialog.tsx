import { zodResolver } from '@hookform/resolvers/zod'
import { useForm, useWatch } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, Select, TextField } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { outsourceSchema, type OutsourceFormValues } from '../../schemas'
import { OUTSIDE_KIND_LABELS, type CaseDetail, type OutsideWorkKind } from '../../types'

interface Props {
  caseDetail: CaseDetail
  open: boolean
  onClose: () => void
}

const KIND_HINTS: Record<OutsideWorkKind, string> = {
  warranty:
    'The manufacturer repairs or replaces it under warranty. If they refuse, you choose again.',
  contract: 'Covered by a package contract (elevator, fire system...): they come without a quote.',
  quote: 'They survey and quote first; the payer decides before any work.',
}

/**
 * F5 steps ④–⑤: hand the job to the manufacturer or a contractor. The technician stays the contact person on site
 * and checks the result; the case waits On Hold "waiting for an outside party". No money is stored (N22).
 */
export function OutsourceDialog({ caseDetail, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Hand to an outside party"
      wide
      description="The technician is told and stays the contact person on site."
    >
      <OutsourceForm caseDetail={caseDetail} onDone={onClose} />
    </Dialog>
  )
}

function OutsourceForm({ caseDetail, onDone }: { caseDetail: CaseDetail; onDone: () => void }) {
  const outsource = useCaseMutation(casesApi.outsource)
  const info = caseDetail.decisionInfo
  const underWarranty = info?.underWarranty ?? false
  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<OutsourceFormValues>({
    resolver: zodResolver(outsourceSchema),
    defaultValues: {
      kind: underWarranty ? 'warranty' : 'contract',
      job: caseDetail.assessment?.recommendation === 'replace' ? 'replace' : 'repair',
      companyName: underWarranty ? (info?.manufacturerName ?? '') : '',
      contact: underWarranty ? (info?.manufacturerPhone ?? '') : '',
      appointmentAt: '',
      note: '',
    },
  })
  const kind = useWatch({ control, name: 'kind' })

  return (
    <form
      onSubmit={handleSubmit((v) =>
        outsource.mutate(
          {
            id: caseDetail.id,
            ...v,
            appointmentAt: v.appointmentAt ? new Date(v.appointmentAt).toISOString() : null,
            note: v.note || null,
          },
          { onSuccess: onDone },
        ),
      )}
      noValidate
      className="flex flex-col gap-4"
    >
      <div className="grid gap-4 sm:grid-cols-2">
        <Select label="Who" hint={KIND_HINTS[kind]} {...register('kind')}>
          {(Object.keys(OUTSIDE_KIND_LABELS) as OutsideWorkKind[]).map((k) => (
            <option key={k} value={k}>
              {OUTSIDE_KIND_LABELS[k]}
            </option>
          ))}
        </Select>
        <Select label="To do" {...register('job')}>
          <option value="repair">Repair the asset</option>
          <option value="replace">Replace the asset</option>
        </Select>
      </div>
      <div className="grid gap-4 sm:grid-cols-2">
        <TextField
          label="Company *"
          placeholder="e.g. Cool Air Co."
          error={errors.companyName?.message}
          {...register('companyName')}
        />
        <TextField
          label="Contact *"
          placeholder="Name and phone"
          error={errors.contact?.message}
          {...register('contact')}
        />
      </div>
      <div className="grid gap-4 sm:grid-cols-2">
        <TextField
          label="Appointment"
          type="datetime-local"
          hint={kind === 'quote' ? 'When they come to survey.' : 'When they come.'}
          {...register('appointmentAt')}
        />
        <TextField
          label="Note"
          hint="Optional: why this party, what to tell them"
          {...register('note')}
        />
      </div>
      {outsource.error && <Alert>{outsource.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={outsource.isPending}>
          Hand over
        </Button>
      </DialogActions>
    </form>
  )
}
