import { animate, motion, useReducedMotion } from 'motion/react'
import { useEffect, useId, useState, type ReactNode } from 'react'
import { Link } from 'react-router'
import { cn } from '@/shared/utils/cn'
import { EASE_SNAP } from '@/shared/utils/motion'
import { HelpTip } from './HelpTip'
import { Icon, type IconName } from './Icon'

/*
 * Dashboard pieces for ui/mercato-style (17/11): KPI cards with a tinted icon chip, a bar + line chart with
 * 3D blue bars, a tick gauge, and the motion that brings a dashboard in (cards rise one after another, figures
 * count up, bars grow, the line draws). Every animation runs once when the page opens and respects reduced motion.
 */

// ---------- motion ----------

/** A card that rises 14px into place, `index` × 60ms after the page opens. */
export function Rise({
  index = 0,
  className,
  children,
}: {
  index?: number
  className?: string
  children: ReactNode
}) {
  const reduce = useReducedMotion()
  return (
    <motion.div
      initial={reduce ? { opacity: 0 } : { opacity: 0, y: 14 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5, ease: EASE_SNAP, delay: index * 0.06 }}
      className={className}
    >
      {children}
    </motion.div>
  )
}

/** A figure that counts up from 0 (numbers only: "86%" counts 0 → 86 and keeps the "%"). */
export function CountUp({ value }: { value: ReactNode }) {
  const reduce = useReducedMotion()
  const text = typeof value === 'number' ? String(value) : typeof value === 'string' ? value : null
  const match = text ? /^(-?\d+(?:\.\d+)?)(.*)$/.exec(text) : null
  const target = match ? Number(match[1]) : null
  const decimals = match?.[1]?.split('.')[1]?.length ?? 0
  const [shown, setShown] = useState(reduce || target === null ? target : 0)

  useEffect(() => {
    if (target === null || reduce) return
    const controls = animate(0, target, {
      duration: 0.9,
      ease: EASE_SNAP,
      onUpdate: (v) => setShown(v),
    })
    return () => controls.stop()
  }, [target, reduce])

  if (match === null || target === null) return <>{value}</>
  return (
    <>
      {(shown ?? target).toFixed(decimals)}
      {match[2]}
    </>
  )
}

// ---------- KPI card ----------

export type ChipHue = 'orange' | 'violet' | 'green' | 'blue' | 'red'

const CHIP: Record<ChipHue, string> = {
  orange: 'bg-chip-orange/15 text-chip-orange',
  violet: 'bg-chip-violet/15 text-chip-violet',
  green: 'bg-chip-green/15 text-chip-green',
  blue: 'bg-chip-blue/15 text-chip-blue',
  red: 'bg-negative/15 text-negative',
}

export interface KpiCardProps {
  label: string
  value: ReactNode
  icon: IconName
  hue?: ChipHue
  /** Change against the previous period; `good` colours it (null = grey). */
  change?: { text: string; good: boolean | null } | null
  /** One grey line when there is no change to show. */
  hint?: string
  /** The list behind the number ("View more"). */
  to?: string
  /** Red figure: late or unacknowledged work above zero. */
  alert?: boolean
  /** What the figure counts and how, behind a "?" (meeting note 18/11). */
  help?: ReactNode
  index?: number
}

export function KpiCard({
  label,
  value,
  icon,
  hue = 'orange',
  change,
  hint,
  to,
  alert,
  help,
  index = 0,
}: KpiCardProps) {
  return (
    <Rise index={index} className="h-full">
      <div className="card-soft rounded-card flex h-full flex-col p-4 sm:p-5">
        <div className="flex items-start justify-between gap-3">
          <span className="flex items-center gap-1.5 text-[13.5px] font-semibold">
            {label}
            {help && (
              <HelpTip label={label} align={index % 2 === 0 ? 'left' : 'right'}>
                {help}
              </HelpTip>
            )}
          </span>
          <span
            className={cn(
              'grid size-9 flex-none place-items-center rounded-xl shadow-[inset_0_1px_0_rgb(255_255_255/0.5)]',
              CHIP[hue],
            )}
          >
            <Icon name={icon} className="size-[18px]" />
          </span>
        </div>
        <div
          className={cn(
            'mt-2 text-[26px] leading-8 font-semibold tracking-[-0.03em] tabular-nums sm:text-[30px] sm:leading-9',
            alert && 'text-negative',
          )}
        >
          <CountUp value={value} />
        </div>
        <div className="mt-1 min-h-4 text-xs tabular-nums">
          {change ? (
            <span
              className={cn(
                'font-medium',
                change.good === null
                  ? 'text-muted'
                  : change.good
                    ? 'text-positive'
                    : 'text-negative',
              )}
            >
              {change.text}
              <span className="text-muted font-normal"> vs previous period</span>
            </span>
          ) : (
            hint && <span className="text-muted">{hint}</span>
          )}
        </div>
        {to && (
          <div className="mt-auto pt-4">
            <Link
              to={to}
              className="border-rule text-muted hover:text-ink group flex items-center justify-between border-t pt-3 text-xs font-medium transition-colors duration-150"
            >
              View more
              <Icon
                name="arrowRight"
                className="size-3.5 transition-transform duration-150 group-hover:translate-x-0.5"
              />
            </Link>
          </div>
        )}
      </div>
    </Rise>
  )
}

// ---------- chart: 3D bars + a line ----------

export interface ChartPoint {
  label: string
  /** Shown in the tooltip, e.g. "Week of 3 Nov". */
  title?: string
  bar: number
  line?: number
}

/** A round top for the y axis that splits into 4 whole steps (0, 2, 4, 6, 8 — never 0, 2, 3, 5, 6). */
function niceMax(n: number) {
  const raw = Math.max(1, n / 4)
  const p = 10 ** Math.floor(Math.log10(raw))
  const step =
    [1, 2, 2.5, 5, 10].map((s) => s * p).find((s) => s >= raw && Number.isInteger(s)) ?? 10 * p
  return step * 4
}

export function PerformanceChart({
  data,
  barName,
  lineName,
  height = 220,
}: {
  data: ChartPoint[]
  barName: string
  lineName?: string
  height?: number
}) {
  const reduce = useReducedMotion()
  const clipId = 'chart-line-' + useId().replace(/[^a-zA-Z0-9_-]/g, '')
  const max = niceMax(Math.max(1, ...data.map((d) => Math.max(d.bar, d.line ?? 0))))
  const ticks = [0, 0.25, 0.5, 0.75, 1].map((t) => Math.round(max * t))
  const hasLine = lineName && data.some((d) => d.line !== undefined)
  const n = Math.max(1, data.length)
  const points = data
    .map((d, i) => `${((i + 0.5) / n) * 100},${100 - ((d.line ?? 0) / max) * 100}`)
    .join(' ')

  return (
    <div className="flex flex-col gap-3">
      <div className="text-muted flex items-center gap-4 text-xs">
        <span className="flex items-center gap-1.5">
          <span className="bar-3d size-2.5 rounded-full" /> {barName}
        </span>
        {hasLine && (
          <span className="flex items-center gap-1.5">
            <span className="bg-line size-2.5 rounded-full" /> {lineName}
          </span>
        )}
      </div>
      <div className="flex gap-3">
        {/* y axis */}
        <div
          className="text-faint flex flex-col-reverse justify-between pb-6 text-right text-[11px] tabular-nums"
          style={{ height }}
        >
          {ticks.map((t) => (
            <span key={t} className="leading-none">
              {t}
            </span>
          ))}
        </div>
        <div className="relative min-w-0 flex-1" style={{ height }}>
          {/* grid */}
          <div className="absolute inset-x-0 top-0 bottom-6 flex flex-col justify-between">
            {ticks.map((t) => (
              <div key={t} className="border-rule border-t border-dashed" />
            ))}
          </div>
          {/* bars */}
          <div className="group/plot absolute inset-x-0 top-0 bottom-6 flex items-end">
            {data.map((d, i) => (
              <div
                key={d.label + i}
                tabIndex={0}
                aria-label={`${d.title ?? d.label}: ${barName} ${d.bar}${hasLine ? `, ${lineName} ${d.line ?? 0}` : ''}`}
                className="group/bar relative flex h-full flex-1 items-end justify-center outline-none"
              >
                <motion.div
                  initial={reduce ? false : { scaleY: 0 }}
                  animate={{ scaleY: 1 }}
                  transition={{ duration: 0.7, ease: EASE_SNAP, delay: 0.15 + i * 0.04 }}
                  className="bar-3d w-[46%] max-w-9 origin-bottom rounded-t-[7px] transition-opacity duration-150 group-hover/bar:opacity-100! group-hover/plot:opacity-50 group-focus-visible/bar:opacity-100!"
                  style={{ height: `${(d.bar / max) * 100}%`, minHeight: d.bar > 0 ? 3 : 0 }}
                />
                <span
                  role="tooltip"
                  className="bg-ink text-surface pointer-events-none absolute top-0 left-1/2 z-10 -translate-x-1/2 -translate-y-[85%] rounded-lg px-2.5 py-1.5 text-[11px] whitespace-nowrap opacity-0 shadow-lg transition-[opacity,transform] duration-150 group-hover/bar:-translate-y-full group-hover/bar:opacity-100 group-focus-visible/bar:-translate-y-full group-focus-visible/bar:opacity-100"
                >
                  <span className="opacity-70">{d.title ?? d.label}</span>
                  <b className="ml-1.5 font-semibold tabular-nums">{d.bar}</b>
                  {hasLine && (
                    <span className="ml-1.5 tabular-nums opacity-80">· {d.line ?? 0}</span>
                  )}
                </span>
              </div>
            ))}
          </div>
          {/* line: revealed left to right by a growing clip (a dash animation would break the
              non-scaling stroke into pieces) */}
          {hasLine && (
            <svg
              viewBox="0 0 100 100"
              preserveAspectRatio="none"
              aria-hidden="true"
              className="pointer-events-none absolute inset-x-0 top-0 bottom-6 h-[calc(100%-24px)] w-full overflow-visible"
            >
              <defs>
                <clipPath id={clipId}>
                  <motion.rect
                    x={-2}
                    y={-10}
                    height={120}
                    initial={{ width: reduce ? 104 : 0 }}
                    animate={{ width: 104 }}
                    transition={{ duration: 1.1, ease: EASE_SNAP, delay: 0.35 }}
                  />
                </clipPath>
              </defs>
              <polyline
                points={points}
                clipPath={`url(#${clipId})`}
                fill="none"
                stroke="var(--color-line)"
                strokeWidth={2.2}
                strokeLinejoin="round"
                strokeLinecap="round"
                vectorEffect="non-scaling-stroke"
              />
            </svg>
          )}
          {/* x labels */}
          <div className="absolute inset-x-0 bottom-0 flex h-5 items-end">
            {data.map((d, i) => (
              <span
                key={d.label + i}
                className="text-faint flex-1 truncate text-center text-[11px] leading-none"
              >
                {n > 8 && i % 2 === 1 ? '' : d.label}
              </span>
            ))}
          </div>
        </div>
      </div>
    </div>
  )
}

// ---------- gauge ----------

/** A half-circle of ticks; the filled ticks go pale → deep blue, the figure in the middle counts up. */
export function Gauge({
  value,
  caption,
  ticks = 34,
}: {
  /** 0..1, or null when there is nothing to measure yet. */
  value: number | null
  caption: string
  ticks?: number
}) {
  const reduce = useReducedMotion()
  const filled = value === null ? 0 : Math.round(Math.max(0, Math.min(1, value)) * ticks)
  const cx = 110
  const cy = 108
  return (
    <div className="flex flex-col items-center">
      <svg viewBox="0 0 220 124" className="w-full max-w-[300px]" aria-hidden="true">
        {Array.from({ length: ticks }, (_, i) => {
          const angle = Math.PI - (i / (ticks - 1)) * Math.PI
          const inner = 70
          const outer = i < filled ? 100 : 94
          const x1 = cx + inner * Math.cos(angle)
          const y1 = cy - inner * Math.sin(angle)
          const x2 = cx + outer * Math.cos(angle)
          const y2 = cy - outer * Math.sin(angle)
          const on = i < filled
          // pale sky blue at the start of the arc, deep blue at the end of the filled part
          const t = filled > 1 ? i / (filled - 1) : 1
          const color = on ? `hsl(${212 + t * 10} 92% ${74 - t * 22}%)` : 'var(--color-rule)'
          return (
            <motion.line
              key={i}
              x1={x1}
              y1={y1}
              x2={x2}
              y2={y2}
              stroke={color}
              strokeWidth={5}
              strokeLinecap="round"
              initial={reduce || !on ? false : { opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ duration: 0.25, delay: 0.2 + i * 0.025 }}
            />
          )
        })}
      </svg>
      <div className="-mt-14 text-center">
        <div className="text-[34px] leading-10 font-semibold tracking-[-0.03em] tabular-nums">
          {value === null ? (
            '—'
          ) : (
            <CountUp
              value={`${Number.isInteger(Math.round(value * 1000) / 10) ? Math.round(value * 100) : (value * 100).toFixed(1)}%`}
            />
          )}
        </div>
        <div className="text-muted text-[13px]">{caption}</div>
      </div>
    </div>
  )
}

/** Two small figures under a gauge ("This week 23.5k ↑ 4%"). */
export function MiniStat({
  label,
  value,
  note,
}: {
  label: string
  value: ReactNode
  note?: ReactNode
}) {
  return (
    <div className="well rounded-xl px-3.5 py-2.5">
      <div className="text-muted text-[11px]">{label}</div>
      <div className="mt-0.5 flex items-baseline gap-2">
        <span className="text-[17px] font-semibold tabular-nums">{value}</span>
        {note && <span className="text-xs">{note}</span>}
      </div>
    </div>
  )
}

/** A card title row in the dashboard style: title (and what it shows) left, a round control or link right. */
export function PanelHeader({
  title,
  subtitle,
  help,
  aside,
}: {
  title: string
  /** One grey line under the title, e.g. the period the numbers cover. */
  subtitle?: ReactNode
  /** What the panel shows, behind a "?" (meeting note 18/11). */
  help?: ReactNode
  aside?: ReactNode
}) {
  return (
    <div className="mb-4 flex items-start justify-between gap-3">
      <div className="min-w-0">
        <h2 className="text-section flex items-center gap-1.5 font-semibold">
          {title}
          {help && (
            <HelpTip label={title} align="left">
              {help}
            </HelpTip>
          )}
        </h2>
        {subtitle && <p className="text-muted mt-0.5 text-xs">{subtitle}</p>}
      </div>
      {aside}
    </div>
  )
}
