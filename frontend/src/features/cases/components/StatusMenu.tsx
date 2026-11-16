import { useState } from 'react'
import { Alert, Button, Dialog, DialogActions, Select, TextField } from '@/shared/ui'
import { formatVnd } from '@/shared/utils/format'
import { casesApi } from '../api'
import { useCaseMutation } from '../hooks'
import { taskProgress } from '../task-progress'
import { STATUS_LABELS, type CaseDetail } from '../types'
import { workedHoursNow } from '../work-hours'

type Target = 'in_progress' | 'on_hold' | 'completed'

/**
 * Meeting 07/11: the technician moves the status with one dropdown — In progress (starts or resumes, the work clock
 * runs), On hold (the clock stops; no reason, no date), Completed (nothing to fill in). The choices are the arrows the
 * server allows now (start / resume / hold / complete).
 */
export function StatusMenu({ caseDetail }: { caseDetail: CaseDetail }) {
  const start = useCaseMutation(casesApi.start)
  const resume = useCaseMutation(casesApi.resume)
  const hold = useCaseMutation(casesApi.hold)
  // When the dialog opened (read in the click, not during render): the hours shown count up to then.
  const [completingAt, setCompletingAt] = useState<number | null>(null)
  const allowed = caseDetail.allowedActions
  const id = caseDetail.id

  const targets: { value: Target; label: string; go: () => void }[] = []
  if (allowed.includes('start'))
    targets.push({
      value: 'in_progress',
      label: 'In progress (start the work)',
      go: () => start.mutate({ id }),
    })
  if (allowed.includes('resume'))
    targets.push({
      value: 'in_progress',
      label: 'In progress (continue)',
      go: () => resume.mutate({ id }),
    })
  if (allowed.includes('hold'))
    targets.push({ value: 'on_hold', label: 'On hold', go: () => hold.mutate({ id }) })
  if (allowed.includes('complete'))
    targets.push({ value: 'completed', label: 'Completed', go: () => setCompletingAt(Date.now()) })
  if (targets.length === 0) return null

  const busy = start.isPending || resume.isPending || hold.isPending
  const error = start.error ?? resume.error ?? hold.error

  return (
    <div className="flex flex-col items-end gap-1">
      <Select
        label="Status"
        className="w-56"
        value=""
        disabled={busy}
        onChange={(e) => targets.find((t) => t.value === e.target.value)?.go()}
      >
        <option value="">{STATUS_LABELS[caseDetail.status]}</option>
        {targets.map((t) => (
          <option key={t.value} value={t.value}>
            → {t.label}
          </option>
        ))}
      </Select>
      {error && <Alert>{error.message}</Alert>}
      <CompleteDialog
        caseDetail={caseDetail}
        open={completingAt !== null}
        now={completingAt ?? 0}
        onClose={() => setCompletingAt(null)}
      />
    </div>
  )
}

/**
 * Completed: nothing to fill in (meeting 07/11). The dialog only shows what was recorded (tasks, hours, costs) so a
 * missing piece can still be added first. An asset an outside party replaced (old F5 flow) needs the new model.
 */
function CompleteDialog({
  caseDetail: c,
  open,
  now,
  onClose,
}: {
  caseDetail: CaseDetail
  open: boolean
  now: number
  onClose: () => void
}) {
  const complete = useCaseMutation(casesApi.complete)
  const [model, setModel] = useState('')
  const [serial, setSerial] = useState('')
  const needsReplacement = c.thirdParties.some((t) => t.status === 'done' && t.job === 'replace')
  const hours = workedHoursNow(c, now)
  const open_ = c.tasks.filter((t) => t.status === 'pending').length
  const costs = c.costs?.filter((x) => x.name !== 'Labour') ?? []

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Set to Completed"
      description="The staff checks the work and closes the case."
    >
      <dl className="flex flex-col gap-2 text-sm">
        <Line label="Tasks">{taskProgress(c)}</Line>
        <Line label="Work hours">{hours.toFixed(2)} h</Line>
        <Line label="Costs you added">
          {costs.length === 0
            ? 'None'
            : `${costs.length} · ${formatVnd(costs.reduce((s, x) => s + x.amount, 0))}`}
        </Line>
      </dl>
      {(open_ > 0 || hours === 0) && (
        <p className="text-muted mt-3 text-sm">
          {open_ > 0 && `${open_} ${open_ === 1 ? 'task is' : 'tasks are'} not done yet. `}
          {hours === 0 && 'No work hours recorded. '}
          You can still go back and add them, or complete now.
        </p>
      )}
      {needsReplacement && (
        <div className="mt-4 grid gap-3 sm:grid-cols-2">
          <TextField
            label="New asset model *"
            value={model}
            onChange={(e) => setModel(e.target.value)}
          />
          <TextField label="Serial" value={serial} onChange={(e) => setSerial(e.target.value)} />
        </div>
      )}
      {complete.error && (
        <div className="mt-3">
          <Alert>{complete.error.message}</Alert>
        </div>
      )}
      <DialogActions>
        <Button variant="ghost" onClick={onClose}>
          Back
        </Button>
        <Button
          variant="primary"
          loading={complete.isPending}
          onClick={() =>
            complete.mutate(
              { id: c.id, replacementModel: model || null, replacementSerial: serial || null },
              { onSuccess: onClose },
            )
          }
        >
          Set to Completed
        </Button>
      </DialogActions>
    </Dialog>
  )
}

function Line({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="flex items-baseline justify-between gap-3">
      <dt className="text-muted">{label}</dt>
      <dd className="tabular-nums">{children}</dd>
    </div>
  )
}
