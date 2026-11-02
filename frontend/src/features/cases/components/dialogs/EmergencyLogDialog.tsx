import { zodResolver } from '@hookform/resolvers/zod'
import { useForm, useWatch } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, Select, TextArea } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { emergencyLogSchema, type EmergencyLogFormValues } from '../../schemas'
import { LOG_KIND_LABELS, type EmergencyLogKind } from '../../types'

interface Props {
  caseId: number
  open: boolean
  onClose: () => void
}

const WHO_READS: Record<EmergencyLogKind, string> = {
  third_party: 'The technician is told: they are the contact person on site.',
  workaround: 'The customer is told.',
  customer_update: 'The customer is told.',
  other: 'Only in the case history.',
}

/** F4 N3 / N2 / G2: a line in the case history — third party called, temporary measure, news for the customer. */
export function EmergencyLogDialog({ caseId, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Add to the log"
      description="Written to the case history with your name and the time."
    >
      <LogForm caseId={caseId} onDone={onClose} />
    </Dialog>
  )
}

function LogForm({ caseId, onDone }: { caseId: number; onDone: () => void }) {
  const add = useCaseMutation(casesApi.addLog)
  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<EmergencyLogFormValues>({
    resolver: zodResolver(emergencyLogSchema),
    defaultValues: { kind: 'third_party', note: '' },
  })
  const kind = useWatch({ control, name: 'kind' })

  return (
    <form
      onSubmit={handleSubmit((v) => add.mutate({ id: caseId, ...v }, { onSuccess: onDone }))}
      noValidate
      className="flex flex-col gap-4"
    >
      <Select label="What" hint={WHO_READS[kind]} {...register('kind')}>
        {(Object.keys(LOG_KIND_LABELS) as EmergencyLogKind[]).map((k) => (
          <option key={k} value={k}>
            {LOG_KIND_LABELS[k]}
          </option>
        ))}
      </Select>
      <TextArea
        label="Details *"
        placeholder={
          kind === 'third_party'
            ? 'e.g. Elevator contractor, 0901 234 567, on site at 14:30'
            : 'e.g. Meeting room 12 is free for the tenant until noon'
        }
        rows={3}
        error={errors.note?.message}
        {...register('note')}
      />
      {add.error && <Alert>{add.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={add.isPending}>
          Add
        </Button>
      </DialogActions>
    </form>
  )
}
