import { motion, useReducedMotion } from 'motion/react'
import { useId } from 'react'
import { paths } from '@/app/paths'
import type { Overview } from '@/features/analytics'
import { EmptyBlock, Icon, type IconName } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { formatDuration } from '@/shared/utils/format'
import { EASE_SNAP } from '@/shared/utils/motion'
import { Link } from 'react-router'

/*
 * Manager home pieces (17/11): a manager reads the whole picture, read only (BR-12). Throughput shows whether the
 * team keeps up (cases in vs cases done, 12 weeks, as two areas); the league ranks the buildings; the emergency
 * strip shows how fast the team reacts at each F4 milestone.
 */

/** Catmull-Rom through the points, as a smooth SVG path (x, y in 0..100). */
function smooth(points: [number, number][]) {
  if (points.length < 2) return ''
  let d = `M ${points[0]![0]} ${points[0]![1]}`
  for (let i = 0; i < points.length - 1; i++) {
    const p0 = points[i - 1] ?? points[i]!
    const p1 = points[i]!
    const p2 = points[i + 1]!
    const p3 = points[i + 2] ?? p2
    // control points never dip below the baseline (y 96): a week with 0 must not draw a negative swing
    const c1 = [p1[0] + (p2[0] - p0[0]) / 6, Math.min(96, p1[1] + (p2[1] - p0[1]) / 6)]
    const c2 = [p2[0] - (p3[0] - p1[0]) / 6, Math.min(96, p2[1] - (p3[1] - p1[1]) / 6)]
    d += ` C ${c1[0]} ${c1[1]}, ${c2[0]} ${c2[1]}, ${p2[0]} ${p2[1]}`
  }
  return d
}

export function ThroughputArea({ weekly }: { weekly: Overview['weekly'] }) {
  const reduce = useReducedMotion()
  const id = 'tp' + useId().replace(/[^a-zA-Z0-9_-]/g, '')
  if (weekly.length === 0) return <EmptyBlock>No weeks to show yet.</EmptyBlock>

  const max = Math.max(1, ...weekly.flatMap((w) => [w.reported, w.completed]))
  const n = weekly.length
  const x = (i: number) => (n === 1 ? 50 : (i / (n - 1)) * 100)
  const y = (v: number) => 96 - (v / max) * 86
  const inPts = weekly.map((w, i) => [x(i), y(w.reported)] as [number, number])
  const outPts = weekly.map((w, i) => [x(i), y(w.completed)] as [number, number])
  const area = (pts: [number, number][]) => `${smooth(pts)} L 100 100 L 0 100 Z`
  const totalIn = weekly.reduce((s, w) => s + w.reported, 0)
  const totalOut = weekly.reduce((s, w) => s + w.completed, 0)
  const gap = totalIn - totalOut

  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-wrap items-end gap-6">
        <div>
          <div className="text-muted text-xs">Came in</div>
          <div className="text-[26px] leading-8 font-semibold tabular-nums">{totalIn}</div>
        </div>
        <div>
          <div className="text-muted text-xs">Finished</div>
          <div className="text-[26px] leading-8 font-semibold tabular-nums">{totalOut}</div>
        </div>
        <div
          className={cn(
            'ml-auto rounded-full px-3 py-1 text-xs font-medium',
            gap > 0
              ? 'bg-pill-warning-bg text-pill-warning-ink'
              : 'bg-pill-positive-bg text-pill-positive-ink',
          )}
        >
          {gap > 0
            ? `Backlog grew by ${gap} in ${n} weeks`
            : gap < 0
              ? `Backlog shrank by ${-gap} in ${n} weeks`
              : 'Keeping up exactly'}
        </div>
      </div>
      <div className="relative h-[200px]">
        <svg
          viewBox="0 0 100 100"
          preserveAspectRatio="none"
          className="absolute inset-0 size-full overflow-visible"
          aria-hidden="true"
        >
          <defs>
            <clipPath id={id}>
              <motion.rect
                x={-2}
                y={-10}
                height={120}
                initial={{ width: reduce ? 104 : 0 }}
                animate={{ width: 104 }}
                transition={{ duration: 1.2, ease: EASE_SNAP, delay: 0.2 }}
              />
            </clipPath>
          </defs>
          {[0.25, 0.5, 0.75].map((t) => (
            <line
              key={t}
              x1="0"
              x2="100"
              y1={96 - t * 86}
              y2={96 - t * 86}
              stroke="var(--color-rule)"
              strokeDasharray="1.5 1.5"
              vectorEffect="non-scaling-stroke"
            />
          ))}
          <g clipPath={`url(#${id})`}>
            <path d={area(inPts)} fill="var(--color-accent)" fillOpacity="0.14" />
            <path d={area(outPts)} fill="var(--color-line)" fillOpacity="0.16" />
            <path
              d={smooth(inPts)}
              fill="none"
              stroke="var(--color-accent)"
              strokeWidth="2.4"
              vectorEffect="non-scaling-stroke"
            />
            <path
              d={smooth(outPts)}
              fill="none"
              stroke="var(--color-line)"
              strokeWidth="2.4"
              strokeDasharray="5 4"
              vectorEffect="non-scaling-stroke"
            />
          </g>
        </svg>
        {/* hover columns with the week's numbers */}
        <div className="absolute inset-0 flex">
          {weekly.map((w) => (
            <div
              key={w.weekStart}
              tabIndex={0}
              className="group/w relative flex-1 outline-none"
              aria-label={`Week of ${w.weekStart}: ${w.reported} in, ${w.completed} finished`}
            >
              <span className="border-accent/30 absolute inset-y-0 left-1/2 border-l border-dashed opacity-0 transition-opacity group-hover/w:opacity-100 group-focus-visible/w:opacity-100" />
              <span className="bg-ink text-surface pointer-events-none absolute top-0 left-1/2 z-10 -translate-x-1/2 rounded-lg px-2.5 py-1.5 text-[11px] whitespace-nowrap opacity-0 shadow-lg transition-opacity group-hover/w:opacity-100 group-focus-visible/w:opacity-100">
                {new Date(w.weekStart).toLocaleDateString('en-GB', {
                  day: 'numeric',
                  month: 'short',
                })}
                <b className="ml-1.5">{w.reported}</b> in · <b>{w.completed}</b> done
              </span>
            </div>
          ))}
        </div>
      </div>
      <div className="text-muted flex justify-between text-[11px]">
        {weekly
          .filter((_, i) => i % 3 === 0 || i === n - 1)
          .map((w) => (
            <span key={w.weekStart}>
              {new Date(w.weekStart).toLocaleDateString('en-GB', {
                day: 'numeric',
                month: 'short',
              })}
            </span>
          ))}
      </div>
      <div className="text-muted flex gap-4 text-xs">
        <span className="flex items-center gap-1.5">
          <span className="bg-accent h-0.5 w-4 rounded" /> came in
        </span>
        <span className="flex items-center gap-1.5">
          <span className="border-line w-4 border-t-2 border-dashed" /> finished
        </span>
      </div>
    </div>
  )
}

// ---------- building league ----------

export function BuildingLeague({ buildings }: { buildings: Overview['byBuilding'] }) {
  const reduce = useReducedMotion()
  if (buildings.length === 0) return <EmptyBlock>No building to compare.</EmptyBlock>
  const ranked = [...buildings].sort(
    (a, b) => (b.onTimeRate ?? -1) - (a.onTimeRate ?? -1) || a.overdueNow - b.overdueNow,
  )
  return (
    <ol className="flex flex-col gap-3">
      {ranked.map((b, i) => (
        <li key={b.buildingId} className="well rounded-xl px-4 py-3">
          <div className="flex items-center gap-3">
            <span
              className={cn(
                'grid size-7 flex-none place-items-center rounded-lg text-xs font-bold',
                i === 0 ? 'bg-accent text-on-accent' : 'bg-active text-muted',
              )}
            >
              {i + 1}
            </span>
            <span className="min-w-0 flex-1 truncate font-semibold">{b.name}</span>
            <span className="text-muted text-xs tabular-nums">{b.openNow} open</span>
            {b.overdueNow > 0 && (
              <span className="bg-pill-negative-bg text-pill-negative-ink rounded-full px-2 py-0.5 text-xs font-medium tabular-nums">
                {b.overdueNow} late
              </span>
            )}
          </div>
          <div className="mt-2.5 flex items-center gap-3">
            <div className="bg-rule h-2 flex-1 overflow-hidden rounded-full">
              <motion.div
                className="bar-3d h-full origin-left rounded-full"
                style={{ width: `${Math.round((b.onTimeRate ?? 0) * 100)}%` }}
                initial={reduce ? false : { scaleX: 0 }}
                animate={{ scaleX: 1 }}
                transition={{ duration: 0.8, ease: EASE_SNAP, delay: 0.2 + i * 0.1 }}
              />
            </div>
            <span className="w-24 text-right text-xs tabular-nums">
              {b.onTimeRate === null
                ? 'no deadline yet'
                : `${Math.round(b.onTimeRate * 100)}% on time`}
            </span>
          </div>
        </li>
      ))}
    </ol>
  )
}

// ---------- emergency response ----------

const minutes = (m: number | null) => (m === null ? '—' : formatDuration(m * 60_000))

export function EmergencyResponse({ e }: { e: Overview['emergency'] }) {
  const reduce = useReducedMotion()
  const steps: { icon: IconName; label: string; value: string }[] = [
    { icon: 'bell', label: 'Acknowledged', value: minutes(e.avgAcknowledgeMinutes) },
    {
      icon: 'checklist',
      label: 'Fixed',
      value: e.avgFixHours === null ? '—' : formatDuration(e.avgFixHours * 3_600_000),
    },
  ]
  return (
    <div className="flex flex-col gap-5">
      <div className="flex items-baseline justify-between">
        <span className="text-[26px] leading-8 font-semibold tabular-nums">{e.count}</span>
        <span className="text-muted text-xs">
          emergencies · {e.downgraded} lowered to normal urgency · {e.merged} duplicates
        </span>
      </div>
      <div className="relative">
        <div className="bg-rule absolute top-5 right-1/4 left-1/4 h-1 rounded-full" />
        <motion.div
          className="bg-negative absolute top-5 left-1/4 h-1 origin-left rounded-full"
          style={{ width: '50%' }}
          initial={reduce ? false : { scaleX: 0 }}
          animate={{ scaleX: 1 }}
          transition={{ duration: 1, ease: EASE_SNAP, delay: 0.3 }}
        />
        <ol className="relative grid grid-cols-2">
          {steps.map((s, i) => (
            <motion.li
              key={s.label}
              className="flex flex-col items-center gap-1.5 text-center"
              initial={reduce ? false : { opacity: 0, y: 6 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.35, ease: EASE_SNAP, delay: 0.3 + i * 0.25 }}
            >
              <span className="bump text-negative grid size-10 place-items-center rounded-full">
                <Icon name={s.icon} className="size-[18px]" />
              </span>
              <span className="text-[15px] font-semibold tabular-nums">{s.value}</span>
              <span className="text-muted text-[11px]">average to {s.label.toLowerCase()}</span>
            </motion.li>
          ))}
        </ol>
      </div>
      <Link
        to={paths.emergencies}
        className="text-muted hover:text-ink flex items-center justify-end gap-1 text-xs font-medium"
      >
        Emergency board <Icon name="arrowRight" className="size-3.5" />
      </Link>
    </div>
  )
}
