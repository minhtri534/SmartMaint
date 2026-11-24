import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
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
 * TEC-05, meeting 07/11: the technician updates one task: its status, a note and photos. Meeting note 18/11: each new
 * photo is confirmed first, with its caption. "Not done" is fine too, to write down progress along the way.
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
      status: task.status === 'pending' || task.status === 'moved' ? 'pass' : task.status,
      note: '',
      photos: [],
      captions: [],
      failAction: 'fixed_on_site',
      dangerous: false,
    },
  })
  const [status, captions, failAction] = useWatch({
    control,
    name: ['status', 'captions', 'failAction'],
  })
  // Photos picked but not confirmed yet, one confirm screen at a time.
  const [picked, setPicked] = useState<string[]>([])

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
            captions={captions}
            onPicked={setPicked}
            error={errors.photos?.message}
            hint="Up to 5. You can add a caption when you confirm each photo."
          />
        )}
      />
      {picked[0] && (
        <PhotoConfirmDialog
          key={picked[0].slice(-60) + picked.length}
          photo={picked[0]}
          remaining={picked.length - 1}
          onDiscard={() => setPicked(picked.slice(1))}
          onConfirm={(caption) => {
            setValue('photos', [...getValues('photos'), picked[0]!], { shouldValidate: true })
            setValue('captions', [...getValues('captions'), caption])
            setPicked(picked.slice(1))
          }}
        />
      )}
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

/** Meeting note 18/11: a new photo joins the task only after this screen, where its caption is written. */
function PhotoConfirmDialog({
  photo,
  remaining,
  onDiscard,
  onConfirm,
}: {
  photo: string
  /** How many more photos wait after this one. */
  remaining: number
  onDiscard: () => void
  onConfirm: (caption: string) => void
}) {
  const [caption, setCaption] = useState('')
  const tooLong = caption.trim().length > 200
  return (
    <Dialog
      open
      onClose={onDiscard}
      title="Add this photo?"
      description={remaining > 0 ? `${remaining} more after this one.` : undefined}
    >
      <img
        src={photo}
        alt="The photo you picked"
        className="border-border max-h-[45vh] w-full rounded-lg border object-contain"
      />
      <TextField
        label="Caption"
        placeholder="Optional, e.g. Before cleaning"
        value={caption}
        error={tooLong ? 'At most 200 characters.' : undefined}
        onChange={(e) => setCaption(e.target.value)}
      />
      <DialogActions>
        <Button variant="ghost" onClick={onDiscard}>
          Discard
        </Button>
        <Button variant="primary" disabled={tooLong} onClick={() => onConfirm(caption.trim())}>
          Add photo
        </Button>
      </DialogActions>
    </Dialog>
  )
}
