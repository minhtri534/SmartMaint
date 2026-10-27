import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, TextArea } from '@/shared/ui'
import { useCaseMutation } from '../../hooks'
import { reasonSchema, type ReasonFormValues } from '../../schemas'

interface Props {
  caseId: number
  open: boolean
  onClose: () => void
  title: string
  description: string
  label: string
  placeholder?: string
  confirmLabel: string
  danger?: boolean
  /** The casesApi function to call, e.g. casesApi.cancel. */
  action: (variables: { id: number; reason: string }) => Promise<unknown>
}

/** Any status change that only needs a written reason: cancel (DLG-12), decline (DLG-09), reopen (DLG-07). */
export function ReasonDialog({ open, onClose, title, description, ...formProps }: Props) {
  return (
    <Dialog open={open} onClose={onClose} title={title} description={description}>
      <ReasonForm {...formProps} onDone={onClose} />
    </Dialog>
  )
}

function ReasonForm({
  caseId,
  label,
  placeholder,
  confirmLabel,
  danger,
  action,
  onDone,
}: Omit<Props, 'open' | 'onClose' | 'title' | 'description'> & { onDone: () => void }) {
  const mutation = useCaseMutation(action)
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ReasonFormValues>({
    resolver: zodResolver(reasonSchema),
    defaultValues: { reason: '' },
  })

  const onSubmit = handleSubmit((values) =>
    mutation.mutate({ id: caseId, reason: values.reason }, { onSuccess: onDone }),
  )

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
      <TextArea
        label={label}
        placeholder={placeholder}
        error={errors.reason?.message}
        {...register('reason')}
      />
      {mutation.error && <Alert>{mutation.error.message}</Alert>}
      <DialogActions>
        <Button variant="secondary" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant={danger ? 'danger' : 'primary'} loading={mutation.isPending}>
          {confirmLabel}
        </Button>
      </DialogActions>
    </form>
  )
}
