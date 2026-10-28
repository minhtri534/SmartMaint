import { Link } from 'react-router'
import { caseDetailPath } from '@/app/paths'
import { formatRelative } from '@/shared/utils/format'
import type { CaseSummary } from '../types'
import { StatusBadge, UrgencyNote } from './CaseBadges'
import { SlaCell, SlaText } from './SlaTrack'

interface CaseTableProps {
  cases: CaseSummary[]
  /** Extra columns depending on who looks at the list. */
  showRequester?: boolean
  showTechnician?: boolean
  emptyText: string
}

/**
 * The case list. Desktop: a table of two-line rows (identity on line 1, grey detail on line 2), rules
 * between rows, no zebra, no header band, the deadline right-aligned. Phone: two-line rows, nothing dropped.
 */
export function CaseTable({ cases, showRequester, showTechnician, emptyText }: CaseTableProps) {
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
            <th className="hidden h-9 pr-4 font-normal lg:table-cell">Location</th>
            {showRequester && <th className="h-9 pr-4 font-normal">Reported by</th>}
            {showTechnician && (
              <th className="hidden h-9 pr-4 font-normal lg:table-cell">Technician</th>
            )}
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
                  <span className="lg:hidden"> · {c.location.roomName}</span>
                </div>
              </td>
              <td className="hidden py-3 pr-4 lg:table-cell">
                <div className="max-w-[220px] truncate">{c.location.roomName}</div>
                <div className="text-muted mt-0.5 text-[13px] whitespace-nowrap">
                  {c.location.buildingName} · {c.location.floorName}
                </div>
              </td>
              {showRequester && (
                <td className="py-3 pr-4">
                  <div className="whitespace-nowrap">{c.requester.fullName}</div>
                  <div className="text-muted mt-0.5 text-[13px]">
                    {formatRelative(c.submittedAt)}
                  </div>
                </td>
              )}
              {showTechnician && (
                <td className="hidden py-3 pr-4 whitespace-nowrap lg:table-cell">
                  {c.technician?.fullName ?? <span className="text-muted">—</span>}
                </td>
              )}
              <td className="py-3 pr-4">
                <div className="flex flex-col items-start gap-1">
                  {c.isEmergencyFlow ? (
                    <span className="text-negative text-xs font-medium">Emergency · F4</span>
                  ) : (
                    <StatusBadge status={c.status} />
                  )}
                  <UrgencyNote urgency={c.urgency} />
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
              {showTechnician && c.technician && <> · {c.technician.fullName}</>}
            </div>
            <div className="mt-1 flex gap-2">
              <SlaText c={c} />
              <UrgencyNote urgency={c.urgency} />
            </div>
          </li>
        ))}
      </ul>
    </>
  )
}
