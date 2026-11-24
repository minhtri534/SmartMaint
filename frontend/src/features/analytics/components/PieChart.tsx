import { useId, useState } from 'react'
import { cn } from '@/shared/utils/cn'

export interface PieSlice {
  key: string | number
  label: string
  value: number
  /** How the value reads ("38 h"); the slice size uses value. */
  valueText?: string
}

/** Six hues in a fixed order (validated for colour blindness, both themes); a seventh part folds into "Other". */
const MAX_SLICES = 6
const TAU = Math.PI * 2

function point(r: number, angle: number) {
  // 0 = 12 o'clock, clockwise
  return `${60 + r * Math.sin(angle)} ${60 - r * Math.cos(angle)}`
}

function arc(from: number, to: number) {
  const outer = 54
  const inner = 34
  const large = to - from > Math.PI ? 1 : 0
  return [
    `M ${point(outer, from)}`,
    `A ${outer} ${outer} 0 ${large} 1 ${point(outer, to)}`,
    `L ${point(inner, to)}`,
    `A ${inner} ${inner} 0 ${large} 0 ${point(inner, from)}`,
    'Z',
  ].join(' ')
}

/**
 * Meeting note 18/11: a pie (drawn as a ring) for "how one whole splits". The list beside it names every part with
 * its figure and share, so colour never carries the meaning alone; hovering a part or its row brings it forward.
 */
export function PieChart({
  slices,
  unit,
  totalText,
}: {
  slices: PieSlice[]
  /** "cases", "hours": read after the total in the middle. */
  unit: string
  /** How the total reads; default the sum. */
  totalText?: string
}) {
  const [active, setActive] = useState<number | null>(null)
  const id = useId()
  const sorted = [...slices].filter((s) => s.value > 0).sort((a, b) => b.value - a.value)
  const shown =
    sorted.length > MAX_SLICES
      ? [
          ...sorted.slice(0, MAX_SLICES - 1),
          {
            key: 'other',
            label: 'Other',
            value: sorted.slice(MAX_SLICES - 1).reduce((n, s) => n + s.value, 0),
          },
        ]
      : sorted
  const total = shown.reduce((n, s) => n + s.value, 0)

  // Each part starts where the parts before it end; a single part would be a full circle, which one arc command
  // cannot draw, so it stops just short.
  const startOf = (i: number) => (shown.slice(0, i).reduce((n, s) => n + s.value, 0) / total) * TAU
  const arcs = shown.map((s, i) => ({
    slice: s,
    path: arc(startOf(i), Math.min(startOf(i) + TAU - 0.0001, startOf(i + 1))),
  }))
  const current = active === null ? null : shown[active]
  const share = (v: number) => `${Math.round((v / total) * 100)}%`

  return (
    <div className="grid items-center gap-5 sm:grid-cols-[160px_minmax(0,1fr)]">
      <div className="relative mx-auto size-40">
        <svg viewBox="0 0 120 120" className="size-full" role="img" aria-labelledby={`${id}-sum`}>
          {arcs.map(({ slice, path }, i) => (
            <path
              key={slice.key}
              d={path}
              fill={`var(--color-chart-${i + 1})`}
              // a 2px surface gap between parts
              stroke="var(--color-surface)"
              strokeWidth="2"
              strokeLinejoin="round"
              className={cn(
                'transition-opacity duration-150',
                active !== null && active !== i && 'opacity-35',
              )}
              onMouseEnter={() => setActive(i)}
              onMouseLeave={() => setActive(null)}
            />
          ))}
        </svg>
        <div
          id={`${id}-sum`}
          className="pointer-events-none absolute inset-0 flex flex-col items-center justify-center px-8 text-center"
        >
          <span className="text-lg leading-6 font-semibold tabular-nums">
            {current ? (current.valueText ?? current.value) : (totalText ?? total)}
          </span>
          <span className="text-muted line-clamp-2 text-[11px] leading-4">
            {current ? `${current.label} · ${share(current.value)}` : unit}
          </span>
        </div>
      </div>
      <ul className="flex min-w-0 flex-col gap-1.5 text-sm">
        {shown.map((s, i) => (
          <li
            key={s.key}
            onMouseEnter={() => setActive(i)}
            onMouseLeave={() => setActive(null)}
            className={cn(
              'rounded-control flex items-center gap-2.5 px-2 py-1 transition-colors duration-150',
              active === i && 'bg-raised',
            )}
          >
            <span
              aria-hidden="true"
              className="size-2.5 flex-none rounded-full"
              style={{ background: `var(--color-chart-${i + 1})` }}
            />
            <span className="min-w-0 flex-1 truncate">{s.label}</span>
            <span className="tabular-nums">{s.valueText ?? s.value}</span>
            <span className="text-muted w-10 text-right text-xs tabular-nums">
              {share(s.value)}
            </span>
          </li>
        ))}
      </ul>
    </div>
  )
}
