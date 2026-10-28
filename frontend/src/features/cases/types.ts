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

/** Officer reject reasons (DLG-02). "Duplicate" is gone: duplicates are merged instead (N5). NOT IN ERD v5. */
export const REJECT_REASONS = ['missing_info', 'out_of_scope'] as const
export type RejectReason = (typeof REJECT_REASONS)[number]

export const REJECT_REASON_LABELS: Record<RejectReason, string> = {
  missing_info: 'Missing information',
  out_of_scope: 'Outside the building’s service scope',
}

// ---------------------------------------------------------------------------
// Reference data (owned by F1 — Huy). Shapes are only what F3 reads.
// ---------------------------------------------------------------------------

export interface PersonRef {
  id: number
  fullName: string
}

/**
 * One item of the 2-level service catalogue (N24): group "Water" -> type "Leak".
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

/** Where the problem is. NOT IN ERD v5: Cases only links Assets; a case without a known asset has no location. */
export interface CaseLocation {
  buildingId: number
  buildingName: string
  floorName: string
  roomId: number
  roomName: string
}

// ---------------------------------------------------------------------------
// Case
// ---------------------------------------------------------------------------

/** One row in a case list. */
export interface CaseSummary {
  id: number
  /** Display code, e.g. CS-000123 (D-plan). NOT IN ERD v5. */
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
  /** Officer in charge. ERD: Cases.Staff */
  officer: PersonRef | null
  /** ERD: Cases.AssignedTechnician */
  technician: PersonRef | null
  /** ERD: Cases.TimeSubmitted */
  submittedAt: string
  /** Effective deadline = the later of the SLA deadline and the officer's extension (N16). ERD: Cases.DueTime */
  dueAt: string | null
  /** When the SLA clock started: approval, or the last reopen. Derived from CaseStatusHistory (not a column). */
  slaStartedAt: string | null
  /** When the technician reported the work done. ERD: Cases.TimeCompleted */
  completedAt: string | null
  /** Case was sent to the emergency flow F4 at creation (N26). Derived from the service type. */
  isEmergencyFlow: boolean
}

/** Photo attached to a case. ERD: CaseImages(Image, Notes). NOT IN ERD v5: kind (before / after, N21). */
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
 * One assignment of a technician, kept even after a decline or a reassign (N10: the Manager sees declines).
 * ERD: only Cases.AssignedTechnician + TechnicianRejections(Technician, Case, RejectedTime, Reason).
 * NOT IN ERD v5 as a full history (proposed CaseAssignments table).
 */
export interface Assignment {
  technician: PersonRef
  assignedBy: PersonRef
  assignedAt: string
  response: 'waiting' | 'accepted' | 'declined'
  respondedAt: string | null
  declineReason: string | null
}

/** Technician's repair record (N21). ERD: Cases.ActualCompletionHours only. NOT IN ERD v5: the 3 text fields. */
export interface RepairReport {
  workDone: string
  rootCause: string
  /** Parts are written by hand, there is no stock module (N2). */
  partsUsed: string
  laborHours: number
}

/** Customer rating (DLG-14). NOT IN ERD v5. */
export interface Rating {
  stars: number
  comment: string
  ratedAt: string
}

export interface CaseDetail extends CaseSummary {
  /** ERD: Cases.Description */
  description: string
  /** ERD: Cases.Asset (optional: the customer may not know which asset) */
  asset: AssetRef | null
  serviceTypeId: number
  /** Copied from the service type at creation, confirmed by the officer on approval. NOT IN ERD v5. */
  costBearer: CostBearer
  /** SLA deadline before any extension (N16). NOT IN ERD v5 (only one DueTime column). */
  slaDueAt: string | null
  photos: CasePhoto[]
  history: StatusChange[]
  assignments: Assignment[]
  repairReport: RepairReport | null
  rating: Rating | null
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
  /** Data URLs while we use mock data; the real API will take an upload (Supabase Storage). */
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

export interface CompleteWorkRequest extends RepairReport {
  beforePhotos: string[]
  afterPhotos: string[]
}

export interface RateCaseRequest {
  stars: number
  comment: string
}

/** Technician option in the assign dialog (DLG-04). ERD: Technicians + TechnicianSpecialty. */
export interface TechnicianOption extends PersonRef {
  specialties: string[]
  openCases: number
}

/** AI suggestion while the customer writes the request (F3 section 4). The customer decides. */
export interface ServiceTypeSuggestion {
  serviceTypeId: number | null
  /** Hints to avoid a "Missing information" rejection (O05). */
  tips: string[]
}
