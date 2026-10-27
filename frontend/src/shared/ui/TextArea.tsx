import { useId, type ComponentProps } from 'react'
import { cn } from '@/shared/utils/cn'

interface TextAreaProps extends ComponentProps<'textarea'> {
  label: string
  error?: string
  hint?: string
}

/** Label + textarea + error message. Works with react-hook-form: <TextArea {...register('x')} />. */
export function TextArea({ label, error, hint, className, id, rows = 4, ...rest }: TextAreaProps) {
  const generatedId = useId()
  const inputId = id ?? generatedId
  const messageId = `${inputId}-message`

  return (
    <div className={cn('flex flex-col gap-1.5', className)}>
      <label htmlFor={inputId} className="text-sm font-medium">
        {label}
      </label>
      <textarea
        id={inputId}
        rows={rows}
        aria-invalid={error ? true : undefined}
        aria-describedby={error || hint ? messageId : undefined}
        className={cn(
          'border-line bg-surface rounded-md border px-3 py-2 text-sm outline-none',
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
