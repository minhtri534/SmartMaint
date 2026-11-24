import { useState } from 'react'
import { Alert, Badge, Button, Dialog, DialogActions, SearchInput } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { formatDateTime } from '@/shared/utils/format'
import { useTechnicianSuggestion } from '../hooks'
import type { CaseStatus, TechnicianJob, TechnicianOption } from '../types'

interface Props {
  /** In the order to show them: the ones who fit the case first, then the least busy. */
  technicians: TechnicianOption[]
  /** Selected technician id as a string ("" = nobody). */
  value: string
  onChange: (id: string) => void
  /** The case being planned. */
  caseId: number
  error?: string
}

/** How many technicians the form itself shows; the rest are in "View all available technicians". */
const SHORT_LIST = 3
const PAGE_SIZE = 6
/** Not-started jobs listed per technician (soonest first); the rest are counted. */
const SHOWN_JOBS = 3

/** Given to the technician but not started yet: the jobs that decide when they are free. */
const NOT_STARTED: readonly CaseStatus[] = ['approved', 'accepted', 'reopened']

const isWorkingNow = (t: TechnicianOption) => t.jobs.some((j) => j.status === 'in_progress')
const waiting = (t: TechnicianOption) => t.jobs.filter((j) => NOT_STARTED.includes(j.status))

/**
 * Meeting note 18/11: the form shows a few technicians who are free now (not working on a job), the ones who fit the
 * job first. "View all available technicians" opens the whole list with a search and paging. Each technician shows
 * only what decides the choice: the jobs given to them and not started yet, with their start time and deadline.
 * The AI can suggest one; the staff member still chooses.
 */
export function TechnicianPicker({ technicians, value, onChange, caseId, error }: Props) {
  const [showAll, setShowAll] = useState(false)
  const suggestion = useTechnicianSuggestion()

  const available = technicians.filter((t) => !isWorkingNow(t))
  const selected = technicians.find((t) => String(t.id) === value)
  const short = available.slice(0, SHORT_LIST)
  // Someone chosen from the whole list stays in sight.
  const rows = selected && !short.includes(selected) ? [selected, ...short.slice(0, -1)] : short
  const suggested = technicians.find((t) => t.id === suggestion.data?.technicianId)

  return (
    <fieldset
      className="flex flex-col gap-2"
      aria-describedby={error ? 'technician-error' : undefined}
    >
      <legend className="mb-1 text-sm font-medium">Technician</legend>
      <div className="flex flex-wrap items-center justify-between gap-2">
        <p className="text-muted text-[13px]">
          {available.length === 0
            ? 'Every technician is working on a job right now.'
            : `Free now, the ones who fit this job first (${available.length} free).`}
        </p>
        <Button
          variant="secondary"
          loading={suggestion.isPending}
          onClick={() => suggestion.mutate(caseId)}
        >
          Suggest with AI
        </Button>
      </div>

      {suggestion.error && <Alert>{suggestion.error.message}</Alert>}
      {suggestion.data && (
        <div className="border-accent bg-accent-soft rounded-control flex flex-wrap items-center justify-between gap-3 border px-3 py-2 text-sm">
          <div className="min-w-0">
            <div className="font-medium">
              {suggested ? `Suggested: ${suggested.fullName}` : 'No suggestion'}
            </div>
            <p className="text-muted text-[13px]">
              {suggestion.data.reason}
              {suggestion.data.source === 'rules' &&
                ' The AI is offline, so this comes from specialties and workload.'}
            </p>
          </div>
          {suggested && String(suggested.id) !== value && (
            <Button variant="primary" onClick={() => onChange(String(suggested.id))}>
              Use this
            </Button>
          )}
        </div>
      )}

      {rows.length > 0 && (
        <ul className="border-border rounded-card divide-rule divide-y border">
          {rows.map((t) => (
            <li key={t.id}>
              <TechnicianRow technician={t} checked={value === String(t.id)} onChange={onChange} />
            </li>
          ))}
        </ul>
      )}

      <div className="flex flex-wrap items-center justify-between gap-2 text-sm">
        <span className="text-muted">
          {selected ? `Selected: ${selected.fullName}` : 'Nobody selected'}
        </span>
        <Button variant="ghost" onClick={() => setShowAll(true)}>
          View all available technicians
        </Button>
      </div>
      {error && (
        <p id="technician-error" className="text-negative text-xs">
          {error}
        </p>
      )}

      <AllTechniciansDialog
        open={showAll}
        technicians={technicians}
        value={value}
        onClose={() => setShowAll(false)}
        onChoose={(id) => {
          onChange(id)
          setShowAll(false)
        }}
      />
    </fieldset>
  )
}

/** The whole list: free technicians by default, the busy ones on request. Choosing one closes it. */
function AllTechniciansDialog({
  open,
  technicians,
  value,
  onClose,
  onChoose,
}: {
  open: boolean
  technicians: TechnicianOption[]
  value: string
  onClose: () => void
  onChoose: (id: string) => void
}) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      wide
      title="Available technicians"
      description="Free now, the ones who fit this job first."
    >
      {open && (
        <AllTechnicians
          technicians={technicians}
          value={value}
          onClose={onClose}
          onChoose={onChoose}
        />
      )}
    </Dialog>
  )
}

function AllTechnicians({
  technicians,
  value,
  onClose,
  onChoose,
}: {
  technicians: TechnicianOption[]
  value: string
  onClose: () => void
  onChoose: (id: string) => void
}) {
  const [search, setSearch] = useState('')
  const [withBusy, setWithBusy] = useState(false)
  const [page, setPage] = useState(0)
  const [choice, setChoice] = useState(value)

  const words = search.trim().toLowerCase()
  const found = technicians.filter(
    (t) =>
      (withBusy || !isWorkingNow(t)) &&
      (!words ||
        t.fullName.toLowerCase().includes(words) ||
        t.specialties.some((s) => s.toLowerCase().includes(words))),
  )
  const lastPage = Math.max(0, Math.ceil(found.length / PAGE_SIZE) - 1)
  const current = Math.min(page, lastPage)
  const rows = found.slice(current * PAGE_SIZE, (current + 1) * PAGE_SIZE)

  return (
    <div className="flex flex-col gap-3">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <SearchInput
          label="Search technicians"
          placeholder="Name or specialty"
          value={search}
          onChange={(e) => {
            setSearch(e.target.value)
            setPage(0)
          }}
        />
        <label className="flex items-center gap-2 text-sm">
          <input
            type="checkbox"
            checked={withBusy}
            onChange={(e) => {
              setWithBusy(e.target.checked)
              setPage(0)
            }}
            className="accent-primary size-4"
          />
          Also show the ones working now
        </label>
      </div>

      {rows.length === 0 ? (
        <p className="text-muted border-control rounded-card border border-dashed px-3 py-4 text-center text-sm">
          {words ? `No technician matches “${search}”.` : 'No technician is free right now.'}
        </p>
      ) : (
        <ul className="border-border rounded-card divide-rule divide-y border">
          {rows.map((t) => (
            <li key={t.id}>
              <TechnicianRow
                technician={t}
                name="technician-all"
                checked={choice === String(t.id)}
                onChange={setChoice}
              />
            </li>
          ))}
        </ul>
      )}

      {lastPage > 0 && (
        <div className="flex items-center justify-end gap-1 text-sm">
          <span className="text-muted mr-1 tabular-nums">
            Page {current + 1} of {lastPage + 1}
          </span>
          <Button variant="ghost" disabled={current === 0} onClick={() => setPage(current - 1)}>
            Previous
          </Button>
          <Button
            variant="ghost"
            disabled={current === lastPage}
            onClick={() => setPage(current + 1)}
          >
            Next
          </Button>
        </div>
      )}

      <DialogActions>
        <Button variant="ghost" onClick={onClose}>
          Back
        </Button>
        <Button variant="primary" disabled={!choice} onClick={() => onChoose(choice)}>
          Choose
        </Button>
      </DialogActions>
    </div>
  )
}

/** Name, whether they fit, and the jobs given to them that have not started (start time and deadline). */
function TechnicianRow({
  technician: t,
  checked,
  onChange,
  name = 'technician',
}: {
  technician: TechnicianOption
  checked: boolean
  onChange: (id: string) => void
  name?: string
}) {
  const id = String(t.id)
  const next = waiting(t)
  const working = isWorkingNow(t)
  return (
    <label
      className={cn(
        'flex cursor-pointer items-start gap-3 px-3 py-2.5 transition-colors',
        checked ? 'bg-accent-soft' : 'hover:bg-raised',
      )}
    >
      <input
        type="radio"
        name={name}
        value={id}
        checked={checked}
        onChange={() => onChange(id)}
        className="accent-primary mt-1 size-4 flex-none"
      />
      <div className="flex min-w-0 flex-1 flex-col gap-1">
        <div className="flex flex-wrap items-baseline justify-between gap-x-3">
          <span className="flex items-baseline gap-2">
            <span className="font-medium">{t.fullName}</span>
            {t.fits && <Badge tone="positive">Fits this job</Badge>}
            {working && <Badge tone="warning">Working now</Badge>}
          </span>
          <span className="text-muted text-xs tabular-nums">
            {next.length === 0
              ? 'Nothing waiting'
              : `${next.length} not started ${next.length === 1 ? 'job' : 'jobs'}`}
          </span>
        </div>
        {next.length > 0 && (
          <ul className="flex flex-col gap-0.5">
            {next.slice(0, SHOWN_JOBS).map((j) => (
              <JobLine key={j.id} job={j} />
            ))}
            {next.length > SHOWN_JOBS && (
              <li className="text-muted text-xs">and {next.length - SHOWN_JOBS} more</li>
            )}
          </ul>
        )}
      </div>
    </label>
  )
}

/** A job not started yet: its code, when it starts and its deadline. */
function JobLine({ job }: { job: TechnicianJob }) {
  return (
    <li className="text-muted flex flex-wrap items-baseline gap-x-3 text-xs tabular-nums">
      <span className="text-ink font-mono">{job.code}</span>
      <span>Starts {job.plannedStartAt ? formatDateTime(job.plannedStartAt) : 'not set'}</span>
      <span>Deadline {job.dueAt ? formatDateTime(job.dueAt) : 'none'}</span>
    </li>
  )
}
