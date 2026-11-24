import { motion } from 'motion/react'
import { useId, useState } from 'react'
import { useSearchParams } from 'react-router'
import { Button, FilterDate, FilterSelect, HelpTip, SearchInput, Tabs } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { pageFade, slide } from '@/shared/utils/motion'
import { useCases } from '../hooks'
import { isOverdue } from '../sla'
import {
  CASE_STATUSES,
  STATUS_LABELS,
  URGENCIES,
  URGENCY_LABELS,
  type CaseStatus,
  type CaseSummary,
  type Urgency,
} from '../types'
import { CaseTable } from './CaseTable'

/** One extra filter: the id and the name a case has for it (null = it has none). */
interface MoreFilter {
  key: string
  label: string
  of: (c: CaseSummary) => readonly [id: number, name: string] | null
}

/** The filters of the staff and the manager (meeting note 18/11), each one a URL parameter. */
const MORE_FILTERS: readonly MoreFilter[] = [
  {
    key: 'technician',
    label: 'Technician',
    of: (c) => (c.technician ? [c.technician.id, c.technician.fullName] : null),
  },
  { key: 'staff', label: 'Staff', of: (c) => (c.staff ? [c.staff.id, c.staff.fullName] : null) },
  { key: 'asset', label: 'Asset', of: (c) => (c.asset ? [c.asset.id, c.asset.name] : null) },
  {
    key: 'assetType',
    label: 'Asset type',
    of: (c) => (c.asset ? [c.asset.typeId, c.asset.typeName] : null),
  },
  { key: 'fault', label: 'Fault category', of: (c) => [c.serviceTypeId, c.serviceTypeName] },
  {
    key: 'building',
    label: 'Location',
    of: (c) => [c.location.buildingId, c.location.buildingName],
  },
]

/** The choices of one filter: what occurs in the list, by name. */
function optionsOf(rows: CaseSummary[], of: MoreFilter['of']) {
  const found = new Map<string, string>()
  for (const c of rows) {
    const pair = of(c)
    if (pair) found.set(String(pair[0]), String(pair[1]))
  }
  return [...found.entries()].sort((a, b) => a[1].localeCompare(b[1]))
}

/** "CS-000022", "000022" or "22" all find case 22. */
function codeMatches(c: CaseSummary, typed: string) {
  const code = typed.trim().toUpperCase()
  if (!code) return true
  const digits = code.replace(/\D/g, '')
  return c.code.includes(code) || (digits !== '' && String(c.id) === String(Number(digits)))
}

const urgencyRank = (u: Urgency) => URGENCIES.indexOf(u)
const time = (iso: string | null) => (iso ? new Date(iso).getTime() : Number.POSITIVE_INFINITY)

const SORTS = {
  newest: {
    label: 'Newest first',
    compare: (a: CaseSummary, b: CaseSummary) => time(b.submittedAt) - time(a.submittedAt),
  },
  oldest: {
    label: 'Oldest first',
    compare: (a: CaseSummary, b: CaseSummary) => time(a.submittedAt) - time(b.submittedAt),
  },
  urgency: {
    label: 'Most urgent first',
    compare: (a: CaseSummary, b: CaseSummary) =>
      urgencyRank(a.urgency) - urgencyRank(b.urgency) || time(b.submittedAt) - time(a.submittedAt),
  },
  deadline: {
    label: 'Deadline soonest',
    compare: (a: CaseSummary, b: CaseSummary) => time(a.dueAt) - time(b.dueAt),
  },
} as const
type SortKey = keyof typeof SORTS

const isKey = <T extends object>(obj: T, key: string | null): key is Extract<keyof T, string> =>
  key !== null && Object.hasOwn(obj, key)

export interface CaseTab {
  value: string
  label: string
  /** Which cases belong to this filter. */
  match: (c: CaseSummary) => boolean
  emptyText: string
  /** Order of the rows (default: newest first, as the API sends them). */
  sort?: (a: CaseSummary, b: CaseSummary) => number
  /** One grey line under the count on a count card, e.g. "2 review · 1 assign". */
  note?: (rows: CaseSummary[]) => string | null
  /** What the tile counts, behind its "?" (count cards only). */
  help?: string
}

interface CaseListCardProps {
  tabs: readonly [CaseTab, ...CaseTab[]]
  /** "cards": count cards that filter (a queue to work through); "tabs": underline tabs (a personal list). */
  variant?: 'cards' | 'tabs'
  showRequester?: boolean
  showTechnician?: boolean
  timeColumn?: 'reported' | 'planned'
  /** The staff's and the manager's extra filters. */
  advanced?: boolean
}

/**
 * Case list with its filter, shared by the customer, staff, manager and technician screens.
 * The selected filter lives in the URL (?tab=...) so "Back" from a case returns to the same filter.
 * Counts are computed from the rows on every render.
 * Meeting 07/11: every list can also be filtered by status, urgency and the dates it was reported, found by its code,
 * and sorted; those choices live in the URL too. Tab counts ignore them.
 */
export function CaseListCard({
  tabs,
  variant = 'tabs',
  showRequester,
  showTechnician,
  timeColumn,
  advanced,
}: CaseListCardProps) {
  const cases = useCases()
  const tileGroup = useId()
  const [params, setParams] = useSearchParams()
  const current = tabs.find((t) => t.value === params.get('tab')) ?? tabs[0]
  const setParam = (key: string, value: string) =>
    setParams(
      (prev) => {
        const next = new URLSearchParams(prev)
        if (value) next.set(key, value)
        else next.delete(key)
        return next
      },
      { replace: true },
    )
  const select = (tab: string) => setParam('tab', tab)

  const rawStatus = params.get('status')
  const status = CASE_STATUSES.includes(rawStatus as CaseStatus) ? (rawStatus as CaseStatus) : ''
  const rawUrgency = params.get('urgency')
  const urgency = URGENCIES.includes(rawUrgency as Urgency) ? (rawUrgency as Urgency) : ''
  const from = params.get('from') ?? ''
  const to = params.get('to') ?? ''
  const code = params.get('code') ?? ''
  const more = MORE_FILTERS.map((f) => ({ ...f, value: params.get(f.key) ?? '' }))
  const moreActive = more.filter((f) => f.value !== '').length
  const [showMore, setShowMore] = useState(moreActive > 0)
  const rawSort = params.get('sort')
  const sort: SortKey | '' = isKey(SORTS, rawSort) ? rawSort : ''
  // ui/mercato-style: the top bar's search lands here as ?q= (title, code, building or room).
  const q = (params.get('q') ?? '').trim().toLowerCase()
  const filtered =
    status !== '' ||
    urgency !== '' ||
    from !== '' ||
    to !== '' ||
    code.trim() !== '' ||
    moreActive > 0 ||
    q !== ''

  if (cases.isPending) return <ListSkeleton cards={variant === 'cards' ? tabs.length : 0} />
  if (cases.error) {
    return (
      <div className="border-border rounded-card flex items-center justify-between gap-3 border px-4 py-3 text-sm">
        <span>Couldn't load the cases. {cases.error.message}</span>
        <Button variant="ghost" onClick={() => void cases.refetch()}>
          Retry
        </Button>
      </div>
    )
  }

  const all = cases.data
  const inTab = all.filter(current.match)
  // The chosen days, whole days in the viewer's time zone.
  const since = from ? new Date(`${from}T00:00:00`).getTime() : null
  const until = to ? new Date(`${to}T23:59:59.999`).getTime() : null
  const rows = inTab.filter(
    (c) =>
      (status === '' || c.status === status) &&
      (urgency === '' || c.urgency === urgency) &&
      (since === null || new Date(c.submittedAt).getTime() >= since) &&
      (until === null || new Date(c.submittedAt).getTime() <= until) &&
      codeMatches(c, code) &&
      more.every((f) => f.value === '' || String(f.of(c)?.[0]) === f.value) &&
      (q === '' ||
        [c.title, c.code, c.location.buildingName, c.location.roomName].some((t) =>
          t.toLowerCase().includes(q),
        )),
  )
  if (sort) rows.sort(SORTS[sort].compare)
  else if (current.sort) rows.sort(current.sort)
  // Only the statuses that occur in this tab are offered.
  const statuses = CASE_STATUSES.filter((s) => s === status || inTab.some((c) => c.status === s))

  return (
    <div className="flex flex-col gap-4">
      {variant === 'cards' ? (
        <div
          role="tablist"
          className="grid grid-cols-2 gap-2 sm:auto-cols-fr sm:grid-flow-col sm:grid-cols-none"
        >
          {tabs.map((t, tileIndex) => {
            const inTab = all.filter(t.match)
            const overdue = inTab.filter((c) => isOverdue(c)).length
            const note = t.note?.(inTab)
            const selected = t.value === current.value
            return (
              <div key={t.value} className="relative min-w-0">
                <button
                  type="button"
                  role="tab"
                  aria-selected={selected}
                  onClick={() => select(t.value)}
                  className={cn(
                    'card-soft rounded-card relative isolate h-full w-full min-w-0 px-4 py-3 pr-8 text-left',
                    'ease-snap transition-[border-color,transform] duration-150 active:scale-[0.98]',
                    selected ? 'border-transparent' : 'border-border hover:border-control',
                  )}
                >
                  {/* The selected tint slides to the chosen card (shared layoutId), like the tab underline. */}
                  {selected && (
                    <motion.span
                      layoutId={`case-tile-${tileGroup}`}
                      transition={slide}
                      className="rounded-card border-accent bg-accent-soft absolute -inset-px -z-10 border"
                    />
                  )}
                  <div className="text-muted text-[13px] leading-5">{t.label}</div>
                  <div className="flex items-baseline gap-2">
                    <span className="text-[28px] leading-8 font-normal tracking-[-0.03em] tabular-nums">
                      {inTab.length}
                    </span>
                    {overdue > 0 && (
                      <span className="text-negative truncate text-xs tabular-nums">
                        {overdue} overdue
                      </span>
                    )}
                  </div>
                  {note && <div className="text-muted mt-0.5 truncate text-xs">{note}</div>}
                </button>
                {t.help && (
                  <span className="absolute top-2.5 right-2.5">
                    <HelpTip label={t.label} align={tileIndex < tabs.length / 2 ? 'left' : 'right'}>
                      {t.help}
                    </HelpTip>
                  </span>
                )}
              </div>
            )
          })}
        </div>
      ) : (
        <Tabs
          items={tabs.map((t) => ({
            value: t.value,
            label: t.label,
            count: all.filter(t.match).length,
          }))}
          value={current.value}
          onChange={select}
        />
      )}

      {/* One row: the three filters, what they leave, then the order on the right (16/11, was 4 full-width
          selects). On a phone the row scrolls sideways inside itself so the list starts high on the screen. */}
      <div className="-mx-4 flex items-center gap-2 overflow-x-auto px-4 sm:mx-0 sm:flex-wrap sm:overflow-visible sm:px-0">
        <SearchInput
          label="Find a case by its code"
          placeholder="Case code"
          value={code}
          onChange={(e) => setParam('code', e.target.value)}
        />
        <FilterSelect
          label="Status"
          value={status}
          onChange={(e) => setParam('status', e.target.value)}
        >
          <option value="">All</option>
          {statuses.map((s) => (
            <option key={s} value={s}>
              {STATUS_LABELS[s]}
            </option>
          ))}
        </FilterSelect>
        <FilterSelect
          label="Urgency"
          value={urgency}
          onChange={(e) => setParam('urgency', e.target.value)}
        >
          <option value="">All</option>
          {URGENCIES.map((u) => (
            <option key={u} value={u}>
              {URGENCY_LABELS[u]}
            </option>
          ))}
        </FilterSelect>
        <FilterDate
          label="From"
          value={from}
          max={to || undefined}
          onChange={(v) => setParam('from', v)}
        />
        <FilterDate
          label="To"
          value={to}
          min={from || undefined}
          onChange={(v) => setParam('to', v)}
        />
        {advanced && (
          <Button variant="ghost" aria-expanded={showMore} onClick={() => setShowMore((v) => !v)}>
            {showMore ? 'Fewer filters' : 'More filters'}
            {moreActive > 0 && ` (${moreActive})`}
          </Button>
        )}

        {filtered && (
          <>
            {q && (
              <span className="bg-pill-accent-bg text-pill-accent-ink rounded-full px-2.5 py-1 text-xs whitespace-nowrap">
                “{params.get('q')}”
              </span>
            )}
            <span className="text-muted ml-1 text-sm tabular-nums">
              {rows.length} of {inTab.length}
            </span>
            <Button
              variant="ghost"
              onClick={() =>
                setParams(
                  (prev) => {
                    const next = new URLSearchParams(prev)
                    for (const key of [
                      'status',
                      'urgency',
                      'from',
                      'to',
                      'code',
                      'q',
                      ...MORE_FILTERS.map((f) => f.key),
                    ])
                      next.delete(key)
                    return next
                  },
                  { replace: true },
                )
              }
            >
              Clear
            </Button>
          </>
        )}

        <FilterSelect
          label="Sort"
          neutral
          value={sort}
          onChange={(e) => setParam('sort', e.target.value)}
          className="flex-none sm:ml-auto"
        >
          <option value="">{current.sort ? 'Overdue first' : 'Newest first'}</option>
          {Object.entries(SORTS)
            .filter(([key]) => current.sort || key !== 'newest')
            .map(([key, s]) => (
              <option key={key} value={key}>
                {s.label}
              </option>
            ))}
        </FilterSelect>
      </div>

      {advanced && showMore && (
        <div className="-mx-4 flex items-center gap-2 overflow-x-auto px-4 sm:mx-0 sm:flex-wrap sm:overflow-visible sm:px-0">
          {more.map((f) => (
            <FilterSelect
              key={f.key}
              label={f.label}
              value={f.value}
              onChange={(e) => setParam(f.key, e.target.value)}
            >
              <option value="">All</option>
              {optionsOf(all, f.of).map(([id, name]) => (
                <option key={id} value={id}>
                  {name}
                </option>
              ))}
            </FilterSelect>
          ))}
        </div>
      )}

      {/* A new filter swaps the rows with a 150ms fade (opacity only) instead of a flash. */}
      <motion.div
        key={[current.value, status, urgency, from, to, sort, ...more.map((f) => f.value)].join(
          '|',
        )}
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={pageFade}
      >
        <CaseTable
          cases={rows}
          showRequester={showRequester}
          showTechnician={showTechnician}
          timeColumn={timeColumn}
          emptyText={filtered ? 'No case matches these filters.' : current.emptyText}
        />
      </motion.div>
    </div>
  )
}

/** Loading: the final layout with flat grey bars at their real sizes, no shimmer. */
function ListSkeleton({ cards }: { cards: number }) {
  return (
    <div className="flex flex-col gap-4" aria-busy="true" aria-label="Loading cases">
      {cards > 0 && (
        <div className="hidden gap-2 sm:grid sm:auto-cols-fr sm:grid-flow-col">
          {Array.from({ length: cards }, (_, i) => (
            <div key={i} className="border-border rounded-card h-[66px] border px-3 py-2.5">
              <div className="bg-raised h-3.5 w-20 rounded" />
              <div className="bg-raised mt-2 h-5 w-8 rounded" />
            </div>
          ))}
        </div>
      )}
      <div className="border-border h-9 border-b" />
      {Array.from({ length: 4 }, (_, i) => (
        <div key={i} className="border-rule flex items-center justify-between border-b py-3">
          <div>
            <div className="bg-raised h-4 w-56 rounded" />
            <div className="bg-raised mt-2 h-3 w-40 rounded" />
          </div>
          <div className="bg-raised h-4 w-24 rounded" />
        </div>
      ))}
    </div>
  )
}
