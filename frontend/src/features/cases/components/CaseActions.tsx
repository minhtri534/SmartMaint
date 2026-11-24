import { useState } from 'react'
import { useNavigate } from 'react-router'
import { caseApprovePath, caseEditPath } from '@/app/paths'
import { useAuth } from '@/features/auth'
import { Alert, Button, ConfirmDialog } from '@/shared/ui'
import { casesApi } from '../api'
import { useCaseMutation } from '../hooks'
import type { CaseAction, CaseDetail } from '../types'
import { AddCostDialog } from './dialogs/AddCostDialog'
import { CostsDialog } from './dialogs/CostsDialog'
import { EmergencyLogDialog } from './dialogs/EmergencyLogDialog'
import { ExtendDueDialog } from './dialogs/ExtendDueDialog'
import { FollowersDialog } from './dialogs/FollowersDialog'
import { IncidentReportDialog } from './dialogs/IncidentReportDialog'
import { RateDialog } from './dialogs/RateDialog'
import { ReasonDialog } from './dialogs/ReasonDialog'
import { RejectDialog } from './dialogs/RejectDialog'
import { RequestDecisionDialog } from './dialogs/RequestDecisionDialog'
import { taskProgress } from '../task-progress'
import { StatusMenu } from './StatusMenu'
import { TasksDialog } from './TaskList'
import { WorkHoursDialog } from './dialogs/WorkHoursDialog'

/**
 * Button text and style per action, in display order: secondary actions first so the main one ends up on the right.
 * Meeting 07/11: the staff's change urgency / let continue / close / reopen are one "Edit" (the work order form, with
 * the status change in it; Reassign is inside it); merge, downgrade and the quote steps are gone. The technician's
 * start / hold / resume / complete are one status dropdown (StatusMenu); "Start work on site" is gone.
 */
const BUTTONS: Partial<
  Record<CaseAction, { label: string; variant: 'primary' | 'secondary' | 'danger' }>
> = {
  cancel: { label: 'Cancel case', variant: 'danger' },
  reject: { label: 'Reject', variant: 'secondary' },
  decline: { label: 'Decline job', variant: 'secondary' },
  request_decision: { label: 'Needs outside party', variant: 'secondary' },
  log_hours: { label: 'Work hours', variant: 'secondary' },
  add_cost: { label: 'Add cost', variant: 'secondary' },
  extend_due: { label: 'Move deadline', variant: 'secondary' },
  update_costs: { label: 'Enter costs', variant: 'secondary' },
  manage_followers: { label: 'Followers', variant: 'secondary' },
  outsource: { label: 'Hand to outside party', variant: 'secondary' },
  add_log: { label: 'Add to log', variant: 'secondary' },
  write_incident_report: { label: 'Post-incident report', variant: 'secondary' },
  report_not_fixed: { label: 'Still not fixed', variant: 'secondary' },
  decline_not_fixed: { label: 'Decline report', variant: 'secondary' },
  edit: { label: 'Edit', variant: 'secondary' },
  reopen: { label: 'Reopen', variant: 'secondary' },
  approve: { label: 'Approve and assign', variant: 'primary' },
  acknowledge: { label: 'Acknowledge', variant: 'primary' },
  accept: { label: 'Accept job', variant: 'primary' },
  accept_not_fixed: { label: 'Reopen for the customer', variant: 'primary' },
  rate: { label: 'Rate the service', variant: 'primary' },
}
const ORDER = Object.keys(BUTTONS) as CaseAction[]

/**
 * The buttons the signed-in user may press on this case, and the dialog each one opens.
 * Which buttons exist is decided by the server (caseDetail.allowedActions): the web only draws them (W4).
 */
export function CaseActions({ caseDetail }: { caseDetail: CaseDetail }) {
  const { user } = useAuth()
  const navigate = useNavigate()
  const [open, setOpen] = useState<CaseAction | 'tasks' | null>(null)
  const accept = useCaseMutation(casesApi.accept)
  const acceptNotFixed = useCaseMutation(casesApi.acceptNotFixed)
  const acknowledge = useCaseMutation(casesApi.acknowledge)
  const outsource = useCaseMutation(casesApi.outsource)

  const actions = ORDER.filter((a) => caseDetail.allowedActions.includes(a))
  // Meeting 07/11: a separate "Tasks" button for everyone who works on the case.
  const showTasks = user?.role !== 'customer' && caseDetail.tasks.length > 0
  const showStatus = ['start', 'resume', 'hold', 'complete'].some((a) =>
    caseDetail.allowedActions.includes(a as CaseAction),
  )
  if (actions.length === 0 && !showTasks && !showStatus) return null

  const id = caseDetail.id
  const onClose = () => setOpen(null)
  // Actions without a dialog run straight away; approve and edit open the work order form.
  const direct: Partial<Record<CaseAction, () => void>> = {
    accept: () => accept.mutate({ id }),
    acknowledge: () => acknowledge.mutate({ id }),
    approve: () => navigate(caseApprovePath(id)),
    edit: () => navigate(caseEditPath(id)),
  }
  const pending: Partial<Record<CaseAction, boolean>> = {
    accept: accept.isPending,
    acknowledge: acknowledge.isPending,
  }
  const directError = accept.error ?? acknowledge.error
  // One primary at a time: an unacknowledged emergency is acknowledged first; a completed case is checked with Edit.
  const variant = (action: CaseAction) =>
    action === 'edit' && caseDetail.status === 'completed' && !actions.includes('acknowledge')
      ? 'primary'
      : BUTTONS[action]!.variant
  const label = (action: CaseAction) =>
    action === 'edit' && caseDetail.status === 'completed'
      ? 'Check and close'
      : BUTTONS[action]!.label

  return (
    <div className="flex flex-col items-end gap-2">
      {showStatus && <StatusMenu caseDetail={caseDetail} />}
      <div className="flex flex-wrap justify-end gap-2">
        {showTasks && (
          <Button variant="secondary" onClick={() => setOpen('tasks')}>
            Tasks{' '}
            <span className="text-muted ml-1 text-xs tabular-nums">{taskProgress(caseDetail)}</span>
          </Button>
        )}
        {actions.map((action) => (
          <Button
            key={action}
            variant={variant(action)}
            loading={pending[action]}
            onClick={direct[action] ?? (() => setOpen(action))}
          >
            {label(action)}
            {action === 'manage_followers' && caseDetail.followers.length > 0 && (
              <span className="text-muted ml-1 text-xs tabular-nums">
                {caseDetail.followers.length}
              </span>
            )}
          </Button>
        ))}
      </div>
      {directError && <Alert>{directError.message}</Alert>}

      <TasksDialog caseDetail={caseDetail} open={open === 'tasks'} onClose={onClose} />
      <RejectDialog caseDetail={caseDetail} open={open === 'reject'} onClose={onClose} />
      <RequestDecisionDialog caseId={id} open={open === 'request_decision'} onClose={onClose} />
      <WorkHoursDialog caseDetail={caseDetail} open={open === 'log_hours'} onClose={onClose} />
      <AddCostDialog caseId={id} open={open === 'add_cost'} onClose={onClose} />
      <RateDialog caseId={id} open={open === 'rate'} onClose={onClose} />
      <ExtendDueDialog caseDetail={caseDetail} open={open === 'extend_due'} onClose={onClose} />
      <CostsDialog caseDetail={caseDetail} open={open === 'update_costs'} onClose={onClose} />
      <FollowersDialog
        caseDetail={caseDetail}
        open={open === 'manage_followers'}
        onClose={onClose}
      />
      <EmergencyLogDialog caseId={id} open={open === 'add_log'} onClose={onClose} />
      <IncidentReportDialog
        caseDetail={caseDetail}
        open={open === 'write_incident_report'}
        onClose={onClose}
      />
      <ConfirmDialog
        open={open === 'outsource'}
        onClose={onClose}
        title="Hand to an outside party"
        description="Agree the job with the party outside the system first. The case waits on hold; mark the tasks they do in Tasks, then let the technician continue with Edit."
        confirmLabel="Hand over"
        loading={outsource.isPending}
        error={outsource.error?.message}
        onConfirm={() => outsource.mutate({ id }, { onSuccess: onClose })}
      >
        {caseDetail.assessment && (
          <p className="text-sm">The technician says: “{caseDetail.assessment.condition}”</p>
        )}
      </ConfirmDialog>
      <ReasonDialog
        caseId={id}
        open={open === 'cancel'}
        onClose={onClose}
        title="Cancel case"
        description={
          user?.role === 'customer'
            ? 'You can cancel while the case is still waiting for review.'
            : 'The customer and the technician (if any) are notified.'
        }
        label="Reason"
        confirmLabel="Cancel case"
        danger
        action={casesApi.cancel}
      />
      <ReasonDialog
        caseId={id}
        open={open === 'decline'}
        onClose={onClose}
        title="Decline job"
        description="The case goes back to the staff. Declines and reasons are visible to the manager."
        label="Why can't you take it?"
        placeholder="e.g. Not my specialty / already fully booked today"
        confirmLabel="Decline"
        danger
        action={casesApi.decline}
      />
      <ReasonDialog
        caseId={id}
        open={open === 'report_not_fixed'}
        onClose={onClose}
        title="Still not fixed"
        description="The staff of the building decide whether to reopen the case. You can do this within 7 days of closing."
        label="What is still wrong?"
        placeholder="e.g. The AC blows warm air again since this morning."
        confirmLabel="Send"
        action={casesApi.reportNotFixed}
      />
      <ReasonDialog
        caseId={id}
        open={open === 'reopen'}
        onClose={onClose}
        title="Reopen the case"
        description="The same technician gets it back and must accept it again. The deadline starts over when work restarts."
        label="Why is it reopened?"
        placeholder="e.g. The customer says the AC blows warm air again."
        confirmLabel="Reopen"
        action={casesApi.reopen}
      />
      <ReasonDialog
        caseId={id}
        open={open === 'decline_not_fixed'}
        onClose={onClose}
        title="Decline the report"
        description="The case stays closed. The customer reads your answer."
        label="Your answer to the customer"
        placeholder="e.g. Checked on site: the room cools to 24 degrees."
        confirmLabel="Decline"
        danger
        action={casesApi.declineNotFixed}
      />
      <ConfirmDialog
        open={open === 'accept_not_fixed'}
        onClose={onClose}
        title="Reopen for the customer"
        description="The same technician gets the case back and must accept it again. The deadline starts over when work restarts."
        confirmLabel="Reopen"
        loading={acceptNotFixed.isPending}
        error={acceptNotFixed.error?.message}
        onConfirm={() => acceptNotFixed.mutate({ id }, { onSuccess: onClose })}
      >
        {caseDetail.notFixed && <p className="text-sm">“{caseDetail.notFixed.reason}”</p>}
      </ConfirmDialog>
    </div>
  )
}
