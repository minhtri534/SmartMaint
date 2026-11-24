import { useState } from 'react'
import { useSearchParams } from 'react-router'
import { useAuth } from '@/features/auth'
import {
  Alert,
  Button,
  Card,
  EmptyBlock,
  ErrorBlock,
  FilterDate,
  FilterSelect,
  Gauge,
  KpiCard,
  LoadingBlock,
  MiniStat,
  PageHeader,
  PanelHeader,
  PerformanceChart,
  Rise,
  Segmented,
  SortHeader,
  sortRows,
  Tabs,
  type KpiCardProps,
  type SortState,
} from '@/shared/ui'
import { formatDate, formatVnd } from '@/shared/utils/format'
import { AttentionList } from '../components/AttentionList'
import { BarList } from '../components/Bars'
import { ColumnChart } from '../components/ColumnChart'
import { PieChart } from '../components/PieChart'
import { hours, minutes, percent } from '../format'
import { useChart, useExport, useFailures, useOverview, useTechnicianStats } from '../hooks'
import type {
  AnalyticsFilters,
  Chart,
  ChartMetric,
  ChartStep,
  ExportTab,
  Overview,
  TechnicianRow,
} from '../types'

type Tab = 'overview' | 'charts' | 'technicians' | 'failures' | 'attention'

const EXPORT_TAB: Record<Tab, ExportTab> = {
  overview: 'Overview',
  charts: 'Charts',
  technicians: 'Technicians',
  failures: 'Failures',
  attention: 'Attention',
}

/** yyyy-MM-dd of a local date. */
function isoDay(d: Date) {
  return new Date(d.getTime() - d.getTimezoneOffset() * 60_000).toISOString().slice(0, 10)
}

/** From the same day `months` months (or `days` days) ago to today. Called from event handlers and state setup only. */
function rangeBack(months: number, days = 0) {
  const from = new Date()
  from.setMonth(from.getMonth() - months)
  from.setDate(from.getDate() - days)
  return { from: isoDay(from), to: isoDay(new Date()) }
}

/**
 * ANA-01..04 (F6, managers of their buildings; read only, BR-12). Meeting note 18/11: the period is From – To (by
 * default a month back to today) and every figure is a total or an average over it, with no comparison; a Charts tab;
 * Export saves the tab on screen as an Excel file. The choices live in the URL so a link shows the same numbers.
 */
export function AnalyticsPage() {
  const { user } = useAuth()
  const isManager = user?.role === 'manager'
  const [params, setParams] = useSearchParams()
  const [defaults] = useState(() => rangeBack(1))
  const filters: AnalyticsFilters = {
    from: params.get('from') || defaults.from,
    to: params.get('to') || defaults.to,
    buildingId: params.get('building') ? Number(params.get('building')) : undefined,
  }
  const tabs: { value: Tab; label: string }[] = [
    { value: 'overview', label: 'Overview' },
    ...(isManager
      ? [
          { value: 'charts' as const, label: 'Charts' },
          { value: 'technicians' as const, label: 'Technicians' },
        ]
      : []),
    { value: 'failures', label: 'Failures and assets' },
    { value: 'attention', label: 'Needs attention' },
  ]
  const tab = tabs.find((t) => t.value === params.get('tab'))?.value ?? 'overview'
  const set = (patch: Record<string, string>) => {
    const next = new URLSearchParams(params)
    for (const [key, value] of Object.entries(patch)) {
      if (value) next.set(key, value)
      else next.delete(key)
    }
    setParams(next, { replace: true })
  }
  // The building list comes from the overview of all the caller's buildings.
  const scope = useOverview({ from: filters.from, to: filters.to })
  const buildings = scope.data?.byBuilding ?? []
  const exportFile = useExport()
  const chartChoice = chartChoiceOf(params)
  const technicianSort = technicianSortOf(params)
  // The Charts tab's window: the button last pressed; a typed From or To reads as no window.
  const windowValue = params.get('window') ?? (params.get('from') || params.get('to') ? '' : '1m')

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title="Analytics"
        description={`${formatDate(filters.from!)} – ${formatDate(filters.to!)}. Duplicates and asset surveys are not counted.`}
        actions={
          <>
            <div className="flex flex-wrap items-center gap-2">
              <FilterDate
                label="From"
                value={filters.from!}
                max={filters.to}
                onChange={(v) => set({ from: v, window: '' })}
              />
              <FilterDate
                label="To"
                value={filters.to!}
                min={filters.from}
                onChange={(v) => set({ to: v, window: '' })}
              />
              {buildings.length > 1 && (
                <FilterSelect
                  label="Building"
                  value={filters.buildingId ? String(filters.buildingId) : ''}
                  onChange={(e) => set({ building: e.target.value })}
                >
                  <option value="">All</option>
                  {buildings.map((b) => (
                    <option key={b.buildingId} value={b.buildingId}>
                      {b.name}
                    </option>
                  ))}
                </FilterSelect>
              )}
            </div>
            <Button
              variant="secondary"
              loading={exportFile.isPending}
              onClick={() =>
                exportFile.mutate({
                  ...filters,
                  tab: EXPORT_TAB[tab],
                  metric: chartChoice.metric,
                  step: chartChoice.step,
                  sort: technicianSort?.key,
                  descending: technicianSort?.dir === 'desc',
                })
              }
            >
              Export
            </Button>
          </>
        }
      />
      {exportFile.error && <Alert>{exportFile.error.message}</Alert>}
      <Tabs items={tabs} value={tab} onChange={(v) => set({ tab: v })} />
      {tab === 'overview' && <OverviewTab filters={filters} isManager={isManager} />}
      {tab === 'charts' && isManager && (
        <ChartsTab filters={filters} choice={chartChoice} windowValue={windowValue} set={set} />
      )}
      {tab === 'technicians' && isManager && (
        <TechniciansTab filters={filters} sort={technicianSort} set={set} />
      )}
      {tab === 'failures' && <FailuresTab filters={filters} />}
      {tab === 'attention' && <AttentionList />}
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
  // While a new period loads, the old numbers stay, dimmed.
  return (
    <div className={q.isPlaceholderData ? 'opacity-60 transition-opacity' : 'transition-opacity'}>
      <OverviewView o={q.data} isManager={isManager} />
    </div>
  )
}

const HELP = {
  reported: 'Requests reported in the period. Duplicates and asset surveys are not counted.',
  firstResponse:
    "Average time from a report to the staff's first decision (approve or reject). For an emergency: from the moment it became one to the staff acknowledging it.",
  resolution:
    'Average time from the start of work (the technician sets In progress; an emergency: the moment it became one) to the technician reporting it done. Waiting before the work starts is not counted.',
  onTime:
    'Share of deadlines met: cases reported done by their deadline, out of those done in the period plus those still open whose deadline passed in the period.',
  reopened: 'Share of the cases done in the period that were reopened at least once.',
  rating: 'Average of the stars customers gave to the cases they rated in the period.',
  declined: 'How many times technicians declined a job in the period.',
}

/**
 * Exported for the Manager home (MGR-01 uses the same numbers). Meeting note 18/11: totals and averages of the period,
 * no comparison with the one before; each tile explains itself behind its "?". `compact` keeps the ones the home shows.
 */
export function KpiTiles({
  o,
  isManager,
  compact = false,
}: {
  o: Overview
  isManager: boolean
  compact?: boolean
}) {
  const c = o.current
  const cards: (Omit<KpiCardProps, 'index'> & { full?: boolean })[] = [
    { label: 'Reported', value: c.reported, icon: 'requests', hue: 'orange', help: HELP.reported },
    {
      label: 'First response',
      value: minutes(c.avgFirstResponseMinutes),
      icon: 'clock',
      hue: 'blue',
      hint: 'report → first decision',
      help: HELP.firstResponse,
    },
    {
      label: 'Resolution',
      value: hours(c.avgResolutionHours),
      icon: 'wrench',
      hue: 'violet',
      hint: 'start of work → done',
      help: HELP.resolution,
    },
    {
      label: 'On time',
      value: percent(c.onTimeRate),
      icon: 'checklist',
      hue: 'green',
      help: HELP.onTime,
      full: true,
    },
    {
      label: 'Reopened',
      value: percent(c.reopenRate),
      icon: 'swap',
      hue: 'red',
      help: HELP.reopened,
      full: true,
    },
    ...(isManager
      ? [
          {
            label: 'Rating',
            value: c.avgRating === null ? '—' : `${c.avgRating.toFixed(1)} / 5`,
            icon: 'users' as const,
            hue: 'green' as const,
            hint:
              c.ratings === 0
                ? 'no rating yet'
                : `average stars of ${c.ratings} ${c.ratings === 1 ? 'rating' : 'ratings'}`,
            help: HELP.rating,
          },
        ]
      : []),
    {
      label: 'Jobs declined',
      value: c.technicianDeclines,
      icon: 'swap',
      hue: 'blue',
      hint: 'by technicians',
      help: HELP.declined,
    },
  ]
  const shown = compact ? cards.filter((k) => !k.full) : cards
  return (
    <div
      className={
        compact
          ? 'grid grid-cols-2 gap-3 sm:grid-cols-3 sm:gap-5 xl:grid-cols-5'
          : 'grid grid-cols-2 gap-3 sm:gap-5 xl:grid-cols-4'
      }
    >
      {shown.map(({ full: _full, ...k }, i) => (
        <KpiCard key={k.label} index={i} {...k} />
      ))}
    </div>
  )
}

function OverviewView({ o, isManager }: { o: Overview; isManager: boolean }) {
  const holdTotal = o.holdTime.reduce((n, h) => n + h.totalHours, 0)
  return (
    <div className="flex flex-col gap-5">
      <KpiTiles o={o} isManager={isManager} />

      <div className="grid items-stretch gap-5 xl:grid-cols-[minmax(0,2fr)_minmax(0,1fr)]">
        <Rise index={6}>
          <Card className="h-full">
            <PanelHeader
              title="Cases per week"
              help="Each week of the period: the cases reported (bars) and the cases reported done (line)."
            />
            <PerformanceChart
              data={o.weekly.map((w) => {
                const label = new Date(w.weekStart).toLocaleDateString('en-GB', {
                  day: 'numeric',
                  month: 'short',
                })
                return { label, title: `Week of ${label}`, bar: w.reported, line: w.completed }
              })}
              barName="Reported"
              lineName="Completed"
            />
          </Card>
        </Rise>
        <Rise index={7}>
          <Card className="flex h-full flex-col">
            <PanelHeader title="On time" help={HELP.onTime} />
            <div className="flex flex-1 items-center justify-center">
              <Gauge value={o.current.onTimeRate} caption="finished by the deadline" />
            </div>
            <div className="mt-4 grid grid-cols-2 gap-3">
              <MiniStat label="Completed" value={o.current.completed} />
              <MiniStat label="Reopened" value={percent(o.current.reopenRate)} />
            </div>
          </Card>
        </Rise>
      </div>

      <div className="grid gap-5 lg:grid-cols-2">
        <Card>
          <PanelHeader
            title="Time on hold"
            subtitle={
              o.avgOutsideHours === null
                ? 'No outside work finished in the period'
                : `Outside party: ${hours(o.avgOutsideHours)} on average`
            }
            help="Hours cases spent on hold in the period, by why they were put on hold."
          />
          {o.holdTime.length === 0 ? (
            <EmptyBlock>No case was put on hold.</EmptyBlock>
          ) : (
            <PieChart
              unit="hours on hold"
              totalText={hours(holdTotal)}
              slices={o.holdTime.map((h) => ({
                key: h.reason,
                label: `${h.reason} (${h.times}×)`,
                value: h.totalHours,
                valueText: hours(h.totalHours),
              }))}
            />
          )}
        </Card>
        <Card>
          <PanelHeader
            title="By building"
            help="Reported and on time in the period; open and overdue are counted now."
          />
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

// ----- Charts (meeting note 18/11) -----

const TOPICS = {
  cases: {
    label: 'Cases',
    metrics: [
      ['CasesReported', 'Reported'],
      ['CasesCompleted', 'Completed'],
      ['CasesLate', 'Late'],
    ],
  },
  technicians: {
    label: 'Technicians',
    metrics: [
      ['TechnicianCompleted', 'Completed'],
      ['TechnicianOnTime', 'On time'],
      ['TechnicianLabourHours', 'Labour hours'],
    ],
  },
  staff: {
    label: 'Staff',
    metrics: [
      ['StaffDecisions', 'Requests decided'],
      ['StaffFirstResponse', 'First response time'],
    ],
  },
  costs: {
    label: 'Costs',
    metrics: [
      ['CostTotal', 'By building'],
      ['CostByFault', 'By fault category'],
    ],
  },
} as const satisfies Record<
  string,
  { label: string; metrics: readonly (readonly [ChartMetric, string])[] }
>
type Topic = keyof typeof TOPICS

const STEPS: { value: ChartStep; label: string }[] = [
  { value: 'Day', label: 'Day' },
  { value: 'Week', label: 'Week' },
  { value: 'Month', label: 'Month' },
]

interface ChartChoice {
  topic: Topic
  metric: ChartMetric
  step: ChartStep
}

function chartChoiceOf(params: URLSearchParams): ChartChoice {
  const topic = (Object.keys(TOPICS) as Topic[]).find((t) => t === params.get('topic')) ?? 'cases'
  const metrics = TOPICS[topic].metrics as readonly (readonly [ChartMetric, string])[]
  const metric = metrics.find(([m]) => m === params.get('metric'))?.[0] ?? metrics[0]![0]
  const step = STEPS.find((s) => s.value === params.get('step'))?.value ?? 'Week'
  return { topic, metric, step }
}

/** How one value of the chart reads, by its unit. */
function valueText(unit: Chart['unit'], value: number | null) {
  if (value === null) return '—'
  switch (unit) {
    case '%':
      return `${Math.round(value)}%`
    case 'hours':
      return hours(value)
    case 'minutes':
      return minutes(value)
    case 'VND':
      return formatVnd(value)
    default:
      return String(Math.round(value * 10) / 10)
  }
}

function ChartsTab({
  filters,
  choice,
  windowValue,
  set,
}: {
  filters: AnalyticsFilters
  choice: ChartChoice
  windowValue: string
  set: (patch: Record<string, string>) => void
}) {
  const q = useChart({ ...filters, metric: choice.metric, step: choice.step })
  const metrics = TOPICS[choice.topic].metrics as readonly (readonly [ChartMetric, string])[]
  const metricLabel = metrics.find(([m]) => m === choice.metric)?.[1] ?? ''
  // The window buttons set From – To.
  const windows = [
    { value: '1w', label: '1 week', range: () => rangeBack(0, 7) },
    { value: '1m', label: '1 month', range: () => rangeBack(1) },
    { value: '3m', label: '3 months', range: () => rangeBack(3) },
  ]

  return (
    <div className="flex flex-col gap-4">
      <Tabs
        items={(Object.keys(TOPICS) as Topic[]).map((t) => ({ value: t, label: TOPICS[t].label }))}
        value={choice.topic}
        onChange={(t) => set({ topic: t, metric: '' })}
      />
      <div className="flex flex-wrap items-center gap-2">
        <FilterSelect
          label="Show"
          neutral
          value={choice.metric}
          onChange={(e) => set({ metric: e.target.value })}
        >
          {metrics.map(([m, label]) => (
            <option key={m} value={m}>
              {label}
            </option>
          ))}
        </FilterSelect>
        <Segmented
          label="Window"
          value={windowValue}
          onChange={(v) => {
            const w = windows.find((x) => x.value === v)
            if (w) set({ ...w.range(), window: v })
          }}
          options={windows.map((w) => ({ value: w.value, label: w.label }))}
        />
        <FilterSelect
          label="Each"
          neutral
          value={choice.step}
          onChange={(e) => set({ step: e.target.value })}
        >
          {STEPS.map((s) => (
            <option key={s.value} value={s.value}>
              {s.label}
            </option>
          ))}
        </FilterSelect>
      </div>
      {q.isPending ? (
        <LoadingBlock rows={6} />
      ) : q.error ? (
        <ErrorBlock what="the chart" message={q.error.message} onRetry={() => void q.refetch()} />
      ) : (
        <div
          className={
            'grid gap-5 transition-opacity xl:grid-cols-[minmax(0,2fr)_minmax(0,1fr)]' +
            (q.isPlaceholderData ? ' opacity-60' : '')
          }
        >
          <Card>
            <PanelHeader
              title={`${TOPICS[choice.topic].label}: ${metricLabel.toLowerCase()}`}
              subtitle={`Each ${choice.step.toLowerCase()}, ${formatDate(q.data.period.from)} – ${formatDate(q.data.period.to)}`}
            />
            <ColumnChart
              axis={(v) => valueText(q.data.unit, v)}
              columns={q.data.points.map((p) => {
                const day = new Date(p.start)
                const label =
                  choice.step === 'Month'
                    ? day.toLocaleDateString('en-GB', { month: 'short', year: '2-digit' })
                    : day.toLocaleDateString('en-GB', { day: 'numeric', month: 'short' })
                return {
                  key: p.start,
                  label,
                  title: choice.step === 'Week' ? `Week of ${label}` : label,
                  value: p.value,
                  valueText: valueText(q.data.unit, p.value),
                }
              })}
            />
          </Card>
          <Card>
            <PanelHeader
              title={`By ${q.data.breakdownBy.toLowerCase()}`}
              subtitle="The whole period"
            />
            {q.data.breakdown.length === 0 ? (
              <EmptyBlock>Nothing in this period.</EmptyBlock>
            ) : (
              <BarList
                unit={q.data.unit}
                rows={q.data.breakdown.map((b) => ({
                  key: b.name,
                  label: b.name,
                  value: b.value,
                  valueText: valueText(q.data.unit, b.value),
                }))}
              />
            )}
          </Card>
        </div>
      )}
    </div>
  )
}

// ----- ANA-02 -----

type TechnicianKey =
  | 'name'
  | 'completed'
  | 'avgTime'
  | 'onTime'
  | 'reopened'
  | 'rating'
  | 'labour'
  | 'open'
  | 'declined'

const TECHNICIAN_VALUES: Record<TechnicianKey, (t: TechnicianRow) => string | number | null> = {
  name: (t) => t.technician.fullName,
  completed: (t) => t.completed,
  avgTime: (t) => t.avgCompletionHours,
  onTime: (t) => t.onTimeRate,
  reopened: (t) => t.reopenRate,
  rating: (t) => t.avgRating,
  labour: (t) => t.laborHours,
  open: (t) => t.openNow,
  declined: (t) => t.declines,
}

/** The technician table's sort lives in the URL, so Export saves the same order. */
function technicianSortOf(params: URLSearchParams): SortState<TechnicianKey> | null {
  const key = params.get('sort')
  if (!key || !(key in TECHNICIAN_VALUES)) return null
  return { key: key as TechnicianKey, dir: params.get('dir') === 'desc' ? 'desc' : 'asc' }
}

function TechniciansTab({
  filters,
  sort,
  set,
}: {
  filters: AnalyticsFilters
  sort: SortState<TechnicianKey> | null
  set: (patch: Record<string, string>) => void
}) {
  const q = useTechnicianStats(filters, true)
  const onSort = (key: TechnicianKey) =>
    set({ sort: key, dir: sort?.key === key && sort.dir === 'asc' ? 'desc' : 'asc' })
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
  const rows = sortRows(q.data, sort, TECHNICIAN_VALUES)
  const head = (key: TechnicianKey, label: string, align: 'left' | 'right' = 'right') => (
    <SortHeader sortKey={key} sort={sort} onSort={onSort} align={align} className="h-10">
      {label}
    </SortHeader>
  )
  return (
    <Card
      padding="none"
      className={'overflow-x-auto transition-opacity' + (q.isPlaceholderData ? ' opacity-60' : '')}
    >
      <table className="w-full min-w-[760px] text-left text-sm">
        <thead>
          <tr className="border-border text-muted border-b text-xs [&>th:first-child]:pl-4">
            {head('name', 'Technician', 'left')}
            {head('completed', 'Completed')}
            {head('avgTime', 'Avg time')}
            {head('onTime', 'On time')}
            {head('reopened', 'Reopened')}
            {head('rating', 'Rating')}
            {head('labour', 'Labour')}
            {head('open', 'Open now')}
            {head('declined', 'Declined', 'left')}
          </tr>
        </thead>
        <tbody className="tabular-nums">
          {rows.map((t) => (
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
    <div
      className={
        'flex flex-col gap-5 transition-opacity' + (q.isPlaceholderData ? ' opacity-60' : '')
      }
    >
      <div className="grid gap-5 lg:grid-cols-2">
        <Card>
          <PanelHeader
            title="By fault category"
            help="Cases reported in the period, split by fault category. Rejected and cancelled cases are not counted."
          />
          {f.byServiceType.length === 0 ? (
            <EmptyBlock>No case in this period.</EmptyBlock>
          ) : (
            <PieChart
              unit="cases"
              slices={f.byServiceType.map((s) => ({
                key: s.id,
                label: s.name,
                value: s.cases,
              }))}
            />
          )}
        </Card>
        <Card>
          <PanelHeader
            title="By asset type"
            help="The same cases, split by the type of the asset that had the problem."
          />
          {f.byAssetType.length === 0 ? (
            <EmptyBlock>No case in this period.</EmptyBlock>
          ) : (
            <PieChart
              unit="cases"
              slices={f.byAssetType.map((g) => ({ key: g.group, label: g.group, value: g.cases }))}
            />
          )}
        </Card>
      </div>
      <Card>
        <PanelHeader title="Problem assets" subtitle="Most repaired first" />
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
                  <td className="hidden py-2.5 pr-4 text-right md:table-cell">{a.ageYears} y</td>
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
