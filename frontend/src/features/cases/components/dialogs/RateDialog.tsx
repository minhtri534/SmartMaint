import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, TextArea } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { rateSchema, type RateFormValues } from '../../schemas'

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
          <div className="flex flex-col gap-1.5">
            <span className="text-sm font-medium">Your rating</span>
            <div className="flex gap-1" role="radiogroup" aria-label="Stars">
              {[1, 2, 3, 4, 5].map((n) => (
                <button
                  key={n}
                  type="button"
                  role="radio"
                  aria-checked={field.value === n}
                  aria-label={`${n} star${n > 1 ? 's' : ''}`}
                  onClick={() => field.onChange(n)}
                  className={cn(
                    'text-2xl leading-none',
                    n <= field.value ? 'text-warning' : 'text-line',
                  )}
                >
                  ★
                </button>
              ))}
            </div>
            {errors.stars && <p className="text-danger text-xs">{errors.stars.message}</p>}
          </div>
        )}
      />
      <TextArea label="Comment" rows={3} {...register('comment')} />
      {rate.error && <Alert>{rate.error.message}</Alert>}
      <DialogActions>
        <Button variant="secondary" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" loading={rate.isPending}>
          Send rating
        </Button>
      </DialogActions>
    </form>
  )
}
