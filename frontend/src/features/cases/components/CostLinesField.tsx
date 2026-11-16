import type { ReactNode } from 'react'
import type { UseFormRegisterReturn } from 'react-hook-form'
import { Button, TextField } from '@/shared/ui'

type Key = 'name' | 'description' | 'amount'

interface Props {
  /** The rows of useFieldArray (created with keyName 'key'). */
  rows: { key: string }[]
  /** register(`parts.${i}.name`) / ...description / ...amount of the calling form. */
  field: (index: number, key: Key) => UseFormRegisterReturn
  /** formState.errors.<array> of the calling form (indexed by row). */
  errors?: {
    [row: number]: Partial<Record<Key, { message?: string }>> | undefined
  }
  onAdd: () => void
  onRemove: (index: number) => void
  hint: string
  /** Shown under a row, e.g. its receipt. */
  extra?: (index: number) => ReactNode
}

/**
 * Cost lines (ERD: Costs): title, description (e.g. the receipt number) and amount in VND (meeting 07/11).
 * Used by the staff's "Costs".
 */
export function CostLinesField({ rows, field, errors, onAdd, onRemove, hint, extra }: Props) {
  return (
    <fieldset className="flex flex-col gap-2">
      <legend className="mb-1 text-sm font-medium">Costs</legend>
      <p className="text-muted -mt-1 text-xs">{hint}</p>
      {rows.map((row, i) => (
        <div
          key={row.key}
          className="border-rule grid items-start gap-2 border-b pb-3 last:border-0 sm:grid-cols-[minmax(0,1fr)_minmax(0,1fr)_140px_auto] sm:border-0 sm:pb-0"
        >
          <TextField
            label={`Item ${i + 1}`}
            error={errors?.[i]?.name?.message}
            {...field(i, 'name')}
          />
          <TextField
            label="Description"
            placeholder="Optional, e.g. receipt R-102"
            error={errors?.[i]?.description?.message}
            {...field(i, 'description')}
          />
          <TextField
            label="Amount (VND)"
            inputMode="numeric"
            error={errors?.[i]?.amount?.message}
            {...field(i, 'amount')}
          />
          <Button
            variant="ghost"
            className="justify-self-start sm:mt-6"
            onClick={() => onRemove(i)}
          >
            Remove
          </Button>
          {extra?.(i)}
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
