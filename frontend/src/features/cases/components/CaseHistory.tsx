import { Badge } from '@/shared/ui'
import { formatDateTime } from '@/shared/utils/format'
import { STATUS_LABELS, type CasePhoto, type Rejection, type StatusChange } from '../types'

/** Status timeline, newest first: who changed what, when, and why. */
export function StatusTimeline({ history }: { history: StatusChange[] }) {
  return (
    <ol className="flex flex-col">
      {[...history].reverse().map((h, i, list) => (
        <li key={h.at + i} className="relative pb-4 pl-5 last:pb-0">
          {i < list.length - 1 && (
            <span className="bg-rule absolute top-3 bottom-0 left-[3px] w-0.5" aria-hidden="true" />
          )}
          <span
            className={
              i === 0
                ? 'bg-accent absolute top-1.5 left-0 size-2 rounded-full'
                : 'bg-control absolute top-1.5 left-0 size-2 rounded-full'
            }
            aria-hidden="true"
          />
          <div className="text-sm">
            {h.from === h.to ? (
              // Same status: an update with a reason (urgency, deadline, "not fixed", costs).
              <>
                <span className="font-medium">Update</span>
                <span className="text-muted"> while {STATUS_LABELS[h.to]}</span>
              </>
            ) : (
              <>
                <span className="font-medium">{STATUS_LABELS[h.to]}</span>
                {h.from && <span className="text-muted"> from {STATUS_LABELS[h.from]}</span>}
              </>
            )}
          </div>
          <div className="text-muted text-xs tabular-nums">
            {h.by.fullName} · {formatDateTime(h.at)}
          </div>
          {h.reason && (
            <p className="bg-raised rounded-control mt-1.5 px-2.5 py-1.5 text-sm">{h.reason}</p>
          )}
        </li>
      ))}
    </ol>
  )
}

/** Technicians who declined this case (N10, ERD: TechnicianRejections). Shown to the staff and the manager. */
export function RejectionList({ rejections }: { rejections: Rejection[] }) {
  return (
    <ul className="flex flex-col">
      {rejections.map((r, i) => (
        <li
          key={r.rejectedAt + i}
          className="border-rule border-b py-2.5 first:pt-0 last:border-0 last:pb-0"
        >
          <div className="flex items-center justify-between gap-2">
            <span className="text-sm font-medium">{r.technician.fullName}</span>
            <Badge tone="negative">Declined</Badge>
          </div>
          <div className="text-muted text-xs tabular-nums">{formatDateTime(r.rejectedAt)}</div>
          <p className="mt-1 text-sm">“{r.reason}”</p>
        </li>
      ))}
    </ul>
  )
}

const KIND_LABELS: Record<CasePhoto['kind'], string> = {
  report: 'Reported',
  before: 'Before repair',
  after: 'After repair',
  assessment: 'Assessment',
}

export function PhotoGallery({ photos }: { photos: CasePhoto[] }) {
  if (photos.length === 0) return <p className="text-muted text-sm">No photos attached.</p>
  return (
    <div className="flex flex-wrap gap-3">
      {photos.map((p) => (
        <a
          key={p.id}
          href={p.url}
          target="_blank"
          rel="noreferrer"
          className="group flex flex-col gap-1"
        >
          <img
            src={p.url}
            alt={KIND_LABELS[p.kind]}
            className="border-border h-28 w-40 rounded-lg border object-cover group-hover:opacity-90"
          />
          <span className="text-muted text-xs">{KIND_LABELS[p.kind]}</span>
        </a>
      ))}
    </div>
  )
}
