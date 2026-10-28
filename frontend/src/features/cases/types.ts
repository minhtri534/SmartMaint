/*
 * Data shapes for flow F3 (normal incident). They follow what the F3 screens need
 * (docs/flows/F3_Su_Co_Thong_Thuong.md), and each field says where it lives in Hùng's
 * Logical ERD v5 (Drive, 27/10 14:28):
 *   "ERD: Table.Column"  -> already in the ERD
 *   "NOT IN ERD v5"      -> F3 needs it, the team still has to add it (or cut it and remove the field here)
 * When the backend exists these must match its DTOs exactly (camelCase JSON).
 */

// ---------------------------------------------------------------------------
// Fixed lists
// ---------------------------------------------------------------------------

/** The 11 case statuses (F3 page 5, "Need more info" removed 12/10). ERD: CaseStatuses (rows, not an enum). */
export const CASE_STATUSES = [
  'pending',
  'approved',
  'assigned',
  'accepted',
  'in_progress',
  'on_hold',
  'completed',
  'closed',
  'reopened',
  'rejected',
  'cancelled',
] as const
export type CaseStatus = (typeof CASE_STATUSES)[number]

export const STATUS_LABELS: Record<CaseStatus, string> = {
  pending: 'Pending',
  approved: 'Approved',
  assigned: 'Assigned',
  accepted: 'Accepted',
  in_progress: 'In progress',
  on_hold: 'On hold',
  completed: 'Completed',
  closed: 'Closed',
  reopened: 'Reopened',
  rejected: 'Rejected',
  cancelled: 'Cancelled',
}

/** Statuses where the case is finished and nobody has to act any more. */
export const FINAL_STATUSES: readonly CaseStatus[] = ['closed', 'rejected', 'cancelled']

/** 4 urgency levels (F3 section 3.2). ERD: PriorityLevels (rows). */
export const URGENCIES = ['emergency', 'high', 'normal', 'low'] as const
export type Urgency = (typeof URGENCIES)[number]

export const URGENCY_LABELS: Record<Urgency, string> = {
  emergency: 'Emergency',
  high: 'High',
  normal: 'Normal',
  low: 'Low',
}

/** Who pays (N23). NOT IN ERD v5. */
export const COST_BEARERS = ['building', 'customer'] as const
export type CostBearer = (typeof COST_BEARERS)[number]

export const COST_BEARER_LABELS: Record<CostBearer, string> = {
  building: 'Building (management board)',
  customer: 'Customer (tenant / resident)',
}

/** Why staff reject a case (DLG-02). Duplicates are rejected, not merged: the customer is told a case already exists (28/10). */
export const REJECT_REASONS = ['missing_info', 'out_of_scope', 'duplicate'] as const
export type RejectReason = (typeof REJECT_REASONS)[number]

export const REJECT_REASON_LABELS: Record<RejectReason, string> = {
  missing_info: 'Missing information',
  out_of_scope: 'Outside the building’s service scope',
  duplicate: 'A case for this problem already exists',
}

/** Why a technician pauses work (N13). "waiting_third_party" is set by the staff decision (F5), not offered here. */
export const HOLD_REASONS = ['waiting_parts', 'waiting_customer', 'waiting_staff_decision'] as const
export type HoldReason = (typeof HOLD_REASONS)[number] | 'waiting_third_party'

export const HOLD_REASON_LABELS: Record<HoldReason, string> = {
  waiting_parts: 'Waiting for parts',
  waiting_customer: 'Waiting for the customer (access, appointment)',
  waiting_staff_decision: 'Beyond my ability, waiting for the staff’s decision',
  waiting_third_party: 'Waiting for an outside party',
}

/** What a user can do to a case. The SERVER says which ones are allowed (CaseDetail.allowedActions). */
export type CaseAction =
  | 'cancel'
  | 'approve'
  | 'reject'
  | 'assign'
  | 'accept'
  | 'decline'
  | 'start'
  | 'hold'
  | 'resume'
  | 'complete'
  | 'close'
  | 'reopen'
  | 'rate'

// ---------------------------------------------------------------------------
// Reference data (F1 — Huy owns the real catalogue). Shapes are what GET /api/service-types, /buildings, /assets return.
// ---------------------------------------------------------------------------

export interface PersonRef {
  id: number
  fullName: string
}

/**
 * One item of the 2-level service catalogue (N24): group "Water", type "Leaking pipe or tap".
 * ERD: CaseCategories(Name, Description) — one level only.
 * NOT IN ERD v5: groupName, defaultUrgency, isEmergency, defaultCostBearer, photoRequired.
 */
export interface ServiceType {
  id: number
  groupName: string
  name: string
  defaultUrgency: Urgency
  /** In the emergency list -> goes to F4 as soon as it is created (N26). */
  isEmergency: boolean
  defaultCostBearer: CostBearer
  /** e.g. leaks need a photo (O05: fewer rejections for missing information). */
  photoRequired: boolean
}

/** ERD: Rooms -> Floors -> Buildings. */
export interface Room {
  id: number
  name: string
}
export interface Floor {
  id: number
  name: string
  rooms: Room[]
}
export interface Building {
  id: number
  name: string
  floors: Floor[]
}

/** ERD: Assets(Name, RoomID, TypeID). */
export interface AssetRef {
  id: number
  name: string
  roomId: number
}

/** Where the problem is. NOT IN ERD v5: Cases only links Assets; a customer may not know the asset. */
export interface CaseLocation {
  buildingId: number
  buildingName: string
  floorName: string
  roomId: number
  roomName: string
}

// ---------------------------------------------------------------------------
// Case. Same shape as the API (Modules/Cases/Dtos). "ERD:" = column in Hùng's ERD v5, "NOT IN ERD v5" = F3 needs it,
// the team decides at the ERD meeting (docs/database/F3_Cot_Bo_Sung_Cho_ERD.md).
// ---------------------------------------------------------------------------

/** One row in a case list. */
export interface CaseSummary {
  id: number
  /** Display code, e.g. CS-000123, built from the id. */
  code: string
  /** ERD: Cases.Title */
  title: string
  /** ERD: Cases.Status */
  status: CaseStatus
  /** ERD: Cases.PriorityLevel */
  urgency: Urgency
  /** ERD: Cases.Category (see ServiceType) */
  serviceTypeName: string
  location: CaseLocation
  /** ERD: Cases.RequestingCustomer */
  requester: PersonRef
  /** The staff member who approved it. ERD: Cases.Staff */
  staff: PersonRef | null
  /** ERD: Cases.AssignedTechnician */
  technician: PersonRef | null
  /** ERD: Cases.TimeSubmitted */
  submittedAt: string
  /** SLA deadline = start of work + the urgency's SLA hours. Empty until the technician starts. ERD: Cases.DueTime */
  dueAt: string | null
  /** When the technician started work = when the SLA clock started. ERD: Cases.StartTime */
  slaStartedAt: string | null
  /** When the technician reported the work done. ERD: Cases.TimeCompleted */
  completedAt: string | null
  /** The case belongs to the emergency flow F4 (emergency urgency): F3 offers no action. */
  isEmergencyFlow: boolean
}

/** Photo attached to a case. ERD: CaseImages(Image, Notes). NOT IN ERD v5: kind (report / before / after). */
export interface CasePhoto {
  id: number
  url: string
  kind: 'report' | 'before' | 'after'
}

/** One status change. ERD: CaseStatusHistory(NewStatus, Case, TimeChanged). NOT IN ERD v5: from, by, reason. */
export interface StatusChange {
  at: string
  from: CaseStatus | null
  to: CaseStatus
  by: PersonRef
  reason: string | null
}

/**
 * One assignment of a technician, kept even after a decline (N10: the manager sees declines).
 * ERD: only Cases.AssignedTechnician + TechnicianRejections. NOT IN ERD v5 as a full history (CaseAssignments).
 */
export interface Assignment {
  technician: PersonRef
  assignedBy: PersonRef
  assignedAt: string
  response: 'waiting' | 'accepted' | 'declined'
  respondedAt: string | null
  declineReason: string | null
}

/** Technician's repair record. ERD: Cases.ActualCompletionHours only. NOT IN ERD v5: rootCause, partsUsed. */
export interface RepairReport {
  rootCause: string
  /** Parts are written by hand, there is no stock module (N2). */
  partsUsed: string | null
  /** Typed by the technician, or measured from start to finish by the system. */
  laborHours: number
}

/** Customer rating (DLG-14). NOT IN ERD v5. */
export interface Rating {
  stars: number
  comment: string | null
  ratedAt: string
}

/** Filled while the case is On hold. NOT IN ERD v5. */
export interface Hold {
  reason: HoldReason
  /** yyyy-MM-dd */
  expectedResumeOn: string
}

export interface CaseDetail extends CaseSummary {
  /** ERD: Cases.Description */
  description: string
  /** ERD: Cases.Asset (optional: the customer may not know which asset) */
  asset: AssetRef | null
  serviceTypeId: number
  /** Copied from the service type at creation, confirmed by the staff on approval. NOT IN ERD v5. */
  costBearer: CostBearer
  photos: CasePhoto[]
  history: StatusChange[]
  assignments: Assignment[]
  repairReport: RepairReport | null
  rating: Rating | null
  hold: Hold | null
  /** What the signed-in user may do right now (state machine + role + data scope, decided by the server). */
  allowedActions: CaseAction[]
}

/** GET /api/cases */
export interface CasePage {
  items: CaseSummary[]
  page: number
  pageSize: number
  totalCount: number
}

// ---------------------------------------------------------------------------
// Requests (bodies the pages send)
// ---------------------------------------------------------------------------

export interface CreateCaseRequest {
  title: string
  description: string
  serviceTypeId: number
  roomId: number
  assetId: number | null
  /** Data URLs for now. TODO(storage): Supabase Storage. */
  photos: string[]
}

export interface ApproveCaseRequest {
  urgency: Urgency
  costBearer: CostBearer
}

export interface RejectCaseRequest {
  reason: RejectReason
  note: string
}

export interface AssignCaseRequest {
  technicianId: number
}

export interface ReasonRequest {
  reason: string
}

export interface HoldCaseRequest {
  reason: HoldReason
  /** yyyy-MM-dd, today or later */
  expectedResumeOn: string
}

export interface CompleteWorkRequest {
  rootCause: string
  partsUsed: string | null
  /** Empty = the system measures the time from start to finish. */
  laborHours: number | null
  beforePhotos: string[]
  afterPhotos: string[]
}

export interface RateCaseRequest {
  stars: number
  comment: string
}

/** Technician option in the assign dialog (DLG-04). Specialties come back with F1. */
export interface TechnicianOption extends PersonRef {
  openCases: number
}

/** AI suggestion while the customer writes the request (F3 section 4). The customer decides. */
export interface ServiceTypeSuggestion {
  serviceTypeId: number | null
  /** Hints to avoid a "Missing information" rejection (O05). */
  tips: string[]
}
