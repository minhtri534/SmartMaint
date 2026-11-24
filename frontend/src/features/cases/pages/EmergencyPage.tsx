import { useState } from 'react'
import { Link, useSearchParams } from 'react-router'
import { caseDetailPath } from '@/app/paths'
import {
  EmptyBlock,
  ErrorBlock,
  FilterSelect,
  LoadingBlock,
  PageHeader,
  SearchInput,
  Tabs,
} from '@/shared/ui'
import { formatDateTime, formatDuration } from '@/shared/utils/format'
import { StatusBadge } from '../components/CaseBadges'
import { SlaCell } from '../components/SlaTrack'
import { useCases } from '../hooks'
import { FINAL_STATUSES, type CaseSummary } from '../types'

/** The staff must acknowledge within this (then the backup staff are alerted): drawn in the warning colour after it. */
const ACK_TARGET_MS = 10 * 60_000

const TABS = [
  { value: 'open', label: 'Open' },
  { value: 'finished', label: 'Finished' },
] as const

const isOpen = (c: CaseSummary) => !FINAL_STATUSES.includes(c.status) && c.status !== 'completed'

/**
 * F4 board (staff and managers of the buildings, Leader 02/11): every emergency of the caller's buildings with its
 * two milestones. Question: "Is any emergency waiting for someone?" Polls every 30 seconds; the detail is the case page.
 */
export function EmergencyPage() {
  const cases = useCases(30_000)
  const [params, setParams] = useSearchParams()
  const tab = TABS.find((t) => t.value === params.get('tab'))?.value ?? 'open'
  const [search, setSearch] = useState('')
  const [order, setOrder] = useState<'' | 'oldest' | 'newest'>('')

  const all = (cases.data ?? []).filter((c) => c.emergency && !c.emergency.mergedInto)
  const open = all.filter(isOpen)
  const words = search.trim().toLowerCase()
  // Open ones start with the one waiting longest, finished ones with the latest, until the staff picks an order.
  const oldestFirst = order === '' ? tab === 'open' : order === 'oldest'
  const rows = (tab === 'open' ? open : all.filter((c) => !isOpen(c)))
    .filter(
      (c) =>
        !words ||
        [c.code, c.title, c.location.buildingName, c.location.roomName].some((t) =>
          t.toLowerCase().includes(words),
        ),
    )
    .sort(
      (a, b) =>
        (oldestFirst ? 1 : -1) * a.emergency!.startedAt.localeCompare(b.emergency!.startedAt),
    )
  const now = cases.dataUpdatedAt
  const waiting = open.filter((c) => !c.emergency!.acknowledgedAt).length

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title="Emergencies"
        description={
          cases.data &&
          (open.length === 0
            ? 'No open emergency.'
            : `${open.length} open, ${waiting} not acknowledged yet.`)
        }
      />
      <Tabs
        items={TABS.map((t) => ({
          ...t,
          count: t.value === 'open' ? open.length : all.length - open.length,
        }))}
        value={tab}
        onChange={(v) => setParams({ tab: v }, { replace: true })}
      />
      <div className="flex flex-wrap items-center gap-2">
        <SearchInput
          label="Search emergencies"
          placeholder="Code, title or place"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <FilterSelect
          label="Sort"
          neutral
          className="sm:ml-auto"
          value={oldestFirst ? 'oldest' : 'newest'}
          onChange={(e) => setOrder(e.target.value as 'oldest' | 'newest')}
        >
          <option value="oldest">Oldest first</option>
          <option value="newest">Newest first</option>
        </FilterSelect>
      </div>
      {cases.isPending ? (
        <LoadingBlock rows={4} />
      ) : cases.error ? (
        <ErrorBlock
          what="the emergencies"
          message={cases.error.message}
          onRetry={() => void cases.refetch()}
        />
      ) : rows.length === 0 ? (
        <EmptyBlock>
          {words
            ? `No emergency matches “${search}”.`
            : tab === 'open'
              ? 'No open emergency.'
              : 'No finished emergency.'}
        </EmptyBlock>
      ) : (
        <table className="w-full text-left text-sm">
          <thead>
            <tr className="border-border text-muted border-b text-xs">
              <th className="h-9 pr-4 font-normal">Emergency</th>
              <th className="hidden h-9 pr-4 font-normal md:table-cell">Acknowledged</th>
              <th className="h-9 pr-4 font-normal">Status</th>
              <th className="h-9 text-right font-normal">Deadline</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((c) => (
              <tr key={c.id} className="border-rule border-b align-top last:border-0">
                <td className="max-w-[340px] py-3 pr-4">
                  <Link
                    to={caseDetailPath(c.id)}
                    className="text-ink block truncate font-medium underline-offset-2 hover:underline"
                  >
                    {c.title}
                  </Link>
                  <div className="text-muted mt-0.5 truncate text-[13px]">
                    <span className="font-mono text-xs">{c.code}</span> · {c.location.roomName},{' '}
                    {c.location.buildingName} · {formatDateTime(c.emergency!.startedAt)}
                  </div>
                </td>
                <td className="hidden py-3 pr-4 md:table-cell">
                  <Milestone
                    c={c}
                    at={c.emergency!.acknowledgedAt}
                    now={now}
                    warnAfter={ACK_TARGET_MS}
                  />
                </td>
                <td className="py-3 pr-4">
                  <div className="flex flex-col items-start gap-1">
                    <StatusBadge status={c.status} />
                    {c.technician && (
                      <span className="text-muted text-xs">{c.technician.fullName}</span>
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
      )}
    </div>
  )
}

/** "after 4 m" once reached; "waiting 12 m" while open (warning colour past the target); a dash otherwise. */
function Milestone({
  c,
  at,
  now,
  warnAfter,
}: {
  c: CaseSummary
  at: string | null
  now: number
  warnAfter?: number
}) {
  const start = new Date(c.emergency!.startedAt).getTime()
  if (at) {
    return (
      <span className="tabular-nums">after {formatDuration(new Date(at).getTime() - start)}</span>
    )
  }
  if (!isOpen(c)) return <span className="text-muted">—</span>
  const waited = now - start
  const late = warnAfter !== undefined && waited > warnAfter
  return (
    <span className={late ? 'text-negative font-medium tabular-nums' : 'text-muted tabular-nums'}>
      waiting {formatDuration(waited)}
    </span>
  )
}
