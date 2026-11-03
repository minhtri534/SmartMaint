import type { ReactNode } from 'react'
import { useSearchParams } from 'react-router'
import { useAuth } from '@/features/auth'
import {
  Alert,
  Button,
  Card,
  CardHeader,
  EmptyBlock,
  ErrorBlock,
  LoadingBlock,
  PageHeader,
  Select,
  Tabs,
  TextField,
} from '@/shared/ui'
import { formatDate } from '@/shared/utils/format'
import { BarList, WeekBars } from '../components/Bars'
import { StatTile } from '../components/StatTile'
import { change, hours, minutes, percent } from '../format'
import { useExport, useFailures, useOverview, useTechnicianStats } from '../hooks'
import type { AnalyticsFilters, ExportReport, Overview } from '../types'

type Tab = 'overview' | 'technicians' | 'failures' | 'reports'

/**
 * ANA-01..04 (F6, staff and managers of their buildings; read only, BR-12). Staff get the short version: no
 * technician ranking, no ratings (S01). The period and building live in the URL so a link shows the same numbers.
 */
export function AnalyticsPage() {
  const { user } = useAuth()
  const isManager = user?.role === 'manager'
  const [params, setParams] = useSearchParams()
  const filters: AnalyticsFilters = {
    from: params.get('from') || undefined,
    to: params.get('to') || undefined,
    buildingId: params.get('building') ? Number(params.get('building')) : undefined,
  }
  const tabs: { value: Tab; label: string }[] = [
    { value: 'overview', label: 'Overview' },
    ...(isManager ? [{ value: 'technicians' as const, label: 'Technicians' }] : []),
    { value: 'failures', label: 'Failures and assets' },
    { value: 'reports', label: 'Reports' },
  ]
  const tab = tabs.find((t) => t.value === params.get('tab'))?.value ?? 'overview'
  const set = (key: string, value: string) => {
    const next = new URLSearchParams(params)
    if (value) next.set(key, value)
    else next.delete(key)
    setParams(next, { replace: true })
  }
  // The building list comes from the unfiltered overview (the caller's buildings).
  const buildings = useOverview({ from: filters.from, to: filters.to }).data?.byBuilding ?? []

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title="Analytics"
        description="How maintenance performs in your buildings. Merged reports and surveys are not counted."
        actions={
          <Button variant="secondary" onClick={() => window.print()}>
            Print / PDF
          </Button>
        }
      />
      <div className="flex flex-wrap items-end gap-3 print:hidden">
        <TextField
          label="From"
          type="date"
          value={filters.from ?? ''}
          onChange={(e) => set('from', e.target.value)}
          className="w-40"
        />
        <TextField
          label="To"
          type="date"
          value={filters.to ?? ''}
          onChange={(e) => set('to', e.target.value)}
          className="w-40"
        />
        {buildings.length > 1 && (
          <Select
            label="Building"
            value={filters.buildingId ? String(filters.buildingId) : ''}
            onChange={(e) => set('building', e.target.value)}
            className="w-56"
          >
            <option value="">All my buildings</option>
            {buildings.map((b) => (
              <option key={b.buildingId} value={b.buildingId}>
                {b.name}
              </option>
            ))}
          </Select>
        )}
      </div>
      <div className="print:hidden">
        <Tabs items={tabs} value={tab} onChange={(v) => set('tab', v)} />
      </div>
      {tab === 'overview' && <OverviewTab filters={filters} isManager={isManager} />}
      {tab === 'technicians' && isManager && <TechniciansTab filters={filters} />}
      {tab === 'failures' && <FailuresTab filters={filters} />}
      {tab === 'reports' && <ReportsTab filters={filters} isManager={isManager} />}
    </div>
  )
}

// ----- ANA-01 -----

function OverviewTab({ filters, isManager }: { filters: AnalyticsFilters; isManager: boolean }) {
  const q = useOverview(filters)
  if (q.isPending) return <LoadingBlock rows={6} />
  if (q.error)
    return (
      <ErrorBlock what="the numbers" message={q.error.message} onRetry={() => void q.refetch()} />
    )
  return <OverviewView o={q.data} isManager={isManager} />
}

/** Exported for the Manager home (MGR-01 uses the same numbers). */
export function KpiTiles({ o, isManager }: { o: Overview; isManager: boolean }) {
  const c = o.current
  const p = o.previous
  return (
    <div className="grid grid-cols-2 gap-2 md:grid-cols-3 xl:grid-cols-6">
      <StatTile
        label="Reported"
        value={c.reported}
        change={change(c.reported, p.reported, 'down', 'number')}
        hint={`${c.completed} completed`}
      />
      <StatTile
        label="First response"
        value={minutes(c.avgFirstResponseMinutes)}
        change={change(c.avgFirstResponseMinutes, p.avgFirstResponseMinutes, 'down', 'minutes')}
        hint="report → decision"
      />
      <StatTile
        label="Resolution"
        value={hours(c.avgResolutionHours)}
        change={change(c.avgResolutionHours, p.avgResolutionHours, 'down', 'hours')}
        hint="report → done"
      />
      <StatTile
        label="On time"
        value={percent(c.onTimeRate)}
        change={change(c.onTimeRate, p.onTimeRate, 'up', 'rate')}
        hint={`${percent(c.onTimeOriginalRate)} by the first deadline`}
      />
      <StatTile
        label="Reopened"
        value={percent(c.reopenRate)}
        change={change(c.reopenRate, p.reopenRate, 'down', 'rate')}
        hint={`${c.deadlineMoves} deadline moves`}
      />
      {isManager ? (
        <StatTile
          label="Rating"
          value={c.avgRating === null ? '—' : `${c.avgRating} / 5`}
          change={change(c.avgRating, p.avgRating, 'up', 'number')}
          hint={`${c.technicianDeclines} jobs declined`}
        />
      ) : (
        <StatTile label="Jobs declined" value={c.technicianDeclines} hint="by technicians" />
      )}
    </div>
  )
}

function OverviewView({ o, isManager }: { o: Overview; isManager: boolean }) {
  return (
    <div className="flex flex-col gap-5">
      <p className="text-muted text-sm">
        {formatDate(o.period.from)} – {formatDate(o.period.to)}, compared with{' '}
        {formatDate(o.previousPeriod.from)} – {formatDate(o.previousPeriod.to)}.
      </p>
      <KpiTiles o={o} isManager={isManager} />

      <div className="grid gap-5 lg:grid-cols-2">
        <Card>
          <CardHeader title="Cases per week" />
          <div className="grid gap-5 sm:grid-cols-2">
            <WeekBars
              title="Reported"
              weeks={o.weekly.map((w) => ({ weekStart: w.weekStart, value: w.reported }))}
            />
            <WeekBars
              title="Completed"
              weeks={o.weekly.map((w) => ({ weekStart: w.weekStart, value: w.completed }))}
            />
          </div>
        </Card>
        <Card>
          <CardHeader title="Emergencies and preventive work" />
          <dl className="grid grid-cols-2 gap-x-6 gap-y-3 text-sm">
            <Row label="Emergencies">{o.emergency.count}</Row>
            <Row label="Acknowledged after">{minutes(o.emergency.avgAcknowledgeMinutes)}</Row>
            <Row label="Made safe after">{minutes(o.emergency.avgMadeSafeMinutes)}</Row>
            <Row label="Fixed after">{hours(o.emergency.avgFixHours)}</Row>
            <Row label="Downgraded to F3">{o.emergency.downgraded}</Row>
            <Row label="Merged reports">{o.emergency.merged}</Row>
            <Row label="Preventive due">{o.preventive.due}</Row>
            <Row label="Done on time">{percent(o.preventive.onTimeRate)}</Row>
            <Row label="Inspected">{o.preventive.inspected}</Row>
            <Row label="No failed item">{percent(o.preventive.passedRate)}</Row>
          </dl>
        </Card>
      </div>

      <div className="grid gap-5 lg:grid-cols-2">
        <Card>
          <CardHeader
            title="Time on hold"
            aside={
              o.avgOutsideHours === null
                ? 'no outside work finished'
                : `outside party ${hours(o.avgOutsideHours)} on average`
            }
          />
          {o.holdTime.length === 0 ? (
            <EmptyBlock>No case was put on hold.</EmptyBlock>
          ) : (
            <BarList
              unit="hours"
              rows={o.holdTime.map((h) => ({
                key: h.reason,
                label: h.reason,
                value: h.totalHours,
                valueText: hours(h.totalHours),
                detail: `${h.times}×, ${hours(h.avgHours)} each on average`,
              }))}
            />
          )}
        </Card>
        <Card>
          <CardHeader title="By building" />
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-border text-muted border-b text-xs">
                <th className="h-9 pr-3 font-normal">Building</th>
                <th className="h-9 pr-3 text-right font-normal">Reported</th>
                <th className="h-9 pr-3 text-right font-normal">On time</th>
                <th className="h-9 pr-3 text-right font-normal">Open</th>
                <th className="h-9 text-right font-normal">Overdue</th>
              </tr>
            </thead>
            <tbody className="tabular-nums">
              {o.byBuilding.map((b) => (
                <tr key={b.buildingId} className="border-rule border-b last:border-0">
                  <td className="py-2.5 pr-3">{b.name}</td>
                  <td className="py-2.5 pr-3 text-right">{b.reported}</td>
                  <td className="py-2.5 pr-3 text-right">{percent(b.onTimeRate)}</td>
                  <td className="py-2.5 pr-3 text-right">{b.openNow}</td>
                  <td
                    className={
                      b.overdueNow > 0 ? 'text-negative py-2.5 text-right' : 'py-2.5 text-right'
                    }
                  >
                    {b.overdueNow}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </Card>
      </div>
    </div>
  )
}

// ----- ANA-02 -----

function TechniciansTab({ filters }: { filters: AnalyticsFilters }) {
  const q = useTechnicianStats(filters, true)
  if (q.isPending) return <LoadingBlock rows={5} />
  if (q.error)
    return (
      <ErrorBlock
        what="the technicians"
        message={q.error.message}
        onRetry={() => void q.refetch()}
      />
    )
  if (q.data.length === 0) return <EmptyBlock>No technician worked in this period.</EmptyBlock>
  return (
    <Card padding="none" className="overflow-x-auto">
      <table className="w-full min-w-[760px] text-left text-sm">
        <thead>
          <tr className="border-border text-muted border-b text-xs">
            <th className="h-10 px-4 font-normal">Technician</th>
            <th className="h-10 pr-4 text-right font-normal">Completed</th>
            <th className="h-10 pr-4 text-right font-normal">Avg time</th>
            <th className="h-10 pr-4 text-right font-normal">On time</th>
            <th className="h-10 pr-4 text-right font-normal">Reopened</th>
            <th className="h-10 pr-4 text-right font-normal">Rating</th>
            <th className="h-10 pr-4 text-right font-normal">Labour</th>
            <th className="h-10 pr-4 text-right font-normal">Open now</th>
            <th className="h-10 pr-4 font-normal">Declined</th>
          </tr>
        </thead>
        <tbody className="tabular-nums">
          {q.data.map((t) => (
            <tr key={t.technician.id} className="border-rule border-b align-top last:border-0">
              <td className="px-4 py-3">
                {t.technician.fullName}
                {t.fewCases && (
                  <div className="text-muted text-xs">Under 5 cases: not reliable yet</div>
                )}
              </td>
              <td className="py-3 pr-4 text-right">{t.completed}</td>
              <td className="py-3 pr-4 text-right">{hours(t.avgCompletionHours)}</td>
              <td className="py-3 pr-4 text-right">{percent(t.onTimeRate)}</td>
              <td className="py-3 pr-4 text-right">{percent(t.reopenRate)}</td>
              <td className="py-3 pr-4 text-right">{t.avgRating ?? '—'}</td>
              <td className="py-3 pr-4 text-right">{t.laborHours} h</td>
              <td className="py-3 pr-4 text-right">{t.openNow}</td>
              <td className="max-w-[240px] py-3 pr-4">
                {t.declines}
                {t.declineReasons.length > 0 && (
                  <div className="text-muted truncate text-xs" title={t.declineReasons.join(' · ')}>
                    {t.declineReasons[0]}
                  </div>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </Card>
  )
}

// ----- ANA-03 -----

function FailuresTab({ filters }: { filters: AnalyticsFilters }) {
  const q = useFailures(filters)
  if (q.isPending) return <LoadingBlock rows={5} />
  if (q.error)
    return (
      <ErrorBlock what="the failures" message={q.error.message} onRetry={() => void q.refetch()} />
    )
  const f = q.data
  return (
    <div className="flex flex-col gap-5">
      <div className="grid gap-5 lg:grid-cols-2">
        <Card>
          <CardHeader title="Cases by service type" />
          {f.byServiceType.length === 0 ? (
            <EmptyBlock>No case in this period.</EmptyBlock>
          ) : (
            <BarList
              unit="cases"
              rows={f.byServiceType.slice(0, 10).map((s) => ({
                key: s.id,
                label: s.name,
                value: s.cases,
                detail:
                  s.avgResolutionHours === null
                    ? undefined
                    : `${hours(s.avgResolutionHours)} to fix`,
              }))}
            />
          )}
        </Card>
        <Card>
          <CardHeader title="Cases by group" />
          {f.byGroup.length === 0 ? (
            <EmptyBlock>No case in this period.</EmptyBlock>
          ) : (
            <BarList
              unit="cases"
              rows={f.byGroup.map((g) => ({ key: g.group, label: g.group, value: g.cases }))}
            />
          )}
        </Card>
      </div>
      <Card>
        <CardHeader title="Problem assets" aside="most repaired first" />
        {f.problemAssets.length === 0 ? (
          <EmptyBlock>No asset was repaired in this period.</EmptyBlock>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-border text-muted border-b text-xs">
                <th className="h-9 pr-4 font-normal">Asset</th>
                <th className="h-9 pr-4 text-right font-normal">Repairs</th>
                <th className="h-9 pr-4 text-right font-normal">Last 30 days</th>
                <th className="hidden h-9 pr-4 text-right font-normal md:table-cell">Age</th>
                <th className="hidden h-9 text-right font-normal md:table-cell">Warranty</th>
              </tr>
            </thead>
            <tbody className="tabular-nums">
              {f.problemAssets.map((a) => (
                <tr key={a.id} className="border-rule border-b align-top last:border-0">
                  <td className="py-2.5 pr-4">
                    {a.name}
                    <div className="text-muted text-[13px]">
                      {a.typeName} · {a.location}
                    </div>
                  </td>
                  <td className="py-2.5 pr-4 text-right">{a.repairs}</td>
                  <td className="py-2.5 pr-4 text-right">{a.repairsLast30Days}</td>
                  <td className="hidden py-2.5 pr-4 text-right md:table-cell">
                    {a.ageYears}
                    {a.expectedLifeYears ? ` / ${a.expectedLifeYears}` : ''} y
                  </td>
                  <td className="hidden py-2.5 text-right md:table-cell">
                    {a.underWarranty ? 'yes' : 'no'}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Card>
    </div>
  )
}

// ----- ANA-04 -----

const REPORTS: { report: ExportReport; title: string; text: string; managerOnly?: boolean }[] = [
  {
    report: 'cases',
    title: 'Case summary',
    text: 'Every case reported in the period, with its deadline and result.',
  },
  {
    report: 'assets',
    title: 'Asset failures',
    text: 'The most repaired assets, their age and warranty.',
  },
  {
    report: 'preventive',
    title: 'Preventive compliance',
    text: 'Work orders due in the period: on time, failed items.',
  },
  {
    report: 'technicians',
    title: 'Technician performance',
    text: 'Completed, on time, reopened, rating, labour, declines.',
    managerOnly: true,
  },
]

function ReportsTab({ filters, isManager }: { filters: AnalyticsFilters; isManager: boolean }) {
  const download = useExport()
  return (
    <div className="flex flex-col gap-3">
      <p className="text-muted text-sm">
        CSV files open in Excel. For a PDF, open a tab and use Print / PDF.
      </p>
      <div className="grid gap-3 md:grid-cols-2">
        {REPORTS.filter((r) => !r.managerOnly || isManager).map((r) => (
          <Card key={r.report} className="flex items-start justify-between gap-4">
            <div>
              <div className="font-medium">{r.title}</div>
              <p className="text-muted mt-0.5 text-sm">{r.text}</p>
            </div>
            <Button
              variant="secondary"
              loading={download.isPending && download.variables?.report === r.report}
              onClick={() => download.mutate({ report: r.report, filters })}
            >
              Download CSV
            </Button>
          </Card>
        ))}
      </div>
      {download.error && <Alert>{download.error.message}</Alert>}
    </div>
  )
}

function Row({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="flex items-baseline justify-between gap-3">
      <dt className="text-muted">{label}</dt>
      <dd className="text-right tabular-nums">{children}</dd>
    </div>
  )
}
