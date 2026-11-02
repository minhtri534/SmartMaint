import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, TextArea } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { failTaskSchema, type FailTaskFormValues } from '../../schemas'
import type { CaseTask } from '../../types'
import { PhotoPicker } from '../PhotoPicker'

interface Props {
  caseId: number
  task: CaseTask
  open: boolean
  onClose: () => void
}

/**
 * F2 TEC-05 A checklist item failed (BR-07): what is wrong, a photo, and either "fixed on the spot" or a repair case
 * for the staff to assign (an emergency one, F4, when it is dangerous).
 */
export function FailTaskDialog({ caseId, task, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={`Failed: ${task.name}`}
      description="A photo is required. The staff is told when a repair case is made."
      wide
    >
      <FailTaskForm caseId={caseId} task={task} onDone={onClose} />
    </Dialog>
  )
}

function FailTaskForm({
  caseId,
  task,
  onDone,
}: {
  caseId: number
  task: CaseTask
  onDone: () => void
}) {
  const update = useCaseMutation(casesApi.updateTask)
  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<FailTaskFormValues>({
    resolver: zodResolver(failTaskSchema),
    defaultValues: { note: '', photos: [], failAction: 'fixed_on_site', dangerous: false },
  })
  const hasPhoto = task.photoUrls.length > 0
  const makesCase = useWatch({ control, name: 'failAction' }) === 'create_case'

  const onSubmit = handleSubmit((v) =>
    update.mutate(
      {
        id: caseId,
        taskId: task.id,
        status: 'fail',
        note: v.note,
        photos: v.photos,
        failAction: v.failAction,
        dangerous: v.failAction === 'create_case' && v.dangerous,
      },
      { onSuccess: onDone },
    ),
  )

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
      <TextArea
        label="What is wrong? *"
        placeholder="e.g. The drain pipe is cracked and drips onto the ceiling."
        rows={3}
        error={errors.note?.message}
        {...register('note')}
      />
      <Controller
        control={control}
        name="photos"
        render={({ field }) => (
          <PhotoPicker
            label={hasPhoto ? 'More photos' : 'Photos *'}
            value={field.value}
            onChange={field.onChange}
            hint={hasPhoto ? 'This item already has a photo.' : undefined}
          />
        )}
      />
      <fieldset className="flex flex-col gap-2 text-sm">
        <legend className="mb-1 font-medium">What happens next</legend>
        <label className="flex items-center gap-2">
          <input
            type="radio"
            value="fixed_on_site"
            className="accent-primary size-4"
            {...register('failAction')}
          />
          I fixed it on the spot (the note says what I did)
        </label>
        <label className="flex items-center gap-2">
          <input
            type="radio"
            value="create_case"
            className="accent-primary size-4"
            {...register('failAction')}
          />
          Make a repair case for the staff to assign
        </label>
        {makesCase && (
          <label className="ml-6 flex items-center gap-2">
            <input type="checkbox" className="accent-primary size-4" {...register('dangerous')} />
            It is dangerous: make it an emergency (F4)
          </label>
        )}
      </fieldset>
      {update.error && <Alert>{update.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={update.isPending}>
          Save as failed
        </Button>
      </DialogActions>
    </form>
  )
}
