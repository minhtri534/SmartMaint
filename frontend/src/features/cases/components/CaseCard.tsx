import { Link } from 'react-router'
import { caseDetailPath } from '@/app/paths'
import { Badge, Icon, Rise, type BadgeTone } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { formatDateTime, formatRelative } from '@/shared/utils/format'
import { CHIP_CLASSES, groupOf, groupStyle } from '../service-groups'
import { slaOf } from '../sla'
import { URGENCY_LABELS, type CaseSummary, type Urgency } from '../types'
import { StatusBadge } from './CaseBadges'
import { SlaText } from './SlaTrack'

/*
 * One case as a card (ui/mercato-style, 17/11): Leader found the table rows hard to read (rules everywhere, the
 * deadline cut off on the right). The card reads top to bottom: what kind of problem (a tinted chip per group),
 * the title, where, who, and the deadline with how much of its window is used.
 */

const URGENCY_TONES: Record<Urgency, BadgeTone> = {
  emergency: 'negative',
  high: 'warning',
  normal: 'neutral',
  low: 'neutral',
}

function initials(name: string) {
  const parts = name.trim().split(/\s+/)
  return ((parts[0]?.[0] ?? '') + (parts.length > 1 ? (parts.at(-1)?.[0] ?? '') : '')).toUpperCase()
}

function Person({ name, role }: { name: string | null; role: string }) {
  return (
    <div className="flex min-w-0 items-center gap-2">
      <span
        className={cn(
          'grid size-7 flex-none place-items-center rounded-full text-[10.5px] font-semibold',
          name ? 'bg-active text-ink' : 'border-control text-faint border border-dashed',
        )}
        aria-hidden="true"
      >
        {name ? initials(name) : '?'}
      </span>
      <div className="min-w-0 leading-tight">
        <div className="text-faint text-[11px]">{role}</div>
        <div className={cn('truncate text-[13px]', !name && 'text-muted')}>
          {name ?? 'Not assigned'}
        </div>
      </div>
    </div>
  )
}

export function CaseCard({
  c,
  index = 0,
  showRequester,
  showTechnician,
  planned,
}: {
  c: CaseSummary
  index?: number
  showRequester?: boolean
  showTechnician?: boolean
  planned?: boolean
}) {
  const kind = groupStyle(groupOf(c.serviceTypeName))
  const sla = slaOf(c)
  const showTrack = sla.state === 'running' || sla.state === 'met'

  return (
    // the first cards rise in one after another; further down they simply appear (no long waits)
    <Rise index={Math.min(index, 8)} className="h-full">
      <Link
        to={caseDetailPath(c.id)}
        className="card-soft card-hover rounded-card group flex h-full flex-col p-4 sm:p-5"
      >
        <div className="flex items-start gap-3">
          <span
            className={cn(
              'grid size-10 flex-none place-items-center rounded-xl shadow-[inset_0_1px_0_rgb(255_255_255/0.5)]',
              CHIP_CLASSES[kind.hue],
            )}
            aria-hidden="true"
          >
            <Icon name={kind.icon} className="size-5" />
          </span>
          <div className="min-w-0 flex-1">
            <div className="text-faint flex items-center gap-2 text-xs">
              <span className="font-mono">{c.code}</span>
              {c.isPreventive && <span>· Preventive</span>}
              {c.isEmergencyFlow && <span className="text-negative font-medium">· Emergency</span>}
            </div>
            <h3 className="group-hover:text-accent-ink mt-0.5 line-clamp-2 text-[15px] leading-5 font-semibold transition-colors duration-150">
              {c.title}
            </h3>
            <div className="text-muted mt-0.5 truncate text-[13px]">{c.serviceTypeName}</div>
          </div>
          <div className="flex flex-none flex-col items-end gap-1.5">
            <StatusBadge status={c.status} />
            <Badge tone={URGENCY_TONES[c.urgency]}>{URGENCY_LABELS[c.urgency]}</Badge>
          </div>
        </div>

        <div className="mt-4 mb-4 grid grid-cols-2 gap-x-4 gap-y-2 text-[13px]">
          <div className="flex min-w-0 items-start gap-2">
            <Icon name="pin" className="text-faint mt-0.5 size-4" />
            <div className="min-w-0 leading-tight">
              <div className="truncate">{c.location.roomName}</div>
              <div className="text-muted truncate text-xs">
                {c.location.buildingName} · {c.location.floorName}
              </div>
            </div>
          </div>
          <div className="flex min-w-0 items-start gap-2">
            <Icon name={planned ? 'calendar' : 'clock'} className="text-faint mt-0.5 size-4" />
            <div className="min-w-0 leading-tight">
              <div className="truncate tabular-nums">
                {planned
                  ? c.plannedStartAt
                    ? formatDateTime(c.plannedStartAt)
                    : 'Not planned'
                  : formatRelative(c.submittedAt)}
              </div>
              <div className="text-muted truncate text-xs">
                {planned ? 'planned start' : `reported ${formatDateTime(c.submittedAt)}`}
              </div>
            </div>
          </div>
        </div>

        <div className="border-rule mt-auto flex items-end justify-between gap-3 border-t pt-3">
          <div className="flex min-w-0 gap-4">
            {showRequester && (
              // a preventive work order has no requester: it comes from a maintenance plan (F2)
              <Person
                name={c.requester?.fullName ?? (c.isPreventive ? 'Maintenance plan' : null)}
                role="Reported by"
              />
            )}
            {showTechnician && <Person name={c.technician?.fullName ?? null} role="Technician" />}
            {!showRequester && !showTechnician && (
              <Person name={c.technician?.fullName ?? null} role="Technician" />
            )}
          </div>
          <div className="flex flex-none flex-col items-end gap-1 text-right">
            <SlaText c={c} />
            {c.dueAt && sla.state !== 'met' && (
              <span className="text-faint text-[11px] tabular-nums">
                due {formatDateTime(c.dueAt)}
              </span>
            )}
          </div>
        </div>
        {showTrack && (
          <div className="bg-rule mt-3 h-1 overflow-hidden rounded-full">
            <div
              className={cn(
                'ease-snap h-full origin-left rounded-full transition-transform duration-700',
                sla.state === 'met' ? 'bg-control' : 'bar-3d',
              )}
              style={{ transform: `scaleX(${Math.max(0.03, Math.min(1, sla.used))})` }}
            />
          </div>
        )}
        {c.emergency?.mergedInto && (
          <div className="text-muted mt-2 text-xs">Merged into {c.emergency.mergedInto.code}</div>
        )}
      </Link>
    </Rise>
  )
}
