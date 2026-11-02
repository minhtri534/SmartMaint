import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, TextArea } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { requestDecisionSchema, type RequestDecisionFormValues } from '../../schemas'
import { RECOMMENDATION_LABELS, type TechnicianRecommendation } from '../../types'
import { PhotoPicker } from '../PhotoPicker'

interface Props {
  caseId: number
  open: boolean
  onClose: () => void
}

/**
 * F5 steps ①–②: the job is beyond the technician (or the asset should be replaced). The assessment goes to the
 * staff, who decide: continue, the manufacturer, a contractor, or replace. The case waits On Hold meanwhile.
 */
export function RequestDecisionDialog({ caseId, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Needs the staff's decision"
      wide
      description="The staff read your assessment and decide how to go on. The case waits on hold."
    >
      <AssessmentForm caseId={caseId} onDone={onClose} />
    </Dialog>
  )
}

function AssessmentForm({ caseId, onDone }: { caseId: number; onDone: () => void }) {
  const send = useCaseMutation(casesApi.requestDecision)
  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<RequestDecisionFormValues>({
    resolver: zodResolver(requestDecisionSchema),
    defaultValues: { condition: '', cause: '', recommendation: 'repairable', photos: [] },
  })

  return (
    <form
      onSubmit={handleSubmit((v) => send.mutate({ id: caseId, ...v }, { onSuccess: onDone }))}
      noValidate
      className="flex flex-col gap-4"
    >
      <TextArea
        label="Condition *"
        placeholder="e.g. Compressor bearing worn, the unit trips on overload after 10 minutes"
        rows={3}
        error={errors.condition?.message}
        {...register('condition')}
      />
      <TextArea
        label="Cause *"
        placeholder="e.g. Wear: the unit is old and runs all day"
        rows={2}
        error={errors.cause?.message}
        {...register('cause')}
      />
      <fieldset className="flex flex-col gap-2 text-sm">
        <legend className="mb-1 font-medium">Your recommendation</legend>
        {(Object.keys(RECOMMENDATION_LABELS) as TechnicianRecommendation[]).map((r) => (
          <label key={r} className="flex items-center gap-2">
            <input
              type="radio"
              value={r}
              className="accent-primary size-4"
              {...register('recommendation')}
            />
            {RECOMMENDATION_LABELS[r]}
          </label>
        ))}
      </fieldset>
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
