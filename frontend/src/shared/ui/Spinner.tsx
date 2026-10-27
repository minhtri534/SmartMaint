import { cn } from '@/shared/utils/cn'

export function Spinner({ className }: { className?: string }) {
  return (
    <span
      role="status"
      aria-label="Loading"
      className={cn(
        'inline-block size-5 animate-spin rounded-full border-2 border-current border-t-transparent',
        className,
      )}
    />
  )
}

export function FullPageSpinner() {
  return (
    <div className="text-muted flex h-full items-center justify-center">
      <Spinner className="size-8" />
    </div>
  )
}
