import type { UseFormRegisterReturn } from 'react-hook-form'
import { Button, TextField } from '@/shared/ui'

interface Props {
  /** The rows of useFieldArray. */
  rows: { id: string }[]
  /** register(`parts.${i}.name`) / register(`parts.${i}.amount`) of the calling form. */
  field: (index: number, key: 'name' | 'amount') => UseFormRegisterReturn
  /** formState.errors.<array> of the calling form (indexed by row). */
  errors?: {
    [row: number]: { name?: { message?: string }; amount?: { message?: string } } | undefined
  }
  onAdd: () => void
  onRemove: (index: number) => void
  hint: string
}

/** Cost lines (ERD: Costs): one row per part, name + amount in VND. Used by "report completed" and "correct costs". */
export function CostLinesField({ rows, field, errors, onAdd, onRemove, hint }: Props) {
  return (
    <fieldset className="flex flex-col gap-2">
      <legend className="mb-1 text-sm font-medium">Costs</legend>
      <p className="text-muted -mt-1 text-xs">{hint}</p>
      {rows.map((row, i) => (
        <div key={row.id} className="grid grid-cols-[minmax(0,1fr)_140px_auto] items-start gap-2">
          <TextField
            label={`Item ${i + 1}`}
            error={errors?.[i]?.name?.message}
            {...field(i, 'name')}
          />
          <TextField
            label="Amount (VND)"
            inputMode="numeric"
            error={errors?.[i]?.amount?.message}
            {...field(i, 'amount')}
          />
          <Button variant="ghost" className="mt-6" onClick={() => onRemove(i)}>
            Remove
          </Button>
        </div>
      ))}
      <div>
        <Button variant="ghost" className="-ml-3" onClick={onAdd}>
          Add a cost line
        </Button>
      </div>
    </fieldset>
  )
}
