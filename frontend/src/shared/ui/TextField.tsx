import { useId, type ComponentProps } from 'react'
import { cn } from '@/shared/utils/cn'

interface TextFieldProps extends ComponentProps<'input'> {
  label: string
  error?: string
  hint?: string
}

/** Label + input + error message. Works with react-hook-form: <TextField {...register('email')} />. */
export function TextField({ label, error, hint, className, id, ...rest }: TextFieldProps) {
  const generatedId = useId()
  const inputId = id ?? generatedId
  const messageId = `${inputId}-message`

  return (
    <div className={cn('flex flex-col gap-1.5', className)}>
      <label htmlFor={inputId} className="text-sm font-medium">
        {label}
      </label>
      <input
        id={inputId}
        aria-invalid={error ? true : undefined}
        aria-describedby={error || hint ? messageId : undefined}
        className={cn(
          'border-line bg-surface h-9 rounded-md border px-3 text-sm outline-none',
          'focus:border-primary focus:ring-primary/20 focus:ring-2',
          error && 'border-danger focus:border-danger focus:ring-danger/20',
        )}
        {...rest}
      />
      {(error || hint) && (
        <p id={messageId} className={cn('text-xs', error ? 'text-danger' : 'text-muted')}>
          {error ?? hint}
        </p>
      )}
    </div>
  )
}
