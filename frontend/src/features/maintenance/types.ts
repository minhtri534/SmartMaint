import type { CaseStatus } from '@/features/cases'

/** Mirrors backend Modules/Maintenance/Dtos (F2). */

export type FrequencyUnit = 'day' | 'week' | 'month' | 'year'
export const FREQUENCY_UNITS: FrequencyUnit[] = ['day', 'week', 'month', 'year']
export const FREQUENCY_LABELS: Record<FrequencyUnit, string> = {
  day: 'day(s)',
  week: 'week(s)',
  month: 'month(s)',
  year: 'year(s)',
}

/** ERD: TriggerStatuses. */
export type PlanStatus = 'pending_approval' | 'active' | 'rejected' | 'paused' | 'ended'
export const PLAN_STATUS_LABELS: Record<PlanStatus, string> = {
  pending_approval: 'Waiting for approval',
  active: 'Running',
  rejected: 'Rejected',
  paused: 'Paused',
  ended: 'Ended',
}

export type PlanAction = 'edit' | 'approve' | 'reject' | 'pause' | 'resume' | 'end'
export type Urgency = 'emergency' | 'high' | 'normal' | 'low'

export interface PersonRef {
  id: number
  fullName: string
}

/** ERD: CaseChecklists + ChecklistTasks. */
export interface Checklist {
  id: number
  name: string
  description: string | null
  categoryId: number
  categoryName: string
  tasks: { id: number; name: string; description: string | null }[]
  usedByPlans: number
}

export interface ChecklistRequest {
  name: string
  description: string | null
  categoryId: number
  tasks: { name: string; description: string | null }[]
}

/** ERD: MaintenanceWorkOrderAssets. */
export interface PlanAsset {
  assetId: number
  assetName: string
  roomName: string
  technician: PersonRef | null
  /** yyyy-MM-dd */
  startDate: string
  endDate: string | null
  lastDueDate: string | null
  nextDueDate: string | null
  isRetired: boolean
}

export interface PlanSummary {
  id: number
  name: string
  status: PlanStatus
  frequency: number
  frequencyType: FrequencyUnit
  buildingId: number
  buildingName: string
  assetCount: number
  nextDueDate: string | null
  staff: PersonRef
  approvingManager: PersonRef | null
}

export interface PlanDetail extends PlanSummary {
  leadDays: number
  checklistId: number
  checklistName: string
  estimatedHours: number | null
  priority: Urgency
  rejectReason: string | null
  lastPaused: string | null
  lastTriggered: string | null
  assets: PlanAsset[]
  workOrders: {
    id: number
    code: string
    assetName: string
    status: CaseStatus
    dueAt: string | null
    closedAt: string | null
  }[]
  allowedActions: PlanAction[]
}

export interface PlanRequest {
  frequency: number
  frequencyType: FrequencyUnit
  leadDays: number
  checklistId: number
  estimatedHours: number | null
  priority: Urgency
  assets: {
    assetId: number
    technicianId: number | null
    startDate: string
    endDate: string | null
  }[]
}

/** Reference data for the plan form (the staff member's own buildings). */
export interface AssetOption {
  id: number
  name: string
  location: { buildingId: number; buildingName: string; floorName: string; roomName: string }
}

export interface TechnicianOption {
  id: number
  fullName: string
  openCases: number
}

export interface ServiceTypeOption {
  id: number
  groupName: string
  name: string
}

/** "Every month", "Every 3 months". */
export function everyText(frequency: number, unit: FrequencyUnit) {
  return frequency === 1 ? `Every ${unit}` : `Every ${frequency} ${unit}s`
}
