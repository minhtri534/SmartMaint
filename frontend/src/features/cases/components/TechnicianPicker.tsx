import { Badge } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { formatDateTime } from '@/shared/utils/format'
import { STATUS_LABELS, type TechnicianJob, type TechnicianOption } from '../types'
import { UrgencyText } from './CaseBadges'

interface Props {
  technicians: TechnicianOption[]
  /** Selected technician id as a string ("" = nobody). */
  value: string
  onChange: (id: string) => void
  /** The case being planned: its own job is not shown as "busy". */
  caseId: number
  error?: string
  /** Nobody is an allowed choice (an Edit before anyone took the job). */
  allowNone?: boolean
}

const SHOWN_JOBS = 3

/**
 * Meeting 07/11: choosing a technician shows their specialties and their schedule (the open jobs they hold, with the
 * planned window or the deadline), so the staff can see who fits and who is free. A radio list, least busy first.
 */
export function TechnicianPicker({
  technicians,
  value,
  onChange,
  caseId,
  error,
  allowNone,
}: Props) {
  return (
    <fieldset
      className="flex flex-col gap-2"
      aria-describedby={error ? 'technician-error' : undefined}
    >
      <legend className="mb-1 text-sm font-medium">Technician</legend>
      <ul className="border-border rounded-card divide-rule divide-y border">
        {allowNone && (
          <li>
            <Choice id="" checked={value === ''} onChange={onChange}>
              <span className="text-muted text-sm">Nobody yet</span>
            </Choice>
          </li>
        )}
        {technicians.map((t) => {
          const jobs = t.jobs.filter((j) => j.id !== caseId)
          return (
            <li key={t.id}>
              <Choice id={String(t.id)} checked={value === String(t.id)} onChange={onChange}>
                <div className="flex min-w-0 flex-1 flex-col gap-1">
                  <div className="flex flex-wrap items-baseline justify-between gap-x-3">
                    <span className="font-medium">{t.fullName}</span>
                    <span className="text-muted text-xs tabular-nums">
                      {jobs.length === 0
                        ? 'Free'
                        : `${jobs.length} open ${jobs.length === 1 ? 'job' : 'jobs'}`}
                    </span>
                  </div>
                  <div className="text-muted text-[13px]">
                    {t.specialties.length > 0 ? t.specialties.join(' · ') : 'No specialty listed'}
                  </div>
                  {jobs.length > 0 && (
                    <ul className="mt-1 flex flex-col gap-0.5">
                      {jobs.slice(0, SHOWN_JOBS).map((j) => (
                        <JobLine key={j.id} job={j} />
                      ))}
                      {jobs.length > SHOWN_JOBS && (
                        <li className="text-muted text-xs">and {jobs.length - SHOWN_JOBS} more</li>
                      )}
                    </ul>
                  )}
                </div>
              </Choice>
            </li>
          )
        })}
      </ul>
      {error && (
        <p id="technician-error" className="text-negative text-xs">
          {error}
        </p>
      )}
    </fieldset>
  )
}

function Choice({
  id,
  checked,
  onChange,
  children,
}: {
  id: string
  checked: boolean
  onChange: (id: string) => void
  children: React.ReactNode
}) {
  return (
    <label
      className={cn(
        'flex cursor-pointer items-start gap-3 px-3 py-2.5 transition-colors',
        checked ? 'bg-accent-soft' : 'hover:bg-raised',
      )}
    >
      <input
        type="radio"
        name="technician"
        value={id}
        checked={checked}
        onChange={() => onChange(id)}
        className="accent-primary mt-1 size-4 flex-none"
      />
      {children}
    </label>
  )
}

/** One open job: code, what it is, status, and when (planned window, else the deadline). */
function JobLine({ job }: { job: TechnicianJob }) {
  const when = job.plannedStartAt
    ? `${formatDateTime(job.plannedStartAt)}${job.plannedEndAt ? ` → ${formatDateTime(job.plannedEndAt)}` : ''}`
    : job.dueAt
      ? `due ${formatDateTime(job.dueAt)}`
      : 'not started'
  return (
    <li className="flex flex-wrap items-baseline gap-x-2 text-xs">
      <span className="font-mono">{job.code}</span>
      <span className="min-w-0 truncate">{job.title ?? 'Another building'}</span>
      <Badge tone="neutral">{STATUS_LABELS[job.status]}</Badge>
      <UrgencyText urgency={job.urgency} />
      <span className="text-muted tabular-nums">{when}</span>
    </li>
  )
}
