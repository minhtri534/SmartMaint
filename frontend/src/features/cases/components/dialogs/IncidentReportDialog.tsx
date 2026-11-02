import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, Select, TextArea } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { incidentReportSchema, type IncidentReportFormValues } from '../../schemas'
import { FOLLOW_UP_LABELS, type CaseDetail, type IncidentFollowUp } from '../../types'

interface Props {
  caseDetail: CaseDetail
  open: boolean
  onClose: () => void
}

/** F4 step ⑫ (K08): optional post-incident report of a closed emergency; may be rewritten. */
export function IncidentReportDialog({ caseDetail, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Post-incident report"
      wide
      description="What the incident cost and what to change so it does not happen again."
    >
      <ReportForm caseDetail={caseDetail} onDone={onClose} />
    </Dialog>
  )
}

function ReportForm({ caseDetail, onDone }: { caseDetail: CaseDetail; onDone: () => void }) {
  const write = useCaseMutation(casesApi.writeIncidentReport)
  const report = caseDetail.incidentReport
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<IncidentReportFormValues>({
    resolver: zodResolver(incidentReportSchema),
    defaultValues: {
      damage: report?.damage ?? '',
      lessons: report?.lessons ?? '',
      followUp: report?.followUp ?? 'none',
    },
  })

  return (
    <form
      onSubmit={handleSubmit((v) =>
        write.mutate({ id: caseDetail.id, ...v }, { onSuccess: onDone }),
      )}
      noValidate
      className="flex flex-col gap-4"
    >
      <TextArea
        label="Damage *"
        placeholder="e.g. Corridor floor wet for an hour, ceiling tile of room 102 stained"
        rows={3}
        error={errors.damage?.message}
        {...register('damage')}
      />
      <TextArea
        label="Cause and lessons *"
        hint={
          caseDetail.repairReport
            ? `Technician's cause: ${caseDetail.repairReport.rootCause}`
            : undefined
        }
        placeholder="e.g. The pipe joints in the restrooms are 15 years old"
        rows={3}
        error={errors.lessons?.message}
        {...register('lessons')}
      />
      <Select label="What next" {...register('followUp')}>
        {(Object.keys(FOLLOW_UP_LABELS) as IncidentFollowUp[]).map((f) => (
          <option key={f} value={f}>
            {FOLLOW_UP_LABELS[f]}
          </option>
        ))}
      </Select>
      {write.error && <Alert>{write.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={write.isPending}>
          Save report
        </Button>
      </DialogActions>
    </form>
  )
}
