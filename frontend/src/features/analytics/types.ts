/** Mirrors backend Modules/Analytics/Dtos (F6). Rates are 0..1, durations in minutes or hours as named. */

export interface AnalyticsFilters {
  /** yyyy-MM-dd, local dates, both included. Empty = from a month ago to today. */
  from?: string
  to?: string
  buildingId?: number
}

export interface Period {
  from: string
  to: string
}

/**
 * KPIs of one period (incidents F3 / F4). Reports merged into another case and surveys are not counted (N5).
 * onTimeRate: by the deadline in force; onTimeOriginalRate: by the first SLA deadline (S02).
 */
export interface KpiSet {
  reported: number
  rejected: number
  cancelled: number
  completed: number
  avgFirstResponseMinutes: number | null
  avgResolutionHours: number | null
  onTimeRate: number | null
  onTimeOriginalRate: number | null
  extendedCases: number
  deadlineMoves: number
  reopenRate: number | null
  /** Managers only. */
  avgRating: number | null
  /** How many ratings avgRating averages (managers only). */
  ratings: number
  technicianDeclines: number
}

export interface Overview {
  period: Period
  current: KpiSet
  emergency: {
    count: number
    avgAcknowledgeMinutes: number | null
    avgFixHours: number | null
    downgraded: number
    merged: number
  }
  /** S03: done by the end of the due day. */
  preventive: {
    due: number
    onTime: number
    onTimeRate: number | null
    inspected: number
    passedRate: number | null
  }
  holdTime: { reason: string; times: number; totalHours: number; avgHours: number }[]
  avgOutsideHours: number | null
  weekly: { weekStart: string; reported: number; completed: number }[]
  byBuilding: {
    buildingId: number
    name: string
    reported: number
    completed: number
    onTimeRate: number | null
    openNow: number
    overdueNow: number
  }[]
}

/** ANA-02. fewCases: under 5 completed, the rates are not reliable yet. */
export interface TechnicianRow {
  technician: { id: number; fullName: string }
  completed: number
  avgCompletionHours: number | null
  onTimeRate: number | null
  reopenRate: number | null
  avgRating: number | null
  laborHours: number
  openNow: number
  declines: number
  declineReasons: string[]
  fewCases: boolean
}

export interface Failures {
  period: Period
  byServiceType: { id: number; name: string; cases: number; avgResolutionHours: number | null }[]
  byGroup: { group: string; cases: number }[]
  byAssetType: { group: string; cases: number }[]
  problemAssets: {
    id: number
    name: string
    location: string
    typeName: string
    repairs: number
    repairsLast30Days: number
    ageYears: number
    expectedLifeYears: number | null
    underWarranty: boolean
  }[]
}

export type AttentionKind =
  | 'overdue_case'
  | 'long_hold'
  | 'preventive_overdue'
  | 'frequent_failure'
  | 'extended_twice'
  | 'technician_declines'
  | 'no_technician'
  | 'not_fixed_report'
  | 'emergency_not_acknowledged'

/** F6 step ④: shown only, no extra notification. */
export interface AttentionItem {
  kind: AttentionKind
  text: string
  caseId: number | null
  caseCode: string | null
  assetId: number | null
  planId: number | null
  technicianId: number | null
  since: string | null
}

/** The Charts tab (meeting note 18/11). Sent as the server's enum names. */
export type ChartMetric =
  | 'CasesReported'
  | 'CasesCompleted'
  | 'CasesLate'
  | 'TechnicianCompleted'
  | 'TechnicianOnTime'
  | 'TechnicianLabourHours'
  | 'StaffDecisions'
  | 'StaffFirstResponse'
  | 'CostTotal'
  | 'CostByFault'

export type ChartStep = 'Day' | 'Week' | 'Month'

export interface ChartQuery extends AnalyticsFilters {
  metric: ChartMetric
  step: ChartStep
}

export interface Chart {
  period: Period
  unit: 'cases' | '%' | 'hours' | 'minutes' | 'VND'
  points: { start: string; value: number | null }[]
  breakdownBy: string
  breakdown: { name: string; value: number }[]
}

export type ExportTab = 'Overview' | 'Charts' | 'Technicians' | 'Failures' | 'Attention'

/** The page as it is on screen (meeting note 18/11: the file holds what the page shows). */
export interface ExportQuery extends AnalyticsFilters {
  tab: ExportTab
  metric?: ChartMetric
  step?: ChartStep
  sort?: string
  descending?: boolean
}
