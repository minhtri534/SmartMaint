import type { ReactNode } from 'react'
import { cn } from '@/shared/utils/cn'

interface FieldProps {
  id: string
  label: string
  error?: string
  hint?: string
  className?: string
  children: ReactNode
}

/** Label above the control (14/500), 8px gap, then the hint or the error under it. */
export function Field({ id, label, error, hint, className, children }: FieldProps) {
  return (
    <div className={cn('flex flex-col gap-2', className)}>
      <label htmlFor={id} className="text-sm font-medium">
        {label}
      </label>
      {children}
      {(error || hint) && (
        <p id={`${id}-message`} className={cn('text-xs', error ? 'text-negative' : 'text-muted')}>
          {error ?? hint}
        </p>
      )}
    </div>
  )
}
