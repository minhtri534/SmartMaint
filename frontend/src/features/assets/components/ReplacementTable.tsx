import type { ReactNode } from 'react'
import { Link } from 'react-router'
import { assetDetailPath, caseDetailPath } from '@/app/paths'
import { Badge, EmptyBlock, type BadgeTone } from '@/shared/ui'
import { formatDate } from '@/shared/utils/format'
import type { ReplacementRequest, ReplacementStatus } from '../types'

const TONES: Record<ReplacementStatus, BadgeTone> = {
  open: 'warning',
  done: 'positive',
  dismissed: 'neutral',
}
const LABELS: Record<ReplacementStatus, string> = {
  open: 'To register',
  done: 'Registered',
  dismissed: 'Dismissed',
}

/**
 * A06 replacement requests: old asset → new model. The case code is a link only for staff and managers; the
 * Administrator gets the code to quote (BR-11). `actions` draws the Administrator's buttons per row.
 */
export function ReplacementTable({
  rows,
  emptyText,
  actions,
}: {
  rows: ReplacementRequest[]
  emptyText: string
  actions?: (r: ReplacementRequest) => ReactNode
}) {
  if (rows.length === 0) return <EmptyBlock>{emptyText}</EmptyBlock>
  return (
    <table className="w-full text-left text-sm">
      <thead>
        <tr className="border-border text-muted border-b text-xs">
          <th className="h-9 pr-4 font-normal">Old asset</th>
          <th className="h-9 pr-4 font-normal">New model</th>
          <th className="hidden h-9 pr-4 font-normal md:table-cell">Asked by</th>
          <th className="h-9 pr-4 font-normal">Status</th>
          {actions && <th className="h-9" />}
        </tr>
      </thead>
      <tbody>
        {rows.map((r) => (
          <tr key={r.id} className="border-rule border-b align-top last:border-0">
            <td className="py-3 pr-4">
              <Link
                to={assetDetailPath(r.oldAsset.id)}
                className="font-medium underline-offset-2 hover:underline"
              >
                {r.oldAsset.name}
              </Link>
              <div className="text-muted text-[13px]">
                {r.location.buildingName} · {r.location.floorName}, {r.location.roomName}
              </div>
            </td>
            <td className="py-3 pr-4">
              {r.newModel}
              {r.newSerial && <div className="text-muted text-[13px]">Serial {r.newSerial}</div>}
              {r.newAsset && (
                <Link
                  to={assetDetailPath(r.newAsset.id)}
                  className="text-[13px] underline underline-offset-2"
                >
                  {r.newAsset.name}
                </Link>
              )}
            </td>
            <td className="hidden py-3 pr-4 md:table-cell">
              {r.requestedBy}
              <div className="text-muted text-[13px]">
                {r.caseId ? (
                  <Link to={caseDetailPath(r.caseId)} className="font-mono underline">
                    {r.caseCode}
                  </Link>
                ) : (
                  <span className="font-mono">{r.caseCode}</span>
                )}{' '}
                · {formatDate(r.requestedAt)}
              </div>
            </td>
            <td className="py-3 pr-4">
              <Badge tone={TONES[r.status]}>{LABELS[r.status]}</Badge>
              {r.dismissReason && (
                <div className="text-muted mt-1 text-[13px]">{r.dismissReason}</div>
              )}
            </td>
            {actions && <td className="py-2 text-right whitespace-nowrap">{actions(r)}</td>}
          </tr>
        ))}
      </tbody>
    </table>
  )
}
