import { zodResolver } from '@hookform/resolvers/zod'
import { useFieldArray, useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { costsSchema, toCostLines, type CostsFormValues } from '../../schemas'
import type { CaseDetail } from '../../types'
import { CostLinesField } from '../CostLinesField'

interface Props {
  caseDetail: CaseDetail
  open: boolean
  onClose: () => void
}

/** Staff corrects the cost lines (parts + labour) while the case waits for verification. */
export function CostsDialog({ caseDetail, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      wide
      title="Correct costs"
      description="These lines replace the current ones. The change is kept in the history."
    >
      <CostsForm caseDetail={caseDetail} onDone={onClose} />
    </Dialog>
  )
}

function CostsForm({ caseDetail, onDone }: { caseDetail: CaseDetail; onDone: () => void }) {
  const save = useCaseMutation(casesApi.updateCosts)
  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<CostsFormValues>({
    resolver: zodResolver(costsSchema),
    defaultValues: {
      lines: (caseDetail.costs ?? []).map((c) => ({ name: c.name, amount: String(c.amount) })),
    },
  })
  const lines = useFieldArray({ control, name: 'lines' })

  const onSubmit = handleSubmit((v) =>
    save.mutate({ id: caseDetail.id, lines: toCostLines(v.lines) }, { onSuccess: onDone }),
  )

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
      <CostLinesField
        rows={lines.fields}
        field={(i, key) => register(`lines.${i}.${key}`)}
        errors={errors.lines}
        onAdd={() => lines.append({ name: '', amount: '' })}
        onRemove={lines.remove}
        hint="Parts and labour of this repair, in VND."
      />
      {save.error && <Alert>{save.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={save.isPending}>
          Save costs
        </Button>
      </DialogActions>
    </form>
  )
}
