import type { HTMLAttributes, ReactNode } from 'react'
import { cn } from '@/shared/utils/cn'

const PADDING = { none: '', default: 'p-5', roomy: 'p-6 sm:p-8' } as const

interface CardProps extends HTMLAttributes<HTMLDivElement> {
  /** "none" when the card is split into sections that pad themselves. */
  padding?: keyof typeof PADDING
}

/**
 * Panel inside the sheet: white, 1px border, 12px corners and a very soft lift (16/11: a hairline shadow under
 * a lighter edge reads as depth without the "floating card" look).
 */
export function Card({ padding = 'default', className, ...rest }: CardProps) {
  return (
    <div
      className={cn(
        'bg-surface border-border rounded-card border shadow-[0_1px_2px_rgb(15_27_45/0.04),0_6px_20px_-12px_rgb(15_27_45/0.12)]',
        PADDING[padding],
        className,
      )}
      {...rest}
    />
  )
}

/** A card's heading row: 16/600 title on the left, an optional link or meta on the right. */
export function CardHeader({ title, aside }: { title: string; aside?: ReactNode }) {
  return (
    <div className="mb-4 flex items-baseline justify-between gap-3">
      <h2 className="text-section font-semibold">{title}</h2>
      {aside && <div className="text-muted text-xs">{aside}</div>}
    </div>
  )
}
