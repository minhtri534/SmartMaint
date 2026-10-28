import { useState } from 'react'
import { useAuth } from '@/features/auth'
import { Alert, Button, ConfirmDialog } from '@/shared/ui'
import { casesApi } from '../api'
import { useCaseMutation } from '../hooks'
import type { CaseAction, CaseDetail } from '../types'
import { ApproveDialog } from './dialogs/ApproveDialog'
import { AssignDialog } from './dialogs/AssignDialog'
import { CompleteWorkDialog } from './dialogs/CompleteWorkDialog'
import { HoldDialog } from './dialogs/HoldDialog'
import { RateDialog } from './dialogs/RateDialog'
import { ReasonDialog } from './dialogs/ReasonDialog'
import { RejectDialog } from './dialogs/RejectDialog'

/**
 * Button text and style per action, in display order: secondary actions first so the main one ends up on the right.
 * At most one primary per status (the state machine never offers two primary actions at once).
 */
const BUTTONS: Record<CaseAction, { label: string; variant: 'primary' | 'secondary' | 'danger' }> =
  {
    cancel: { label: 'Cancel case', variant: 'danger' },
    reject: { label: 'Reject', variant: 'secondary' },
    decline: { label: 'Decline job', variant: 'secondary' },
    reopen: { label: 'Not OK, reopen', variant: 'secondary' },
    hold: { label: 'Put on hold', variant: 'secondary' },
    approve: { label: 'Approve', variant: 'primary' },
    assign: { label: 'Assign technician', variant: 'primary' },
    accept: { label: 'Accept job', variant: 'primary' },
    start: { label: 'Start work on site', variant: 'primary' },
    resume: { label: 'Resume work', variant: 'secondary' },
    complete: { label: 'Report completed', variant: 'primary' },
    close: { label: 'Verify and close', variant: 'primary' },
    rate: { label: 'Rate the service', variant: 'primary' },
  }
const ORDER = Object.keys(BUTTONS) as CaseAction[]

/**
 * The buttons the signed-in user may press on this case, and the dialog each one opens.
 * Which buttons exist is decided by the server (caseDetail.allowedActions): the web only draws them (W4).
 */
export function CaseActions({ caseDetail }: { caseDetail: CaseDetail }) {
  const { user } = useAuth()
  const [open, setOpen] = useState<CaseAction | null>(null)
  const accept = useCaseMutation(casesApi.accept)
  const start = useCaseMutation(casesApi.start)
  const resume = useCaseMutation(casesApi.resume)
  const close = useCaseMutation(casesApi.close)

  const actions = caseDetail.allowedActions
  if (actions.length === 0) return null

  const id = caseDetail.id
  const onClose = () => setOpen(null)
  // Actions without a dialog run straight away.
  const direct: Partial<Record<CaseAction, () => void>> = {
    accept: () => accept.mutate({ id }),
    start: () => start.mutate({ id }),
    resume: () => resume.mutate({ id }),
  }
  const pending: Partial<Record<CaseAction, boolean>> = {
    accept: accept.isPending,
    start: start.isPending,
    resume: resume.isPending,
  }
  const directError = accept.error ?? start.error ?? resume.error
  const label = (action: CaseAction) =>
    action === 'resume' && user?.role === 'staff'
      ? 'Let technician continue'
      : BUTTONS[action].label

  return (
    <div className="flex flex-col items-end gap-2">
      <div className="flex flex-wrap justify-end gap-2">
        {ORDER.filter((a) => actions.includes(a)).map((action) => (
          <Button
            key={action}
            variant={BUTTONS[action].variant}
            loading={pending[action]}
            onClick={direct[action] ?? (() => setOpen(action))}
          >
            {label(action)}
          </Button>
        ))}
      </div>
      {directError && <Alert>{directError.message}</Alert>}

      <ApproveDialog caseDetail={caseDetail} open={open === 'approve'} onClose={onClose} />
      <RejectDialog caseDetail={caseDetail} open={open === 'reject'} onClose={onClose} />
      <AssignDialog caseDetail={caseDetail} open={open === 'assign'} onClose={onClose} />
      <HoldDialog caseId={id} open={open === 'hold'} onClose={onClose} />
      <CompleteWorkDialog caseId={id} open={open === 'complete'} onClose={onClose} />
      <RateDialog caseId={id} open={open === 'rate'} onClose={onClose} />
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
        open={open === 'reopen'}
        onClose={onClose}
        title="Reopen case"
        description="The same technician gets the case back and must accept it again (BR-04). The deadline starts over when work restarts."
        label="What is not OK?"
        placeholder="e.g. The AC is cooling again but still drips water."
        confirmLabel="Reopen"
        action={casesApi.reopen}
      />
      <ConfirmDialog
        open={open === 'close'}
        onClose={onClose}
        title="Verify and close"
        description="Closing records the repair in the asset history and notifies the customer."
        confirmLabel="Close case"
        loading={close.isPending}
        error={close.error?.message}
        onConfirm={() => close.mutate({ id }, { onSuccess: onClose })}
      >
        <p className="text-sm">
          I checked the repair record and the before / after photos, and the problem is fixed.
        </p>
      </ConfirmDialog>
    </div>
  )
}
