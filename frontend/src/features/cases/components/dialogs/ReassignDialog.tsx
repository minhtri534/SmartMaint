import { useState } from 'react'
import { useNavigate } from 'react-router'
import { caseDetailPath } from '@/app/paths'
import { Alert, Button, Dialog, DialogActions, LoadingBlock } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation, useTechnicians } from '../../hooks'
import type { CaseDetail, CaseStatus } from '../../types'
import { TechnicianPicker } from '../TechnicianPicker'

/** The technician already took the job, so another one continues it in a new case. */
const TAKEN: readonly CaseStatus[] = ['accepted', 'in_progress', 'on_hold']

/**
 * Meeting note 18/11: the technician is changed here, not in the Edit form. Nobody accepted yet: the same case gets the
 * new technician. Already accepted: a new case goes on with the unfinished tasks; this one closes with its whole list
 * (the unfinished ones marked moved) and is never reopened.
 */
export function ReassignDialog({
  caseDetail: c,
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
      title={c.technician ? 'Reassign technician' : 'Assign a technician'}
      description={
        c.technician ? `Now with ${c.technician.fullName}.` : 'Nobody has this case now.'
      }
    >
      {open && <ReassignForm caseDetail={c} onDone={onClose} />}
    </Dialog>
  )
}

function ReassignForm({ caseDetail: c, onDone }: { caseDetail: CaseDetail; onDone: () => void }) {
  const navigate = useNavigate()
  const technicians = useTechnicians(true, c.id)
  const reassign = useCaseMutation(casesApi.reassign)
  const [technicianId, setTechnicianId] = useState('')
  const [missing, setMissing] = useState(false)
  const taken = TAKEN.includes(c.status)

  if (technicians.error) return <Alert>{technicians.error.message}</Alert>
  if (!technicians.data) return <LoadingBlock rows={4} />

  const submit = () => {
    if (!technicianId) {
      setMissing(true)
      return
    }
    // mutateAsync: the refreshed case can unmount this dialog before a mutate callback runs.
    reassign
      .mutateAsync({ id: c.id, technicianId: Number(technicianId) })
      .then((r) => navigate(caseDetailPath(r.nextCase?.id ?? c.id), { replace: true }))
      .catch(() => undefined)
  }

  return (
    <div className="flex flex-col gap-4">
      <TechnicianPicker
        technicians={technicians.data.filter((t) => t.id !== c.technician?.id)}
        value={technicianId}
        onChange={(id) => {
          setTechnicianId(id)
          setMissing(false)
        }}
        caseId={c.id}
        error={missing ? 'Choose a technician.' : undefined}
      />
      {taken && (
        <Alert tone="notice">
          {c.technician?.fullName} already accepted this job. A new case opens for the new
          technician with the unfinished tasks. This case closes and keeps every task, the
          unfinished ones marked “Moved to the new case”; it cannot be reopened.
        </Alert>
      )}
      {reassign.error && <Alert>{reassign.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button variant="primary" loading={reassign.isPending} onClick={submit}>
          {taken ? 'Reassign in a new case' : 'Reassign'}
        </Button>
      </DialogActions>
    </div>
  )
}
