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
        // colours cross-fade when a case changes status in place
        'inline-flex h-[22px] flex-none items-center gap-1.5 rounded-full pr-2.5 pl-2 text-xs font-medium whitespace-nowrap transition-colors duration-200',
        // a status dot in the pill's own ink, so the state reads before the word (16/11)
        'before:size-1.5 before:flex-none before:rounded-full before:bg-current before:opacity-80',
        TONES[tone],
      )}
    >
      {children}
    </span>
  )
}
