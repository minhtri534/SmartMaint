/** Mirrors backend Modules/Analytics/Dtos (F6). Rates are 0..1, durations in minutes or hours as named. */

export interface AnalyticsFilters {
  /** yyyy-MM-dd, local dates, both included. Empty = the last 30 days. */
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
  technicianDeclines: number
}

export interface Overview {
  period: Period
  previousPeriod: Period
  current: KpiSet
  previous: KpiSet
  emergency: {
    count: number
    avgAcknowledgeMinutes: number | null
    avgMadeSafeMinutes: number | null
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

export type ExportReport = 'cases' | 'assets' | 'preventive' | 'technicians'
