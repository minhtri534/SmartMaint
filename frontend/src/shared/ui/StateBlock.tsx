import { Button } from './Button'

/**
 * The three states every data block needs (U9): loading = grey bars of the real size (no shimmer),
 * error = one sentence + Retry inside the block, empty = dashed frame + one sentence.
 */
export function LoadingBlock({ rows = 3 }: { rows?: number }) {
  return (
    <div className="flex flex-col gap-2" aria-busy="true" aria-label="Loading">
      {Array.from({ length: rows }, (_, i) => (
        <div key={i} className="bg-raised h-10 rounded-lg" />
      ))}
    </div>
  )
}

export function ErrorBlock({
  what,
  message,
  onRetry,
}: {
  what: string
  message: string
  onRetry: () => void
}) {
  return (
    <div className="border-border rounded-card flex items-center justify-between gap-3 border px-4 py-3 text-sm">
      <span>
        Couldn't load {what}. {message}
      </span>
      <Button variant="ghost" onClick={onRetry}>
        Retry
      </Button>
    </div>
  )
}

export function EmptyBlock({ children }: { children: string }) {
  return (
    <div className="border-control text-muted rounded-card flex min-h-10 items-center justify-center border border-dashed px-4 py-2.5 text-sm">
      {children}
    </div>
  )
}
