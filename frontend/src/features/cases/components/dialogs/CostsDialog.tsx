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

/**
 * Meeting 07/11: the staff records the costs (not the technician, who hands in the receipts). Any number of lines,
 * each a title, a description and an amount. Labour is computed by the system from the hours and the hourly rate.
 */
export function CostsDialog({ caseDetail, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      wide
      title="Costs"
      description="Parts, materials and outside services of this case. These lines replace the current ones."
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
      lines: (caseDetail.costs ?? []).map((c) => ({
        id: String(c.id),
        name: c.name,
        description: c.description ?? '',
        amount: String(c.amount),
      })),
    },
  })
  // keyName 'key': the rows carry their own "id" (the cost line id).
  const lines = useFieldArray({ control, name: 'lines', keyName: 'key' })
  const receipts = new Map((caseDetail.costs ?? []).map((c) => [String(c.id), c]))

  const onSubmit = handleSubmit((v) =>
    save.mutate({ id: caseDetail.id, lines: toCostLines(v.lines) }, { onSuccess: onDone }),
  )

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
      <CostLinesField
        rows={lines.fields}
        field={(i, key) => register(`lines.${i}.${key}`)}
        errors={errors.lines}
        onAdd={() => lines.append({ id: '', name: '', description: '', amount: '' })}
        onRemove={lines.remove}
        hint="Amounts in VND. A line the technician added keeps its receipt."
        extra={(i) => {
          const cost = receipts.get(lines.fields[i]?.id ?? '')
          if (!cost?.receiptCode && !cost?.receiptUrl) return null
          return (
            <p className="text-muted text-xs sm:col-span-4">
              Receipt {cost.receiptCode}
              {cost.receiptUrl && (
                <>
                  {' '}
                  <a
                    href={cost.receiptUrl}
                    target="_blank"
                    rel="noreferrer"
                    className="underline underline-offset-2"
                  >
                    photo
                  </a>
                </>
              )}
            </p>
          )
        }}
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
