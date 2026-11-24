import { useState } from 'react'
import { Link } from 'react-router'
import { caseDetailPath } from '@/app/paths'
import { Alert, Badge, Button, Dialog, DialogActions, type BadgeTone } from '@/shared/ui'
import { formatDateTime } from '@/shared/utils/format'
import { casesApi } from '../api'
import { useCaseMutation } from '../hooks'
import {
  DONE_BY_LABELS,
  TASK_STATUS_LABELS,
  type CaseDetail,
  type CaseTask,
  type TaskStatus,
} from '../types'
import { taskProgress } from '../task-progress'
import { TaskUpdateDialog } from './dialogs/TaskUpdateDialog'

const TONES: Record<TaskStatus, BadgeTone> = {
  pending: 'neutral',
  pass: 'positive',
  fail: 'negative',
  not_applicable: 'neutral',
  moved: 'warning',
}

/**
 * Meeting 07/11, the "Tasks" button: every task of the case with its status, who did it and when, the notes and the
 * photos. The technician updates a task (status, note, captioned photos) while working; the staff marks
 * the ones an outside party did, which the technician can then no longer change. The server says who may do what.
 */
export function TasksDialog({
  caseDetail,
  open,
  onClose,
}: {
  caseDetail: CaseDetail
  open: boolean
  onClose: () => void
}) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      wide
      title={caseDetail.isPreventive ? 'Checklist' : 'Tasks'}
      description={taskProgress(caseDetail)}
    >
      <TaskList caseDetail={caseDetail} />
      <DialogActions>
        <Button variant="ghost" onClick={onClose}>
          Close
        </Button>
      </DialogActions>
    </Dialog>
  )
}

function TaskList({ caseDetail }: { caseDetail: CaseDetail }) {
  const thirdParty = useCaseMutation(casesApi.setTaskThirdParty)
  const [editing, setEditing] = useState<CaseTask | null>(null)
  // Meeting note 18/11: notes and photos open from a button on each task.
  const [shown, setShown] = useState<{ taskId: number; part: 'notes' | 'photos' } | null>(null)
  const [zoomed, setZoomed] = useState<{
    url: string
    caption: string | null
    task: string
  } | null>(null)
  const technicianMay = caseDetail.allowedActions.includes('update_task')
  const staffMay = caseDetail.allowedActions.includes('set_task_third_party')

  if (caseDetail.tasks.length === 0) {
    return <p className="text-muted text-sm">No task yet. The staff adds them with Edit.</p>
  }

  return (
    <>
      <ol className="flex flex-col">
        {caseDetail.tasks.map((task, i) => {
          const byOutside = task.doneBy === 'third_party'
          return (
            <li
              key={task.id}
              className="border-rule border-b py-3 first:pt-0 last:border-0 last:pb-0"
            >
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div className="min-w-0 text-sm">
                  <span className="text-muted mr-2 tabular-nums">{i + 1}.</span>
                  <span className="font-medium">{task.name}</span>
                  {task.description && (
                    <p className="text-muted mt-0.5 ml-6 text-[13px]">{task.description}</p>
                  )}
                  <p className="text-muted mt-0.5 ml-6 text-xs tabular-nums">
                    {task.doneBy && task.doneAt
                      ? `${DONE_BY_LABELS[task.doneBy]} · ${formatDateTime(task.doneAt)}`
                      : 'Not done yet'}
                  </p>
                </div>
                <div className="flex flex-none flex-wrap items-center justify-end gap-1">
                  <Badge tone={TONES[task.status]}>{TASK_STATUS_LABELS[task.status]}</Badge>
                  {(['notes', 'photos'] as const).map((part) => {
                    const count = task[part].length
                    const open = shown?.taskId === task.id && shown.part === part
                    return (
                      <Button
                        key={part}
                        variant="ghost"
                        disabled={count === 0}
                        aria-expanded={open}
                        onClick={() => setShown(open ? null : { taskId: task.id, part })}
                      >
                        {part === 'notes' ? 'Notes' : 'Photos'} ({count})
                      </Button>
                    )
                  })}
                  {/* Meeting 07/11: a task an outside party did is the staff's, the technician no longer changes it. */}
                  {technicianMay && !byOutside && (
                    <Button variant="secondary" onClick={() => setEditing(task)}>
                      Update
                    </Button>
                  )}
                  {staffMay && (byOutside || task.status === 'pending') && (
                    <Button
                      variant="ghost"
                      disabled={thirdParty.isPending}
                      onClick={() =>
                        thirdParty.mutate({
                          id: caseDetail.id,
                          taskId: task.id,
                          thirdParty: !byOutside,
                        })
                      }
                    >
                      {byOutside ? 'Back to the technician' : 'Done by outside party'}
                    </Button>
                  )}
                </div>
              </div>
              {shown?.taskId === task.id && shown.part === 'notes' && (
                <ul className="bg-raised rounded-control mt-2 ml-6 flex flex-col gap-2 p-3 text-[13px]">
                  {task.notes.map((n) => (
                    <li key={n.at}>
                      {n.note} <span className="text-muted">· {formatDateTime(n.at)}</span>
                    </li>
                  ))}
                </ul>
              )}
              {shown?.taskId === task.id && shown.part === 'photos' && (
                <ul className="bg-raised rounded-control mt-2 ml-6 flex flex-wrap gap-3 p-3">
                  {task.photos.map((p) => (
                    <li key={p.url} className="w-24">
                      <button
                        type="button"
                        aria-label={`Enlarge ${p.caption ?? `a photo of ${task.name}`}`}
                        onClick={() => setZoomed({ ...p, task: task.name })}
                        className="flex w-24 cursor-zoom-in flex-col gap-1 text-left"
                      >
                        <img
                          src={p.url}
                          alt={p.caption ?? `Photo of ${task.name}`}
                          className="border-border size-24 rounded-lg border object-cover"
                        />
                        {p.caption && (
                          <span className="text-muted line-clamp-2 text-xs">{p.caption}</span>
                        )}
                      </button>
                    </li>
                  ))}
                </ul>
              )}
              {task.repairCaseId && (
                <p className="mt-2 ml-6 text-[13px]">
                  Repair case{' '}
                  <Link
                    to={caseDetailPath(task.repairCaseId)}
                    className="font-mono underline underline-offset-2"
                  >
                    {task.repairCaseCode}
                  </Link>
                </p>
              )}
            </li>
          )
        })}
      </ol>
      {thirdParty.error && (
        <div className="mt-3">
          <Alert>{thirdParty.error.message}</Alert>
        </div>
      )}
      {zoomed && (
        <Dialog open wide onClose={() => setZoomed(null)} title={zoomed.task}>
          <img
            src={zoomed.url}
            alt={zoomed.caption ?? `Photo of ${zoomed.task}`}
            className="max-h-[65vh] w-full rounded-lg object-contain"
          />
          {zoomed.caption && <p className="text-sm">{zoomed.caption}</p>}
          <DialogActions>
            <Button variant="ghost" onClick={() => setZoomed(null)}>
              Close
            </Button>
          </DialogActions>
        </Dialog>
      )}
      {editing && (
        <TaskUpdateDialog
          caseId={caseDetail.id}
          isPreventive={caseDetail.isPreventive}
          task={editing}
          open
          onClose={() => setEditing(null)}
        />
      )}
    </>
  )
}
