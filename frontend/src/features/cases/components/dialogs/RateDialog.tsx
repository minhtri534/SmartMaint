import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, TextArea } from '@/shared/ui'

import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { rateSchema, type RateFormValues } from '../../schemas'
import { StarsInput } from '../Stars'

interface Props {
  caseId: number
  open: boolean
  onClose: () => void
}

/** DLG-14 Rate the service (F3 step ⑫): optional, once per case. */
export function RateDialog({ caseId, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Rate the service"
      description="Optional. You can rate each case once."
    >
      <RateForm caseId={caseId} onDone={onClose} />
    </Dialog>
  )
}

function RateForm({ caseId, onDone }: { caseId: number; onDone: () => void }) {
  const rate = useCaseMutation(casesApi.rate)
  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<RateFormValues>({
    resolver: zodResolver(rateSchema),
    defaultValues: { stars: 0, comment: '' },
  })

  const onSubmit = handleSubmit((values) =>
    rate.mutate({ id: caseId, ...values }, { onSuccess: onDone }),
  )

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
      <Controller
        control={control}
        name="stars"
        render={({ field }) => (
          <div className="flex flex-col gap-2">
            <span className="text-sm font-medium">Your rating</span>
            <StarsInput value={field.value} onChange={field.onChange} />
            {errors.stars && <p className="text-negative text-xs">{errors.stars.message}</p>}
          </div>
        )}
      />
      <TextArea label="Comment" rows={3} {...register('comment')} />
      {rate.error && <Alert>{rate.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={rate.isPending}>
          Send rating
        </Button>
      </DialogActions>
    </form>
  )
}
