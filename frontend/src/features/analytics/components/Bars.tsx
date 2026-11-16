import { formatDate } from '@/shared/utils/format'

/*
 * Single-series charts: one accent hue, values in ink beside the marks (never in the series colour), the title names
 * the series so no legend is needed, a native tooltip on every mark, and the numbers are always readable as text.
 */

/** Horizontal bars for a ranked list (service types, groups, hold reasons). */
export function BarList({
  rows,
  unit,
}: {
  /** valueText: how the value reads ("38 h"); the bar length uses value. */
  rows: {
    key: string | number
    label: string
    value: number
    valueText?: string
    detail?: string
  }[]
  unit: string
}) {
  const max = Math.max(1, ...rows.map((r) => r.value))
  return (
    <ul className="flex flex-col gap-2.5">
      {rows.map((r) => (
        <li
          key={r.key}
          className="grid grid-cols-[minmax(0,1fr)_auto] items-center gap-x-3 text-sm"
        >
          <span className="min-w-0 truncate">{r.label}</span>
          <span className="text-right tabular-nums">
            {r.valueText ?? r.value}
            {r.detail && <span className="text-muted text-xs"> · {r.detail}</span>}
          </span>
          <div
            className="bg-rule col-span-2 h-1.5 overflow-hidden rounded-full"
            title={`${r.label}: ${r.value} ${unit}`}
          >
            {/* scaleX: a new period or building moves the bar on the GPU instead of redrawing it */}
            <div
              className="bg-accent ease-snap h-full origin-left rounded-full transition-transform duration-500"
              style={{ transform: `scaleX(${Math.max(0.02, r.value / max)})` }}
            />
          </div>
        </li>
      ))}
    </ul>
  )
}

/** Vertical bars per week, anchored to the baseline with rounded tops; the count sits above each bar. */
export function WeekBars({
  title,
  weeks,
}: {
  title: string
  weeks: { weekStart: string; value: number }[]
}) {
  const max = Math.max(1, ...weeks.map((w) => w.value))
  // A long period (90 days = 13 weeks) keeps the axis readable: about 6 date labels, counted back from the
  // latest week, and only the non-zero counts above the bars (16/11; they were truncated to "1.." before).
  const dense = weeks.length > 8
  const step = Math.ceil(weeks.length / 6)
  const labelled = (i: number) => (weeks.length - 1 - i) % step === 0
  return (
    <figure className="flex min-w-0 flex-col gap-2">
      <figcaption className="text-muted text-[13px]">{title}</figcaption>
      <div className="border-rule flex h-28 items-end gap-1.5 border-b">
        {weeks.map((w) => (
          <div
            key={w.weekStart}
            className="group flex h-full min-w-0 flex-1 flex-col items-center justify-end gap-1"
            title={`Week of ${formatDate(w.weekStart)}: ${w.value}`}
          >
            <span className="text-muted text-[11px] tabular-nums">
              {dense && w.value === 0 ? '' : w.value}
            </span>
            {/* height eases to the new value when the filters change (12 small bars: cheap) */}
            <div
              className="bg-accent ease-snap w-full max-w-8 rounded-t-[4px] transition-[height,opacity] duration-500 group-hover:opacity-80"
              style={{ height: `${(w.value / max) * 80}%`, minHeight: w.value > 0 ? 2 : 0 }}
            />
          </div>
        ))}
      </div>
      <div className="flex gap-1.5">
        {weeks.map((w, i) => (
          <span
            key={w.weekStart}
            className="text-muted flex min-w-0 flex-1 justify-center text-[11px] whitespace-nowrap"
          >
            {labelled(i) &&
              new Date(w.weekStart).toLocaleDateString('en-GB', {
                day: 'numeric',
                month: 'short',
              })}
          </span>
        ))}
      </div>
    </figure>
  )
}
