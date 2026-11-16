import type { ReactNode } from 'react'
import { Link } from 'react-router'
import { cn } from '@/shared/utils/cn'

export interface StatItem {
  label: string
  value: ReactNode
  /** One grey line under the value. */
  hint?: string
  /** The list behind the number. */
  to?: string
  /**
   * 'alert': something late or unacknowledged, above zero — the one orange mark.
   * 'action': work waiting for this person, above zero — blueprint blue (direction A, 16/11).
   */
  tone?: 'alert' | 'action'
  /** Change against the previous period, in words; `good` colours it (null = grey). */
  change?: { text: string; good: boolean | null } | null
}

/**
 * Home numbers as ONE container split by 1px lines (taste app-screens §5: KPIs are type, not tiles).
 * Built with a 1px gap over the border colour so the lines hold whatever the column count.
 * On a phone it is 2 columns; an odd last cell takes the full row.
 */
export function StatStrip({ items, className }: { items: StatItem[]; className?: string }) {
  return (
    <div
      className={cn(
        'border-border bg-border rounded-card grid gap-px overflow-hidden border shadow-[0_1px_2px_rgb(15_27_45/0.04),0_6px_20px_-12px_rgb(15_27_45/0.12)]',
        'grid-cols-2',
        className,
      )}
    >
      {items.map((item) => {
        const body = (
          <>
            <div className="text-muted truncate text-[13px]">{item.label}</div>
            <div
              className={cn(
                // large and light: the figure is the point, not its weight (16/11, after Leader's reference)
                'mt-1.5 text-[34px] leading-10 font-normal tracking-[-0.035em] tabular-nums',
                item.tone === 'alert' && 'text-negative',
                item.tone === 'action' && 'text-accent',
              )}
            >
              {item.value}
            </div>
            {item.change && (
              <div
                className={cn(
                  'mt-0.5 text-xs tabular-nums',
                  item.change.good === null
                    ? 'text-muted'
                    : item.change.good
                      ? 'text-positive'
                      : 'text-negative',
                )}
              >
                {item.change.text}
              </div>
            )}
            {item.hint && <div className="text-muted mt-0.5 truncate text-xs">{item.hint}</div>}
          </>
        )
        const cell = 'bg-surface block min-w-0 px-5 py-4 max-md:last:odd:col-span-2'
        return item.to ? (
          <Link
            key={item.label}
            to={item.to}
            className={cn(cell, 'hover:bg-raised transition-colors duration-150')}
          >
            {body}
          </Link>
        ) : (
          <div key={item.label} className={cell}>
            {body}
          </div>
        )
      })}
    </div>
  )
}
