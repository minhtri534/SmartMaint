import { cn } from '@/shared/utils/cn'

/** Shared look of every text input, select and textarea: white, 1px control border, 6px corners. */
export function controlClass(error?: string) {
  return cn(
    'w-full rounded-control border bg-surface px-3 text-sm text-ink outline-none placeholder:text-faint',
    'transition-colors duration-150 focus:border-accent focus:ring-2 focus:ring-accent/20',
    'disabled:bg-raised disabled:text-muted',
    error ? 'border-negative' : 'border-control',
  )
}
