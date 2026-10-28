import { Badge } from '@/shared/ui'
import { formatDateTime } from '@/shared/utils/format'
import { STATUS_LABELS, type Assignment, type CasePhoto, type StatusChange } from '../types'

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
            <span className="font-medium">{STATUS_LABELS[h.to]}</span>
            {h.from && <span className="text-muted"> from {STATUS_LABELS[h.from]}</span>}
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

const RESPONSE_TONES = { waiting: 'warning', accepted: 'positive', declined: 'negative' } as const
const RESPONSE_LABELS = { waiting: 'Waiting', accepted: 'Accepted', declined: 'Declined' } as const

/** Every assignment, including declined ones (N10). Shown to the staff and the manager. */
export function AssignmentList({ assignments }: { assignments: Assignment[] }) {
  if (assignments.length === 0) return <p className="text-muted text-sm">Not assigned yet.</p>
  return (
    <ul className="flex flex-col">
      {assignments.map((a, i) => (
        <li
          key={a.assignedAt + i}
          className="border-rule border-b py-2.5 first:pt-0 last:border-0 last:pb-0"
        >
          <div className="flex items-center justify-between gap-2">
            <span className="text-sm font-medium">{a.technician.fullName}</span>
            <Badge tone={RESPONSE_TONES[a.response]}>{RESPONSE_LABELS[a.response]}</Badge>
          </div>
          <div className="text-muted text-xs tabular-nums">
            by {a.assignedBy.fullName} · {formatDateTime(a.assignedAt)}
          </div>
          {a.declineReason && <p className="mt-1 text-sm">“{a.declineReason}”</p>}
        </li>
      ))}
    </ul>
  )
}

const KIND_LABELS: Record<CasePhoto['kind'], string> = {
  report: 'Reported',
  before: 'Before repair',
  after: 'After repair',
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
