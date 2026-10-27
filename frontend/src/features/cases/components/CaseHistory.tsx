import { Badge } from '@/shared/ui'
import { formatDateTime } from '@/shared/utils/format'
import { STATUS_LABELS, type Assignment, type CasePhoto, type StatusChange } from '../types'

/** Status timeline, newest first (who changed what, when, and why). */
export function StatusTimeline({ history }: { history: StatusChange[] }) {
  return (
    <ol className="flex flex-col gap-4">
      {[...history].reverse().map((h, i) => (
        <li key={h.at + i} className="border-line relative border-l-2 pl-4">
          <span className="bg-primary absolute top-1.5 -left-[5px] size-2 rounded-full" />
          <div className="text-sm">
            <span className="font-medium">{STATUS_LABELS[h.to]}</span>
            {h.from && <span className="text-muted"> from {STATUS_LABELS[h.from]}</span>}
          </div>
          <div className="text-muted text-xs">
            {h.by.fullName} · {formatDateTime(h.at)}
          </div>
          {h.reason && <p className="bg-canvas mt-1 rounded px-2 py-1 text-sm">{h.reason}</p>}
        </li>
      ))}
    </ol>
  )
}

const RESPONSE_TONES = { waiting: 'warning', accepted: 'success', declined: 'danger' } as const

/** Every assignment, including declined ones (N10). Shown to the officer and the manager. */
export function AssignmentList({ assignments }: { assignments: Assignment[] }) {
  if (assignments.length === 0) return <p className="text-muted text-sm">Not assigned yet.</p>
  return (
    <ul className="flex flex-col gap-3">
      {assignments.map((a, i) => (
        <li key={a.assignedAt + i} className="text-sm">
          <div className="flex items-center justify-between gap-2">
            <span className="font-medium">{a.technician.fullName}</span>
            <Badge tone={RESPONSE_TONES[a.response]}>{a.response}</Badge>
          </div>
          <div className="text-muted text-xs">
            by {a.assignedBy.fullName} · {formatDateTime(a.assignedAt)}
          </div>
          {a.declineReason && <p className="mt-1 text-xs">“{a.declineReason}”</p>}
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
  if (photos.length === 0) return <p className="text-muted text-sm">No photos.</p>
  return (
    <div className="flex flex-wrap gap-3">
      {photos.map((p) => (
        <a key={p.id} href={p.url} target="_blank" rel="noreferrer" className="group">
          <img
            src={p.url}
            alt={KIND_LABELS[p.kind]}
            className="border-line h-28 w-40 rounded-md border object-cover group-hover:opacity-90"
          />
          <span className="text-muted text-xs">{KIND_LABELS[p.kind]}</span>
        </a>
      ))}
    </div>
  )
}
