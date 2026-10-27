import { useId, type ComponentProps } from 'react'
import { cn } from '@/shared/utils/cn'

interface SelectProps extends ComponentProps<'select'> {
  label: string
  error?: string
  hint?: string
  /** Text of the empty first option. Omit to have no empty option. */
  placeholder?: string
}

/** Label + native select + error message. Works with react-hook-form: <Select {...register('x')} />. */
export function Select({
  label,
  error,
  hint,
  placeholder,
  className,
  id,
  children,
  ...rest
}: SelectProps) {
  const generatedId = useId()
  const selectId = id ?? generatedId
  const messageId = `${selectId}-message`

  return (
    <div className={cn('flex flex-col gap-1.5', className)}>
      <label htmlFor={selectId} className="text-sm font-medium">
        {label}
      </label>
      <select
        id={selectId}
        aria-invalid={error ? true : undefined}
        aria-describedby={error || hint ? messageId : undefined}
        className={cn(
          'border-line bg-surface h-9 rounded-md border px-2 text-sm outline-none',
          'focus:border-primary focus:ring-primary/20 focus:ring-2',
          error && 'border-danger focus:border-danger focus:ring-danger/20',
        )}
        {...rest}
      >
        {placeholder !== undefined && <option value="">{placeholder}</option>}
        {children}
      </select>
      {(error || hint) && (
        <p id={messageId} className={cn('text-xs', error ? 'text-danger' : 'text-muted')}>
          {error ?? hint}
        </p>
      )}
    </div>
  )
}
