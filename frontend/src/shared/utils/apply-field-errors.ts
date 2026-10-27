import type { FieldValues, Path, UseFormSetError } from 'react-hook-form'
import type { ApiError } from '@/lib/api-error'

/**
 * Copies server-side validation errors (400 "validation.failed") onto react-hook-form fields.
 * Returns true when at least one field error was applied, so the caller can skip the generic alert.
 */
export function applyFieldErrors<T extends FieldValues>(
  error: ApiError,
  setError: UseFormSetError<T>,
): boolean {
  const entries = Object.entries(error.fieldErrors)
  for (const [field, messages] of entries) {
    setError(field as Path<T>, { type: 'server', message: messages[0] })
  }
  return entries.length > 0
}
