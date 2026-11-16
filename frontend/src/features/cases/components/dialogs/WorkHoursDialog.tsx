import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, TextField } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { logHoursSchema, type LogHoursFormValues } from '../../schemas'
import type { CaseDetail } from '../../types'
import { workedHoursNow } from '../../work-hours'

interface Props {
  caseDetail: CaseDetail
  open: boolean
  onClose: () => void
}

/**
 * Meeting 07/11, "Work hours": the clock runs by itself while the case is In progress and stops on hold or when
 * completed. Forgot to start it? Type the total here; a running clock goes on from that total.
 */
export function WorkHoursDialog({ caseDetail, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Work hours"
      description="The clock runs while the case is In progress. Type the total if it is wrong."
    >
      <HoursForm caseDetail={caseDetail} onDone={onClose} />
    </Dialog>
  )
}

function HoursForm({ caseDetail, onDone }: { caseDetail: CaseDetail; onDone: () => void }) {
  const save = useCaseMutation(casesApi.logHours)
  const now = useNow(caseDetail.clockStartedAt !== null)
  const current = workedHoursNow(caseDetail, now)
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LogHoursFormValues>({
    resolver: zodResolver(logHoursSchema),
    defaultValues: { hours: current > 0 ? current.toFixed(2) : '' },
  })

  return (
    <form
      onSubmit={handleSubmit((v) =>
        save.mutate({ id: caseDetail.id, hours: Number(v.hours) }, { onSuccess: onDone }),
      )}
      noValidate
      className="flex flex-col gap-4"
    >
      <p className="text-sm">
        <span className="text-section font-semibold tabular-nums">{current.toFixed(2)} h</span>{' '}
        <span className="text-muted">
          {caseDetail.clockStartedAt ? '· the clock is running' : '· the clock is stopped'}
        </span>
      </p>
      <TextField
        label="Total hours"
        inputMode="decimal"
        hint="e.g. 1.5 for an hour and a half."
        error={errors.hours?.message}
        {...register('hours')}
      />
      {save.error && <Alert>{save.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={save.isPending}>
          Save hours
        </Button>
      </DialogActions>
    </form>
  )
}

/** The current time, ticking every 30 s while the clock runs. */
function useNow(running: boolean) {
  const [now, setNow] = useState(() => Date.now())
  useEffect(() => {
    if (!running) return
    const timer = window.setInterval(() => setNow(Date.now()), 30_000)
    return () => window.clearInterval(timer)
  }, [running])
  return now
}
