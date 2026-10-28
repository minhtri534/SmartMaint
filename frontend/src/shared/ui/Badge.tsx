import type { ReactNode } from 'react'
import { cn } from '@/shared/utils/cn'

export type BadgeTone = 'neutral' | 'accent' | 'positive' | 'warning' | 'negative'

/** Pale tint with dark ink of the same hue. One status mark per row. */
const TONES: Record<BadgeTone, string> = {
  neutral: 'bg-pill-neutral-bg text-pill-neutral-ink',
  accent: 'bg-pill-accent-bg text-pill-accent-ink',
  positive: 'bg-pill-positive-bg text-pill-positive-ink',
  warning: 'bg-pill-warning-bg text-pill-warning-ink',
  negative: 'bg-pill-negative-bg text-pill-negative-ink',
}

/** Status pill: 22px tall, 12/500, never squeezed. */
export function Badge({ tone = 'neutral', children }: { tone?: BadgeTone; children: ReactNode }) {
  return (
    <span
      className={cn(
        'inline-flex h-[22px] flex-none items-center rounded-full px-2 text-xs font-medium whitespace-nowrap',
        TONES[tone],
      )}
    >
      {children}
    </span>
  )
}
