import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, Select, TextArea, TextField } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { taskUpdateSchema, type TaskUpdateFormValues } from '../../schemas'
import { TASK_STATUS_LABELS, type CaseTask, type TaskStatus } from '../../types'
import { PhotoPicker } from '../PhotoPicker'

interface Props {
  caseId: number
  /** F2 work order: a failed item may make a repair case (BR-07). */
  isPreventive: boolean
  task: CaseTask
  open: boolean
  onClose: () => void
}

const STATUSES: TaskStatus[] = ['pending', 'pass', 'fail', 'not_applicable']

/**
 * TEC-05, meeting 07/11: the technician updates one task — its status, a note and photos, each with a caption if
 * needed. "Not done" is fine too, to write down progress along the way.
 */
export function TaskUpdateDialog({ caseId, isPreventive, task, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      wide
      title={task.name}
      description={task.description ?? undefined}
    >
      <TaskForm caseId={caseId} isPreventive={isPreventive} task={task} onDone={onClose} />
    </Dialog>
  )
}

function TaskForm({
  caseId,
  isPreventive,
  task,
  onDone,
}: {
  caseId: number
  isPreventive: boolean
  task: CaseTask
  onDone: () => void
}) {
  const update = useCaseMutation(casesApi.updateTask)
  const {
    register,
    control,
    handleSubmit,
    setValue,
    setError,
    getValues,
    formState: { errors },
  } = useForm<TaskUpdateFormValues>({
    resolver: zodResolver(taskUpdateSchema),
    defaultValues: {
      status: task.status === 'pending' ? 'pass' : task.status,
      note: '',
      photos: [],
      captions: [],
      failAction: 'fixed_on_site',
      dangerous: false,
    },
  })
  const [status, photos, failAction] = useWatch({
    control,
    name: ['status', 'photos', 'failAction'],
  })

  const onSubmit = handleSubmit((v) => {
    if (v.status === 'fail' && v.photos.length === 0 && task.photos.length === 0) {
      setError('photos', { message: 'A failed task needs a photo.' })
      return
    }
    update.mutate(
      {
        id: caseId,
        taskId: task.id,
        status: v.status,
        note: v.note || null,
        photos: v.photos.map((image, i) => ({ image, caption: v.captions[i] || null })),
        failAction: v.status === 'fail' && isPreventive ? v.failAction : null,
        dangerous:
          v.status === 'fail' && isPreventive && v.failAction === 'create_case' && v.dangerous,
      },
      { onSuccess: onDone },
    )
  })

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
      <Select label="Status" {...register('status')}>
        {STATUSES.map((s) => (
          <option key={s} value={s}>
            {TASK_STATUS_LABELS[s]}
          </option>
        ))}
      </Select>
      <TextArea
        label={status === 'fail' ? 'What is wrong *' : 'Note'}
        rows={3}
        placeholder={
          status === 'fail'
            ? 'e.g. The filter is torn and must be replaced.'
            : 'Optional: what you did or found.'
        }
        error={errors.note?.message}
        {...register('note')}
      />
      {status === 'fail' && isPreventive && (
        <fieldset className="flex flex-col gap-2 text-sm">
          <legend className="mb-1 font-medium">What happens next</legend>
          <label className="flex items-center gap-2">
            <input
              type="radio"
              value="fixed_on_site"
              className="accent-primary size-4"
              {...register('failAction')}
            />
            Fixed on the spot
          </label>
          <label className="flex items-center gap-2">
            <input
              type="radio"
              value="create_case"
              className="accent-primary size-4"
              {...register('failAction')}
            />
            Needs a repair case
          </label>
          {failAction === 'create_case' && (
            <label className="flex items-center gap-2 pl-6">
              <input type="checkbox" className="accent-primary size-4" {...register('dangerous')} />
              Dangerous: make it an emergency
            </label>
          )}
        </fieldset>
      )}
      <Controller
        control={control}
        name="photos"
        render={({ field }) => (
          <PhotoPicker
            label={status === 'fail' && task.photos.length === 0 ? 'Photos *' : 'Photos'}
            value={field.value}
            onChange={(next) => {
              // Keep each caption with its photo when one is removed.
              const kept = getValues('captions').filter(
                (_, i) => field.value[i] && next.includes(field.value[i]),
              )
              setValue(
                'captions',
                next.map((_, i) => kept[i] ?? ''),
              )
              field.onChange(next)
            }}
            error={errors.photos?.message}
            hint="Up to 5. Add a caption under each if it helps."
          />
        )}
      />
      {photos.map((_, i) => (
        <TextField
          key={i}
          label={`Caption of photo ${i + 1}`}
          placeholder="Optional, e.g. Before cleaning"
          error={errors.captions?.[i]?.message}
          {...register(`captions.${i}`)}
        />
      ))}
      {update.error && <Alert>{update.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={update.isPending}>
          Save task
        </Button>
      </DialogActions>
    </form>
  )
}
