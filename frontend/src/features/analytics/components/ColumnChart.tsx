import { motion, useReducedMotion } from 'motion/react'
import { EmptyBlock } from '@/shared/ui'
import { EASE_SNAP } from '@/shared/utils/motion'

export interface Column {
  key: string
  /** Under the column, e.g. "3 Nov". */
  label: string
  /** In the tooltip, e.g. "Week of 3 Nov". */
  title: string
  value: number | null
  valueText: string
}

/** A round top for the axis that splits into 4 whole steps. */
function niceMax(n: number) {
  const raw = Math.max(1, n / 4)
  const p = 10 ** Math.floor(Math.log10(raw))
  // 2.5 only where it stays whole (25, 250…): a count axis never reads "7.5"
  const step =
    [1, 2, 2.5, 5, 10]
      .map((s) => s * p)
      .filter(Number.isInteger)
      .find((s) => s >= raw) ?? 10 * p
  return step * 4
}

/**
 * One series over time (the Charts tab): columns of one hue from one baseline, 4px round tops, a recessive grid with
 * four steps, and the figure of a column in a tooltip on hover or focus. An empty period (nothing to average) has no
 * column and reads "—".
 */
export function ColumnChart({
  columns,
  axis,
}: {
  columns: Column[]
  /** How an axis value reads (e.g. "40%", "2 h"). */
  axis: (value: number) => string
}) {
  const reduce = useReducedMotion()
  if (columns.length === 0) return <EmptyBlock>Nothing to show in this period.</EmptyBlock>
  const top = niceMax(Math.max(0, ...columns.map((c) => c.value ?? 0)))
  // A label every few columns so they never collide.
  const every = Math.ceil(columns.length / 8)

  return (
    <div className="flex gap-3">
      <div className="text-muted flex h-[220px] flex-col justify-between pb-6 text-right text-[11px] tabular-nums">
        {[4, 3, 2, 1, 0].map((i) => (
          <span key={i} className="leading-none">
            {axis((top / 4) * i)}
          </span>
        ))}
      </div>
      <div className="relative min-w-0 flex-1">
        <div className="pointer-events-none absolute inset-x-0 top-0 h-[196px]">
          {[0, 1, 2, 3].map((i) => (
            <div
              key={i}
              className="border-rule absolute inset-x-0 border-t border-dashed"
              style={{ top: `${(i / 4) * 100}%` }}
            />
          ))}
          <div className="border-control absolute inset-x-0 bottom-0 border-t" />
        </div>
        <div className="relative flex h-[220px] items-stretch">
          {columns.map((c, i) => (
            <div
              key={c.key}
              tabIndex={0}
              aria-label={`${c.title}: ${c.valueText}`}
              className="group/c relative flex min-w-0 flex-1 flex-col items-center outline-none"
            >
              <div className="relative flex h-[196px] w-full items-end justify-center">
                {c.value !== null && c.value > 0 && (
                  <motion.div
                    className="bg-accent w-[min(24px,60%)] origin-bottom rounded-t-[4px]"
                    style={{ height: `${(c.value / top) * 100}%` }}
                    initial={reduce ? false : { scaleY: 0 }}
                    animate={{ scaleY: 1 }}
                    transition={{ duration: 0.5, ease: EASE_SNAP, delay: Math.min(i, 20) * 0.02 }}
                  />
                )}
                <span className="bg-ink text-surface pointer-events-none absolute bottom-full left-1/2 z-10 mb-1 -translate-x-1/2 rounded-lg px-2.5 py-1.5 text-[11px] whitespace-nowrap opacity-0 shadow-lg transition-opacity group-hover/c:opacity-100 group-focus-visible/c:opacity-100">
                  {c.title} · <b>{c.valueText}</b>
                </span>
              </div>
              <span className="text-muted mt-2 h-4 text-[11px] whitespace-nowrap">
                {i % every === 0 ? c.label : ''}
              </span>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}
