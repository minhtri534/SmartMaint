import type { ComponentProps } from 'react'
import { cn } from '@/shared/utils/cn'

interface FilterSelectProps extends Omit<ComponentProps<'select'>, 'value'> {
  /** Shown inside the control before the value ("Status  All statuses"); also its accessible name. */
  label: string
  value: string
  /** The value that means "no filter" — anything else marks the control as an active filter. */
  emptyValue?: string
  /** Never tinted (e.g. a sort order, which narrows nothing). */
  neutral?: boolean
}

/**
 * Compact 32px select for a filter row: the label sits inside the control, so a row of filters reads as
 * one line. An active filter takes the accent tint (U3: the selected filter is data). Use Select for forms.
 */
export function FilterSelect({
  label,
  value,
  emptyValue = '',
  neutral = false,
  className,
  children,
  ...rest
}: FilterSelectProps) {
  const active = !neutral && value !== emptyValue
  return (
    <label
      className={cn(
        'rounded-control relative inline-flex h-8 flex-none items-center border pl-3 text-sm whitespace-nowrap',
        'focus-within:ring-accent/20 transition-colors duration-150 focus-within:ring-2',
        active
          ? 'border-accent bg-accent-soft text-ink'
          : 'border-control bg-surface hover:bg-raised',
        className,
      )}
    >
      <span className="text-muted pointer-events-none">{label}</span>
      <select
        aria-label={label}
        value={value}
        // field-sizing: as wide as the chosen option, not the longest one
        className="control-select [field-sizing:content] h-full cursor-pointer bg-transparent pr-9 pl-1.5 font-medium outline-none"
        {...rest}
      >
        {children}
      </select>
    </label>
  )
}
