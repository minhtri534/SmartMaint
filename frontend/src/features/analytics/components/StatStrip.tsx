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
  /** 'alert': the number is something late or unacknowledged and is above zero — the one red mark. */
  tone?: 'alert'
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
        'border-border bg-border rounded-card grid gap-px overflow-hidden border',
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
                'text-section mt-1 font-semibold tabular-nums',
                item.tone === 'alert' && 'text-negative',
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
        const cell = 'bg-surface block min-w-0 px-4 py-3 max-md:last:odd:col-span-2'
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
