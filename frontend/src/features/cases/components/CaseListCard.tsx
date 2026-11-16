import { motion } from 'motion/react'
import { useId } from 'react'
import { useSearchParams } from 'react-router'
import { Button, FilterSelect, Tabs } from '@/shared/ui'
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

const DAY = 24 * 60 * 60 * 1000

/** "Reported" filter: how far back. */
const PERIODS = {
  today: { label: 'Today', days: 0 },
  '7d': { label: 'Last 7 days', days: 7 },
  '30d': { label: 'Last 30 days', days: 30 },
  '90d': { label: 'Last 90 days', days: 90 },
} as const
type Period = keyof typeof PERIODS

/** "Today" = since local midnight; "Last N days" = the last N × 24 h before `now` (when the list was loaded). */
function periodStart(period: Period, now: number): number {
  const days = PERIODS[period].days
  return days === 0 ? new Date(now).setHours(0, 0, 0, 0) : now - days * DAY
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
}

interface CaseListCardProps {
  tabs: readonly [CaseTab, ...CaseTab[]]
  /** "cards": count cards that filter (a queue to work through); "tabs": underline tabs (a personal list). */
  variant?: 'cards' | 'tabs'
  showRequester?: boolean
  showTechnician?: boolean
  timeColumn?: 'reported' | 'planned'
}

/**
 * Case list with its filter, shared by the customer, staff, manager and technician screens.
 * The selected filter lives in the URL (?tab=...) so "Back" from a case returns to the same filter.
 * Counts are computed from the rows on every render.
 * Meeting 07/11: every list can also be filtered by status, urgency and when it was reported, and sorted;
 * those choices live in the URL too (?status=&urgency=&period=&sort=). Tab counts ignore them.
 */
export function CaseListCard({
  tabs,
  variant = 'tabs',
  showRequester,
  showTechnician,
  timeColumn,
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
  const rawPeriod = params.get('period')
  const period: Period | '' = isKey(PERIODS, rawPeriod) ? rawPeriod : ''
  const rawSort = params.get('sort')
  const sort: SortKey | '' = isKey(SORTS, rawSort) ? rawSort : ''
  const filtered = status !== '' || urgency !== '' || period !== ''

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
  const since = period === '' ? null : periodStart(period, cases.dataUpdatedAt)
  const rows = inTab.filter(
    (c) =>
      (status === '' || c.status === status) &&
      (urgency === '' || c.urgency === urgency) &&
      (since === null || new Date(c.submittedAt).getTime() >= since),
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
          {tabs.map((t) => {
            const inTab = all.filter(t.match)
            const overdue = inTab.filter((c) => isOverdue(c)).length
            const note = t.note?.(inTab)
            const selected = t.value === current.value
            return (
              <button
                key={t.value}
                type="button"
                role="tab"
                aria-selected={selected}
                onClick={() => select(t.value)}
                className={cn(
                  'rounded-card bg-surface relative isolate min-w-0 border px-3 py-2.5 text-left',
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
                <div className="text-muted truncate text-[13px] leading-5">{t.label}</div>
                <div className="flex items-baseline gap-2">
                  <span className="text-section font-semibold tabular-nums">{inTab.length}</span>
                  {overdue > 0 && (
                    <span className="text-negative truncate text-xs tabular-nums">
                      {overdue} overdue
                    </span>
                  )}
                </div>
                {note && <div className="text-muted mt-0.5 truncate text-xs">{note}</div>}
              </button>
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
        <FilterSelect
          label="Reported"
          value={period}
          onChange={(e) => setParam('period', e.target.value)}
        >
          <option value="">Any time</option>
          {Object.entries(PERIODS).map(([key, p]) => (
            <option key={key} value={key}>
              {p.label}
            </option>
          ))}
        </FilterSelect>

        {filtered && (
          <>
            <span className="text-muted ml-1 text-sm tabular-nums">
              {rows.length} of {inTab.length}
            </span>
            <Button
              variant="ghost"
              onClick={() =>
                setParams(
                  (prev) => {
                    const next = new URLSearchParams(prev)
                    for (const key of ['status', 'urgency', 'period']) next.delete(key)
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

      {/* A new filter swaps the rows with a 150ms fade (opacity only) instead of a flash. */}
      <motion.div
        key={[current.value, status, urgency, period, sort].join('|')}
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
