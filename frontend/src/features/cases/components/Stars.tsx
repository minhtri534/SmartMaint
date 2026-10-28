import { cn } from '@/shared/utils/cn'

const STAR = 'M10 1.8l2.5 5.2 5.7.8-4.1 4 1 5.6L10 14.7l-5.1 2.7 1-5.6-4.1-4 5.7-.8z'

function Star({ on, className }: { on: boolean; className?: string }) {
  return (
    <svg viewBox="0 0 20 20" aria-hidden="true" className={cn('size-5 flex-none', className)}>
      <path d={STAR} className={on ? 'fill-ink' : 'fill-rule stroke-control'} strokeWidth="1" />
    </svg>
  )
}

/** Read-only rating: filled stars in ink, the rest as outlines. */
export function StarsView({ stars }: { stars: number }) {
  return (
    <span className="flex gap-0.5" role="img" aria-label={`${stars} of 5 stars`}>
      {[1, 2, 3, 4, 5].map((n) => (
        <Star key={n} on={n <= stars} />
      ))}
    </span>
  )
}

/** Rating input (DLG-14): five radio buttons. */
export function StarsInput({ value, onChange }: { value: number; onChange: (n: number) => void }) {
  return (
    <div className="flex gap-1" role="radiogroup" aria-label="Stars">
      {[1, 2, 3, 4, 5].map((n) => (
        <button
          key={n}
          type="button"
          role="radio"
          aria-checked={value === n}
          aria-label={`${n} star${n > 1 ? 's' : ''}`}
          onClick={() => onChange(n)}
          className="rounded-control p-0.5 transition-transform active:scale-95"
        >
          <Star on={n <= value} className="size-7" />
        </button>
      ))}
    </div>
  )
}
