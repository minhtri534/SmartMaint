import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, Select, TextField } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation, useCases } from '../../hooks'
import { mergeSchema, type MergeFormValues } from '../../schemas'
import { STATUS_LABELS, type CaseDetail, type CaseStatus } from '../../types'

interface Props {
  caseDetail: CaseDetail
  open: boolean
  onClose: () => void
}

const OPEN: readonly CaseStatus[] = [
  'approved',
  'assigned',
  'accepted',
  'in_progress',
  'on_hold',
  'reopened',
]

/**
 * F4 N5: several people report the same incident. This report is closed as merged (no KPI); its customer is told
 * when the main case is fixed. A main case below Emergency is raised to Emergency.
 */
export function MergeDialog({ caseDetail, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Merge into another case"
      description="Use this when the same incident is already being handled in this building."
    >
      <MergeForm caseDetail={caseDetail} onDone={onClose} />
    </Dialog>
  )
}

function MergeForm({ caseDetail, onDone }: { caseDetail: CaseDetail; onDone: () => void }) {
  const cases = useCases()
  const merge = useCaseMutation(casesApi.merge)
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<MergeFormValues>({
    resolver: zodResolver(mergeSchema),
    defaultValues: { targetCaseId: '', note: '' },
  })
  const targets = (cases.data ?? []).filter(
    (c) =>
      c.id !== caseDetail.id &&
      c.location.buildingId === caseDetail.location.buildingId &&
      OPEN.includes(c.status) &&
      !c.isPreventive &&
      !c.emergency?.mergedInto,
  )

  return (
    <form
      onSubmit={handleSubmit((v) =>
        merge.mutate(
          { id: caseDetail.id, targetCaseId: Number(v.targetCaseId), note: v.note || null },
          { onSuccess: onDone },
        ),
      )}
      noValidate
      className="flex flex-col gap-4"
    >
      <Select
        label="Main case *"
        placeholder={cases.isPending ? 'Loading…' : targets.length ? 'Choose…' : 'No open case'}
        error={errors.targetCaseId?.message}
        {...register('targetCaseId')}
      >
        {targets.map((c) => (
          <option key={c.id} value={c.id}>
            {c.code} · {c.title} · {c.location.roomName} · {STATUS_LABELS[c.status]}
          </option>
        ))}
      </Select>
      <TextField
        label="Note"
        hint="Optional"
        placeholder="e.g. Same burst pipe, reported from room 502"
        {...register('note')}
      />
      {merge.error && <Alert>{merge.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={merge.isPending}>
          Merge
        </Button>
      </DialogActions>
    </form>
  )
}
