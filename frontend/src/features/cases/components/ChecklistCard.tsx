import { useState } from 'react'
import { Link } from 'react-router'
import { caseDetailPath } from '@/app/paths'
import { Alert, Badge, Button, Card, CardHeader, type BadgeTone } from '@/shared/ui'
import { formatDateTime } from '@/shared/utils/format'
import { casesApi } from '../api'
import { useCaseMutation } from '../hooks'
import { TASK_STATUS_LABELS, type CaseDetail, type CaseTask, type TaskStatus } from '../types'
import { FailTaskDialog } from './dialogs/FailTaskDialog'

const TONES: Record<TaskStatus, BadgeTone> = {
  pending: 'neutral',
  pass: 'positive',
  fail: 'negative',
  not_applicable: 'neutral',
}

/**
 * F2 TEC-05 The checklist of a preventive work order. The technician marks each item Pass / Fail / N/A while the work
 * is in progress (the server says when: allowedActions has "update_task"); everyone else reads the results.
 */
export function ChecklistCard({ caseDetail }: { caseDetail: CaseDetail }) {
  const update = useCaseMutation(casesApi.updateTask)
  const [failing, setFailing] = useState<CaseTask | null>(null)
  const editable = caseDetail.allowedActions.includes('update_task')
  const tasks = caseDetail.tasks
  const done = tasks.filter((t) => t.status !== 'pending').length

  const mark = (task: CaseTask, status: 'pass' | 'not_applicable') =>
    update.mutate({
      id: caseDetail.id,
      taskId: task.id,
      status,
      note: null,
      photos: [],
      failAction: null,
      dangerous: false,
    })

  return (
    <Card>
      <CardHeader title="Checklist" aside={`${done} of ${tasks.length} done`} />
      <ol className="flex flex-col">
        {tasks.map((task, i) => (
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
              </div>
              {editable ? (
                <div className="flex flex-none gap-1">
                  <ResultButton
                    active={task.status === 'pass'}
                    disabled={update.isPending}
                    onClick={() => mark(task, 'pass')}
                  >
                    Pass
                  </ResultButton>
                  <ResultButton
                    active={task.status === 'fail'}
                    disabled={update.isPending}
                    onClick={() => (update.reset(), setFailing(task))}
                  >
                    Fail
                  </ResultButton>
                  <ResultButton
                    active={task.status === 'not_applicable'}
                    disabled={update.isPending}
                    onClick={() => mark(task, 'not_applicable')}
                  >
                    N/A
                  </ResultButton>
                </div>
              ) : (
                <Badge tone={TONES[task.status]}>{TASK_STATUS_LABELS[task.status]}</Badge>
              )}
            </div>
            {(task.notes.length > 0 || task.photoUrls.length > 0 || task.repairCaseId) && (
              <div className="mt-2 ml-6 flex flex-col gap-2 text-[13px]">
                {task.notes.map((n) => (
                  <p key={n.at}>
                    {n.note} <span className="text-muted">· {formatDateTime(n.at)}</span>
                  </p>
                ))}
                {task.photoUrls.length > 0 && (
                  <div className="flex flex-wrap gap-2">
                    {task.photoUrls.map((url) => (
                      <a key={url} href={url} target="_blank" rel="noreferrer">
                        <img
                          src={url}
                          alt={`Photo of ${task.name}`}
                          className="border-border size-16 rounded-lg border object-cover"
                        />
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
        ))}
      </ol>
      {update.error && !failing && (
        <div className="mt-3">
          <Alert>{update.error.message}</Alert>
        </div>
      )}
      {failing && (
        <FailTaskDialog
          caseId={caseDetail.id}
          task={failing}
          open
          onClose={() => setFailing(null)}
        />
      )}
    </Card>
  )
}

function ResultButton({
  active,
  disabled,
  onClick,
  children,
}: {
  active: boolean
  disabled: boolean
  onClick: () => void
  children: string
}) {
  return (
    <Button
      variant={active ? 'primary' : 'secondary'}
      disabled={disabled}
      aria-pressed={active}
      onClick={onClick}
    >
      {children}
    </Button>
  )
}
