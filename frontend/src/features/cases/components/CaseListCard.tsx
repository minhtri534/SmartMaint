import { useSearchParams } from 'react-router'
import { Button, Tabs } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { useCases } from '../hooks'
import { isOverdue } from '../sla'
import type { CaseSummary } from '../types'
import { CaseTable } from './CaseTable'

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
}

/**
 * Case list with its filter, shared by the customer, staff, manager and technician screens.
 * The selected filter lives in the URL (?tab=...) so "Back" from a case returns to the same filter.
 * Counts are computed from the rows on every render.
 */
export function CaseListCard({
  tabs,
  variant = 'tabs',
  showRequester,
  showTechnician,
}: CaseListCardProps) {
  const cases = useCases()
  const [params, setParams] = useSearchParams()
  const current = tabs.find((t) => t.value === params.get('tab')) ?? tabs[0]
  const select = (tab: string) => setParams({ tab }, { replace: true })

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
  const rows = all.filter(current.match)
  if (current.sort) rows.sort(current.sort)

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
                  'rounded-card min-w-0 border px-3 py-2.5 text-left transition-colors',
                  selected
                    ? 'border-accent bg-accent-soft'
                    : 'border-border hover:border-control bg-surface',
                )}
              >
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

      <CaseTable
        cases={rows}
        showRequester={showRequester}
        showTechnician={showTechnician}
        emptyText={current.emptyText}
      />
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
