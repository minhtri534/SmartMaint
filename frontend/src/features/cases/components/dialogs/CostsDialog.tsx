import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useFieldArray, useForm } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import { groupDigits } from '../../amount'
import { costsSchema, toCostLines, type CostsFormValues } from '../../schemas'
import type { CaseDetail } from '../../types'
import { CostLinesField } from '../CostLinesField'

interface Props {
  caseDetail: CaseDetail
  open: boolean
  onClose: () => void
}

/**
 * Meeting note 18/11, "Enter costs": the staff records what the case cost. Any number of lines, each a title, a
 * description and an amount. Labour is computed by the system from the hours and the hourly rate.
 */
export function CostsDialog({ caseDetail, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      wide
      title="Enter costs"
      description="Record each cost of this case with a title, a description and an amount."
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
        amount: groupDigits(String(c.amount)),
      })),
    },
  })
  // keyName 'key': the rows carry their own "id" (the cost line id).
  const lines = useFieldArray({ control, name: 'lines', keyName: 'key' })
  const receipts = new Map((caseDetail.costs ?? []).map((c) => [String(c.id), c]))
  const [zoomed, setZoomed] = useState<{ url: string; code: string | null } | null>(null)

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
        hint="Amounts in VND."
        // Meeting note 18/11: the receipt is what the staff checks first: shown first, large, and opens bigger.
        lead={(i) => {
          const cost = receipts.get(lines.fields[i]?.id ?? '')
          if (!cost?.receiptCode && !cost?.receiptUrl) return null
          return (
            <div className="flex w-40 flex-col gap-1.5">
              {cost.receiptUrl ? (
                <button
                  type="button"
                  onClick={() => setZoomed({ url: cost.receiptUrl!, code: cost.receiptCode })}
                  className="border-border block cursor-zoom-in overflow-hidden rounded-lg border"
                >
                  <img
                    src={cost.receiptUrl}
                    alt={`Receipt for ${cost.name}`}
                    className="h-40 w-40 object-cover"
                  />
                </button>
              ) : (
                <div className="border-border bg-raised text-muted flex h-20 w-40 items-center justify-center rounded-lg border text-xs">
                  No photo
                </div>
              )}
              <span className="text-xs">
                {cost.receiptCode ? `Receipt ${cost.receiptCode}` : 'Receipt photo'}
              </span>
            </div>
          )
        }}
      />
      {save.error && <Alert>{save.error.message}</Alert>}
      {zoomed && (
        <Dialog
          open
          wide
          onClose={() => setZoomed(null)}
          title={zoomed.code ? `Receipt ${zoomed.code}` : 'Receipt'}
        >
          <img
            src={zoomed.url}
            alt="Receipt"
            className="max-h-[70vh] w-full rounded-lg object-contain"
          />
          <DialogActions>
            <Button variant="ghost" onClick={() => setZoomed(null)}>
              Close
            </Button>
          </DialogActions>
        </Dialog>
      )}
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
