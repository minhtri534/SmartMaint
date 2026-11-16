import { Link } from 'react-router'
import { caseDetailPath } from '@/app/paths'
import { formatDate, formatDateTime, formatTime } from '@/shared/utils/format'
import type { CaseSummary } from '../types'
import { StatusBadge, UrgencyText } from './CaseBadges'
import { SlaCell, SlaText } from './SlaTrack'

interface CaseTableProps {
  cases: CaseSummary[]
  /** Extra columns depending on who looks at the list. */
  showRequester?: boolean
  showTechnician?: boolean
  /** 'planned' (technician, meeting 07/11): when the staff planned the work to start, instead of when it was reported. */
  timeColumn?: 'reported' | 'planned'
  emptyText: string
}

/**
 * The case list. Desktop: a table of two-line rows (identity on line 1, grey detail on line 2), rules
 * between rows, no zebra, no header band, the deadline right-aligned. Phone: two-line rows, nothing dropped.
 * Meeting 07/11: the urgency has its own column and every row shows when it was reported.
 */
export function CaseTable({
  cases,
  showRequester,
  showTechnician,
  timeColumn = 'reported',
  emptyText,
}: CaseTableProps) {
  const planned = timeColumn === 'planned'
  if (cases.length === 0) {
    return (
      <div className="border-control text-muted rounded-card flex min-h-10 items-center justify-center border border-dashed px-4 py-2.5 text-sm">
        {emptyText}
      </div>
    )
  }

  return (
    <>
      <table className="hidden w-full text-left text-sm md:table">
        <thead>
          <tr className="border-border text-muted border-b text-xs">
            <th className="h-9 pr-4 font-normal">Case</th>
            <th className="hidden h-9 pr-4 font-normal 2xl:table-cell">Location</th>
            {showRequester && <th className="h-9 pr-4 font-normal">Reported by</th>}
            {showTechnician && (
              <th className="hidden h-9 pr-4 font-normal xl:table-cell">Technician</th>
            )}
            <th className="h-9 pr-4 font-normal">{planned ? 'Starts' : 'Reported'}</th>
            <th className="h-9 pr-4 font-normal">Urgency</th>
            <th className="h-9 pr-4 font-normal">Status</th>
            <th className="h-9 text-right font-normal">Deadline</th>
          </tr>
        </thead>
        <tbody>
          {cases.map((c) => (
            <tr key={c.id} className="border-rule border-b align-top last:border-0">
              <td className="max-w-[320px] py-3 pr-4">
                <Link
                  to={caseDetailPath(c.id)}
                  className="text-ink block truncate font-medium underline-offset-2 hover:underline"
                >
                  {c.title}
                </Link>
                <div className="text-muted mt-0.5 truncate text-[13px]">
                  <span className="font-mono text-xs">{c.code}</span> · {c.serviceTypeName}
                  {c.isPreventive && ' · Preventive'}
                  <span className="2xl:hidden"> · {c.location.roomName}</span>
                </div>
              </td>
              <td className="hidden py-3 pr-4 2xl:table-cell">
                <div className="max-w-[220px] truncate">{c.location.roomName}</div>
                <div className="text-muted mt-0.5 text-[13px] whitespace-nowrap">
                  {c.location.buildingName} · {c.location.floorName}
                </div>
              </td>
              {showRequester && (
                <td className="py-3 pr-4 whitespace-nowrap">{c.requester?.fullName ?? '—'}</td>
              )}
              {showTechnician && (
                <td className="hidden py-3 pr-4 whitespace-nowrap xl:table-cell">
                  {c.technician?.fullName ?? <span className="text-muted">—</span>}
                </td>
              )}
              <td className="py-3 pr-4 whitespace-nowrap tabular-nums">
                {planned ? (
                  c.plannedStartAt ? (
                    <>
                      {formatDate(c.plannedStartAt)}
                      <div className="text-muted mt-0.5 text-[13px]">
                        {formatTime(c.plannedStartAt)}
                      </div>
                    </>
                  ) : (
                    <span className="text-muted">Not planned</span>
                  )
                ) : (
                  <>
                    {formatDate(c.submittedAt)}
                    <div className="text-muted mt-0.5 text-[13px]">{formatTime(c.submittedAt)}</div>
                  </>
                )}
              </td>
              <td className="py-3 pr-4 whitespace-nowrap">
                <UrgencyText urgency={c.urgency} />
              </td>
              <td className="py-3 pr-4">
                <div className="flex flex-col items-start gap-1">
                  <StatusBadge status={c.status} />
                  {c.emergency?.mergedInto && (
                    <span className="text-muted text-xs">
                      Merged into {c.emergency.mergedInto.code}
                    </span>
                  )}
                </div>
              </td>
              <td className="py-3">
                <SlaCell c={c} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <ul className="flex flex-col md:hidden">
        {cases.map((c) => (
          <li key={c.id} className="border-rule border-b py-3 last:border-0">
            <div className="flex items-start justify-between gap-3">
              <Link to={caseDetailPath(c.id)} className="min-w-0 font-medium">
                {c.title}
              </Link>
              <StatusBadge status={c.status} />
            </div>
            <div className="text-muted mt-1 truncate text-[13px]">
              <span className="font-mono text-xs">{c.code}</span> · {c.location.roomName}
              {c.isPreventive && ' · Preventive'}
              {showTechnician && c.technician && <> · {c.technician.fullName}</>}
            </div>
            <div className="mt-1 flex flex-wrap gap-x-2 text-[13px]">
              <UrgencyText urgency={c.urgency} />
              <span className="text-muted tabular-nums">
                {planned
                  ? c.plannedStartAt
                    ? `starts ${formatDateTime(c.plannedStartAt)}`
                    : 'not planned'
                  : formatDateTime(c.submittedAt)}
              </span>
              <SlaText c={c} />
            </div>
          </li>
        ))}
      </ul>
    </>
  )
}
