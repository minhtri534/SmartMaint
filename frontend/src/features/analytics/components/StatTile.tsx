import type { ReactNode } from 'react'
import { Link } from 'react-router'
import { cn } from '@/shared/utils/cn'

interface StatTileProps {
  label: string
  value: ReactNode
  /** Change against the previous period, in words; `good` colours it (null = neutral). */
  change?: { text: string; good: boolean | null } | null
  /** One grey line under the value (definition or count). */
  hint?: string
  /** The list behind the number (MGR-01: "click a tile opens the list with the filter applied"). */
  to?: string
}

/** A KPI tile (MGR-01, ANA-01, home pages): label, large tabular value, change in words (never colour only). */
export function StatTile({ label, value, change, hint, to }: StatTileProps) {
  const body = (
    <>
      <div className="text-muted truncate text-[13px]">{label}</div>
      <div className="text-section mt-1 font-semibold tabular-nums">{value}</div>
      {change && (
        <div
          className={cn(
            'mt-0.5 text-xs tabular-nums',
            change.good === null ? 'text-muted' : change.good ? 'text-positive' : 'text-negative',
          )}
        >
          {change.text}
        </div>
      )}
      {hint && <div className="text-muted mt-0.5 truncate text-xs">{hint}</div>}
    </>
  )
  const box = 'border-border bg-surface rounded-card block min-w-0 border px-4 py-3'
  return to ? (
    <Link to={to} className={cn(box, 'hover:border-control transition-colors')}>
      {body}
    </Link>
  ) : (
    <div className={box}>{body}</div>
  )
}
