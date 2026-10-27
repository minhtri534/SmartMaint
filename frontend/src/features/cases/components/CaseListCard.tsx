import { useSearchParams } from 'react-router'
import { Alert, Card, Spinner, Tabs } from '@/shared/ui'
import { useCases } from '../hooks'
import type { CaseSummary } from '../types'
import { CaseTable } from './CaseTable'

export interface CaseTab {
  value: string
  label: string
  /** Which cases belong to this tab. */
  match: (c: CaseSummary) => boolean
  emptyText: string
}

interface CaseListCardProps {
  tabs: readonly [CaseTab, ...CaseTab[]]
  showRequester?: boolean
  showTechnician?: boolean
}

/**
 * Case list with filter tabs, shared by the customer, officer, manager and technician lists.
 * The selected tab lives in the URL (?tab=...) so "Back" from a case returns to the same tab.
 */
export function CaseListCard({ tabs, showRequester, showTechnician }: CaseListCardProps) {
  const cases = useCases()
  const [params, setParams] = useSearchParams()
  const current = tabs.find((t) => t.value === params.get('tab')) ?? tabs[0]

  if (cases.isPending) {
    return (
      <div className="text-muted flex justify-center py-16">
        <Spinner className="size-8" />
      </div>
    )
  }
  if (cases.error) return <Alert>{cases.error.message}</Alert>

  return (
    <Card className="p-0">
      <div className="px-4 pt-2">
        <Tabs
          items={tabs.map((t) => ({
            value: t.value,
            label: t.label,
            count: cases.data.filter(t.match).length,
          }))}
          value={current.value}
          onChange={(tab) => setParams({ tab }, { replace: true })}
        />
      </div>
      <div className="p-2">
        <CaseTable
          cases={cases.data.filter(current.match)}
          showRequester={showRequester}
          showTechnician={showTechnician}
          emptyText={current.emptyText}
        />
      </div>
    </Card>
  )
}
