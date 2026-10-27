import { Link } from 'react-router'
import { caseDetailPath } from '@/app/paths'
import { Badge } from '@/shared/ui'
import { formatDateTime } from '@/shared/utils/format'
import { isOverdue } from '../state-machine'
import type { CaseSummary } from '../types'
import { StatusBadge, UrgencyBadge } from './CaseBadges'

interface CaseTableProps {
  cases: CaseSummary[]
  /** Extra columns depending on who looks at the list. */
  showRequester?: boolean
  showTechnician?: boolean
  emptyText: string
}

export function CaseTable({ cases, showRequester, showTechnician, emptyText }: CaseTableProps) {
  if (cases.length === 0) {
    return <p className="text-muted py-10 text-center text-sm">{emptyText}</p>
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-left text-sm">
        <thead className="text-muted border-line border-b text-xs uppercase">
          <tr>
            <th className="px-3 py-2 font-medium">Case</th>
            <th className="px-3 py-2 font-medium">Location</th>
            {showRequester && <th className="px-3 py-2 font-medium">Reported by</th>}
            {showTechnician && <th className="px-3 py-2 font-medium">Technician</th>}
            <th className="px-3 py-2 font-medium">Urgency</th>
            <th className="px-3 py-2 font-medium">Status</th>
            <th className="px-3 py-2 font-medium">Submitted</th>
            <th className="px-3 py-2 font-medium">Due</th>
          </tr>
        </thead>
        <tbody>
          {cases.map((c) => (
            <tr key={c.id} className="border-line hover:bg-canvas border-b last:border-0">
              <td className="px-3 py-2.5">
                <Link
                  to={caseDetailPath(c.id)}
                  className="text-primary font-medium hover:underline"
                >
                  {c.title}
                </Link>
                <div className="text-muted text-xs">
                  {c.code} · {c.serviceTypeName}
                </div>
              </td>
              <td className="px-3 py-2.5">
                <div>{c.location.roomName}</div>
                <div className="text-muted text-xs">
                  {c.location.buildingName} · {c.location.floorName}
                </div>
              </td>
              {showRequester && <td className="px-3 py-2.5">{c.requester.fullName}</td>}
              {showTechnician && (
                <td className="px-3 py-2.5">{c.technician?.fullName ?? <Muted>—</Muted>}</td>
              )}
              <td className="px-3 py-2.5">
                <UrgencyBadge urgency={c.urgency} />
              </td>
              <td className="px-3 py-2.5">
                <div className="flex flex-wrap gap-1">
                  <StatusBadge status={c.status} />
                  {c.isEmergencyFlow && <Badge tone="danger">F4</Badge>}
                </div>
              </td>
              <td className="px-3 py-2.5 whitespace-nowrap">{formatDateTime(c.submittedAt)}</td>
              <td className="px-3 py-2.5 whitespace-nowrap">
                {c.dueAt ? (
                  <span className={isOverdue(c) ? 'text-danger font-medium' : undefined}>
                    {formatDateTime(c.dueAt)}
                    {isOverdue(c) && ' · overdue'}
                  </span>
                ) : (
                  <Muted>—</Muted>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function Muted({ children }: { children: string }) {
  return <span className="text-muted">{children}</span>
}
