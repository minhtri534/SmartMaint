import type { CaseStatus } from '@/features/cases'

/** Mirrors backend Modules/Maintenance/Dtos (F2). Since the meeting note of 18/11 a plan works like UpKeep: one
 * work-order template with one or more triggers, each with its own schedule, end day and assets. */

export type FrequencyUnit = 'day' | 'week' | 'month' | 'year'
export const FREQUENCY_UNITS: FrequencyUnit[] = ['day', 'week', 'month', 'year']
export const FREQUENCY_LABELS: Record<FrequencyUnit, string> = {
  day: 'day(s)',
  week: 'week(s)',
  month: 'month(s)',
  year: 'year(s)',
}

/** ERD: TriggerStatuses. "Expired" is not a status: a running or paused plan whose triggers all passed their end day. */
export type PlanStatus = 'pending_approval' | 'active' | 'rejected' | 'paused' | 'archived'
export const PLAN_STATUS_LABELS: Record<PlanStatus, string> = {
  pending_approval: 'Waiting for approval',
  active: 'Running',
  rejected: 'Rejected',
  paused: 'Paused',
  archived: 'Archived',
}

export type PlanAction = 'edit' | 'approve' | 'reject' | 'pause' | 'resume' | 'archive'
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
  tasks: { id: number; name: string; description: string | null }[]
  usedByPlans: number
}

export interface ChecklistRequest {
  name: string
  description: string | null
  tasks: { name: string; description: string | null }[]
}

export interface PlanEvery {
  frequency: number
  frequencyType: FrequencyUnit
}

/** ERD: MaintenanceWorkOrderAssets. */
export interface PlanAsset {
  assetId: number
  assetName: string
  roomName: string
  technician: PersonRef | null
  /** yyyy-MM-dd */
  startDate: string
  lastDueDate: string | null
  nextDueDate: string | null
  isRetired: boolean
}

/** ERD: MaintenanceWorkOrderDetails — one trigger of a plan. */
export interface PlanTrigger extends PlanEvery {
  id: number
  leadDays: number
  startDate: string
  /** Empty: no end. */
  endDate: string | null
  nextDueDate: string | null
  /** It has an end day and no due day is left before it. */
  expired: boolean
  assets: PlanAsset[]
}

export interface PlanSummary {
  id: number
  name: string
  status: PlanStatus
  /** Running or paused, but every trigger passed its end day: nothing is made until an end day moves later. */
  expired: boolean
  every: PlanEvery[]
  buildingId: number
  buildingName: string
  assetCount: number
  nextDueDate: string | null
  staff: PersonRef
  approvingManager: PersonRef | null
}

export interface PlanDetail extends PlanSummary {
  description: string | null
  checklistId: number
  checklistName: string
  checklistItems: number
  estimatedHours: number | null
  priority: Urgency
  rejectReason: string | null
  createdAt: string
  lastPaused: string | null
  lastTriggered: string | null
  archivedAt: string | null
  triggers: PlanTrigger[]
  workOrderCount: number
  allowedActions: PlanAction[]
}

/** A work order the plan made (a case). */
export interface PlanWorkOrder {
  id: number
  code: string
  triggerId: number
  assetName: string
  technicianName: string | null
  status: CaseStatus
  dueAt: string | null
  closedAt: string | null
}

export interface TriggerRequest {
  /** An existing trigger of the plan; null = new. */
  id: number | null
  frequency: number
  frequencyType: FrequencyUnit
  leadDays: number
  startDate: string
  endDate: string | null
  assets: { assetId: number; technicianId: number | null }[]
}

export interface PlanRequest {
  name: string
  description: string | null
  checklistId: number
  estimatedHours: number | null
  priority: Urgency
  triggers: TriggerRequest[]
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

/** "Every month", "Every 3 months". */
export function everyText(frequency: number, unit: FrequencyUnit) {
  return frequency === 1 ? `Every ${unit}` : `Every ${frequency} ${unit}s`
}

/** "Every month, every 3 months". */
export function everyListText(every: PlanEvery[]) {
  return every
    .map((e, i) => {
      const text = everyText(e.frequency, e.frequencyType)
      return i === 0 ? text : text.toLowerCase()
    })
    .join(', ')
}
