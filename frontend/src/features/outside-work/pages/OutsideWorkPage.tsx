import { Link, useSearchParams } from 'react-router'
import { caseDetailPath } from '@/app/paths'
import { ReplacementTable, useReplacements } from '@/features/assets'
import {
  OUTSIDE_KIND_LABELS,
  OUTSIDE_STATUS_LABELS,
  StatusBadge,
  useCases,
  type CaseSummary,
} from '@/features/cases'
import { EmptyBlock, ErrorBlock, LoadingBlock, PageHeader, Tabs } from '@/shared/ui'
import { formatDateTime, formatRelative } from '@/shared/utils/format'

const waitingDecision = (c: CaseSummary) =>
  c.status === 'on_hold' && c.holdReason === 'waiting_staff_decision'
const withParty = (c: CaseSummary) =>
  c.status === 'on_hold' &&
  (c.outsideWork?.status === 'working' || c.outsideWork?.status === 'failed_check')
const waitingQuote = (c: CaseSummary) =>
  c.status === 'on_hold' &&
  (c.outsideWork?.status === 'quoting' || c.outsideWork?.status === 'awaiting_decision')

const TABS = [
  { value: 'decision', label: 'Waiting for a decision', match: waitingDecision },
  { value: 'outside', label: 'With an outside party', match: withParty },
  { value: 'quote', label: 'Quotes', match: waitingQuote },
  { value: 'replacements', label: 'Replacements', match: () => false },
] as const

/**
 * F5 board (R01, staff and managers of the buildings; managers read only): what waits for a repair-or-replace
 * decision, what an outside party is doing, which quotes wait for an answer, and replaced assets the
 * Administrator still has to register. The work itself happens on the case page.
 */
export function OutsideWorkPage() {
  const cases = useCases()
  const replacements = useReplacements()
  const [params, setParams] = useSearchParams()
  const tab = TABS.find((t) => t.value === params.get('tab')) ?? TABS[0]
  const data = cases.data ?? []
  const openReplacements = (replacements.data ?? []).filter((r) => r.status === 'open')
  const count = (t: (typeof TABS)[number]) =>
    t.value === 'replacements' ? openReplacements.length : data.filter(t.match).length

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title="Outside work"
        description={
          cases.data &&
          `${data.filter(waitingDecision).length} waiting for a decision, ${data.filter(withParty).length} with an outside party.`
        }
      />
      <Tabs
        items={TABS.map((t) => ({ value: t.value, label: t.label, count: count(t) }))}
        value={tab.value}
        onChange={(v) => setParams({ tab: v }, { replace: true })}
      />
      {tab.value === 'replacements' ? (
        replacements.isPending ? (
          <LoadingBlock rows={3} />
        ) : replacements.error ? (
          <ErrorBlock
            what="the replacements"
            message={replacements.error.message}
            onRetry={() => void replacements.refetch()}
          />
        ) : (
          <ReplacementTable
            rows={replacements.data}
            emptyText="No replaced asset waits for the Administrator."
          />
        )
      ) : cases.isPending ? (
        <LoadingBlock rows={4} />
      ) : cases.error ? (
        <ErrorBlock
          what="the cases"
          message={cases.error.message}
          onRetry={() => void cases.refetch()}
        />
      ) : (
        <CaseRows rows={data.filter(tab.match)} />
      )}
    </div>
  )
}

function CaseRows({ rows }: { rows: CaseSummary[] }) {
  if (rows.length === 0) return <EmptyBlock>Nothing here.</EmptyBlock>
  return (
    <table className="w-full text-left text-sm">
      <thead>
        <tr className="border-border text-muted border-b text-xs">
          <th className="h-9 pr-4 font-normal">Case</th>
          <th className="h-9 pr-4 font-normal">Outside party</th>
          <th className="hidden h-9 pr-4 font-normal md:table-cell">Technician</th>
          <th className="h-9 text-right font-normal">Status</th>
        </tr>
      </thead>
      <tbody>
        {rows.map((c) => (
          <tr key={c.id} className="border-rule border-b align-top last:border-0">
            <td className="max-w-[320px] py-3 pr-4">
              <Link
                to={caseDetailPath(c.id)}
                className="block truncate font-medium underline-offset-2 hover:underline"
              >
                {c.title}
              </Link>
              <div className="text-muted truncate text-[13px]">
                <span className="font-mono text-xs">{c.code}</span> · {c.location.roomName},{' '}
                {c.location.buildingName} · {formatRelative(c.submittedAt)}
              </div>
            </td>
            <td className="py-3 pr-4">
              {c.outsideWork ? (
                <>
                  {c.outsideWork.companyName}
                  <div className="text-muted text-[13px]">
                    {OUTSIDE_KIND_LABELS[c.outsideWork.kind]} ·{' '}
                    {OUTSIDE_STATUS_LABELS[c.outsideWork.status]}
                    {c.outsideWork.appointmentAt &&
                      ` · ${formatDateTime(c.outsideWork.appointmentAt)}`}
                  </div>
                </>
              ) : (
                <span className="text-muted">—</span>
              )}
            </td>
            <td className="hidden py-3 pr-4 md:table-cell">
              {c.technician?.fullName ?? <span className="text-muted">—</span>}
            </td>
            <td className="py-3 text-right">
              <StatusBadge status={c.status} />
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
