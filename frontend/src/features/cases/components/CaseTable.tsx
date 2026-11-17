import type { CaseSummary } from '../types'
import { CaseCard } from './CaseCard'

interface CaseTableProps {
  cases: CaseSummary[]
  /** Extra details depending on who looks at the list. */
  showRequester?: boolean
  showTechnician?: boolean
  /** 'planned' (technician, meeting 07/11): when the staff planned the work to start, instead of when it was reported. */
  timeColumn?: 'reported' | 'planned'
  emptyText: string
}

/**
 * The case list. ui/mercato-style (17/11): a grid of case cards instead of a table (Leader: the rows and their
 * rules were hard to read and the deadline column was cut off). One column on a phone, two from 768px,
 * three on very wide screens. The name stays CaseTable so the screens that use it do not change.
 */
export function CaseTable({
  cases,
  showRequester,
  showTechnician,
  timeColumn = 'reported',
  emptyText,
}: CaseTableProps) {
  if (cases.length === 0) {
    return (
      <div className="border-control text-muted rounded-card flex min-h-10 items-center justify-center border border-dashed px-4 py-2.5 text-sm">
        {emptyText}
      </div>
    )
  }

  return (
    <ul className="grid gap-4 md:grid-cols-2 2xl:grid-cols-3">
      {cases.map((c, i) => (
        <li key={c.id} className="min-w-0">
          <CaseCard
            c={c}
            index={i}
            showRequester={showRequester}
            showTechnician={showTechnician}
            planned={timeColumn === 'planned'}
          />
        </li>
      ))}
    </ul>
  )
}
