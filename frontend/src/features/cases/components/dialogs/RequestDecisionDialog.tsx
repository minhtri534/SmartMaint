import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, TextArea } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { requestDecisionSchema, type RequestDecisionFormValues } from '../../schemas'
import { PhotoPicker } from '../PhotoPicker'

interface Props {
  caseId: number
  open: boolean
  onClose: () => void
}

/**
 * Meeting 07/11 (replaces the technician's assessment): the technician tells the staff the job needs an outside
 * party — what is wrong, why they cannot do it, and photos. The staff agree it with the party outside the system;
 * the case waits on hold and the work clock stops.
 */
export function RequestDecisionDialog({ caseId, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Needs an outside party"
      wide
      description="The staff read this and arrange the outside party. The case waits on hold."
    >
      <RequestForm caseId={caseId} onDone={onClose} />
    </Dialog>
  )
}

function RequestForm({ caseId, onDone }: { caseId: number; onDone: () => void }) {
  const send = useCaseMutation(casesApi.requestDecision)
  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<RequestDecisionFormValues>({
    resolver: zodResolver(requestDecisionSchema),
    defaultValues: { description: '', reason: '', photos: [] },
  })

  return (
    <form
      onSubmit={handleSubmit((v) => send.mutate({ id: caseId, ...v }, { onSuccess: onDone }))}
      noValidate
      className="flex flex-col gap-4"
    >
      <TextArea
        label="What is wrong *"
        placeholder="e.g. Compressor bearing worn, the unit trips on overload after 10 minutes"
        rows={3}
        error={errors.description?.message}
        {...register('description')}
      />
      <TextArea
        label="Why it needs an outside party *"
        placeholder="e.g. The compressor is only sold and fitted by the manufacturer"
        rows={2}
        error={errors.reason?.message}
        {...register('reason')}
      />
      <Controller
        control={control}
        name="photos"
        render={({ field }) => (
          <PhotoPicker
            label="Photos *"
            value={field.value}
            onChange={field.onChange}
            error={errors.photos?.message}
          />
        )}
      />
      {send.error && <Alert>{send.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={send.isPending}>
          Send to the staff
        </Button>
      </DialogActions>
    </form>
  )
}
