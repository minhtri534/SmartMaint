import type { ComponentProps, ReactNode } from 'react'
import type { UseFormRegisterReturn } from 'react-hook-form'
import { Button, Icon, TextArea, TextField } from '@/shared/ui'
import { groupDigits } from '../amount'

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
  /** Shown first in a line, e.g. its receipt photo (meeting note 18/11: the most important part). */
  lead?: (index: number) => ReactNode
}

/**
 * Cost lines (ERD: Costs): title, description and amount in VND (meeting 07/11). Meeting note 18/11: each line is its
 * own box with real buttons, the amount groups its digits, the description grows with the text, and the receipt
 * comes first. Used by the staff's "Enter costs".
 */
export function CostLinesField({ rows, field, errors, onAdd, onRemove, hint, lead }: Props) {
  return (
    <fieldset className="flex flex-col gap-3">
      <legend className="mb-1 text-sm font-medium">Costs</legend>
      <p className="text-muted -mt-1 text-xs">{hint}</p>
      {rows.length === 0 && <p className="text-muted text-sm">No cost line yet.</p>}
      {rows.map((row, i) => {
        const receipt = lead?.(i)
        return (
          <div
            key={row.key}
            className="border-border rounded-card grid gap-4 border p-4 sm:grid-cols-[auto_minmax(0,1fr)]"
          >
            {receipt && <div className="sm:row-span-2">{receipt}</div>}
            <div className="flex min-w-0 flex-col gap-3">
              <div className="flex items-center justify-between gap-3">
                <span className="text-sm font-medium">Line {i + 1}</span>
                <Button variant="secondary" onClick={() => onRemove(i)}>
                  <Icon name="close" className="size-4" />
                  Remove
                </Button>
              </div>
              <div className="grid gap-3 sm:grid-cols-[minmax(0,1fr)_180px]">
                <TextField label="Title" error={errors?.[i]?.name?.message} {...field(i, 'name')} />
                <AmountField
                  label="Amount (VND)"
                  error={errors?.[i]?.amount?.message}
                  registration={field(i, 'amount')}
                />
              </div>
              <TextArea
                label="Description"
                placeholder="Optional: what it was for, the receipt number…"
                rows={2}
                // grows a line at a time as the text gets longer
                className="[&_textarea]:field-sizing-content [&_textarea]:min-h-16"
                error={errors?.[i]?.description?.message}
                {...field(i, 'description')}
              />
            </div>
          </div>
        )
      })}
      <div>
        <Button variant="secondary" onClick={onAdd}>
          <Icon name="plus" className="size-4" />
          Add a cost line
        </Button>
      </div>
    </fieldset>
  )
}

/** A VND amount typed with a space every three digits (meeting note 18/11): "1 500 000". */
export function AmountField({
  registration,
  ...props
}: Omit<ComponentProps<typeof TextField>, 'onChange'> & { registration: UseFormRegisterReturn }) {
  const { onChange, ...rest } = registration
  return (
    <TextField
      inputMode="numeric"
      {...props}
      {...rest}
      onChange={(e) => {
        e.target.value = groupDigits(e.target.value)
        void onChange(e)
      }}
    />
  )
}
