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
              {(task.notes.length > 0 || task.photos.length > 0 || task.repairCaseId) && (
                <div className="mt-2 ml-6 flex flex-col gap-2 text-[13px]">
                  {task.notes.map((n) => (
                    <p key={n.at}>
                      {n.note} <span className="text-muted">· {formatDateTime(n.at)}</span>
                    </p>
                  ))}
                  {task.photos.length > 0 && (
                    <div className="flex flex-wrap gap-3">
                      {task.photos.map((p) => (
                        <a
                          key={p.url}
                          href={p.url}
                          target="_blank"
                          rel="noreferrer"
                          className="flex w-24 flex-col gap-1"
                        >
                          <img
                            src={p.url}
                            alt={p.caption ?? `Photo of ${task.name}`}
                            className="border-border size-24 rounded-lg border object-cover"
                          />
                          {p.caption && (
                            <span className="text-muted line-clamp-2 text-xs">{p.caption}</span>
                          )}
                        </a>
                      ))}
                    </div>
                  )}
                  {task.repairCaseId && (
                    <p>
                      Repair case{' '}
                      <Link
                        to={caseDetailPath(task.repairCaseId)}
                        className="font-mono underline underline-offset-2"
                      >
                        {task.repairCaseCode}
                      </Link>
                    </p>
                  )}
                </div>
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
