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

/**
 * The 10 case statuses (F3 page 5; "Need more info" removed 12/10, "Assigned" removed 07/11: the staff names the
 * technician on approval). ERD: CaseStatuses (rows, not an enum).
 */
export const CASE_STATUSES = [
  'pending',
  'approved',
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

/** Who pays (N23). Meeting 07/11: no longer recorded for a case; only old F5 quote rows still carry it. */
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
/** What the technician picks in "Put on hold". The two F5 reasons come from "Needs the staff's decision" and the staff's hand-over. */
export const HOLD_REASONS = ['waiting_parts', 'waiting_customer'] as const
export type HoldReason =
  (typeof HOLD_REASONS)[number] | 'waiting_staff_decision' | 'waiting_third_party'

export const HOLD_REASON_LABELS: Record<HoldReason, string> = {
  waiting_parts: 'Waiting for parts',
  waiting_customer: 'Waiting for the customer (access, appointment)',
  waiting_staff_decision: 'Waiting for the staff’s decision (repair or replace)',
  waiting_third_party: 'Waiting for an outside party',
}

/** What a user can do to a case. The SERVER says which ones are allowed (CaseDetail.allowedActions). */
export type CaseAction =
  | 'cancel'
  | 'approve'
  | 'reject'
  | 'accept'
  | 'decline'
  | 'start'
  | 'hold'
  | 'resume'
  | 'complete'
  | 'rate'
  /** Meeting 07/11: the staff's one Edit (replaces assign, reassign, change urgency, continue, close, reopen). */
  | 'edit'
  | 'manage_followers'
  | 'reassign'
  | 'edit_own_cost'
  // Part 4: the deadline, the customer's "not fixed" report, cost lines
  | 'extend_due'
  | 'report_not_fixed'
  | 'accept_not_fixed'
  | 'decline_not_fixed'
  /** Meeting note 18/11: a closed case is read only; reopening is the one action left (never after a reassign). */
  | 'reopen'
  | 'update_costs'
  // Tasks (every case has tasks, meeting 07/11)
  | 'update_task'
  | 'log_hours'
  | 'add_cost'
  | 'set_task_third_party'
  // Part 6: emergency (F4)
  | 'acknowledge'
  | 'add_log'
  | 'write_incident_report'
  // Part 7: repair or replace (F5). Meeting 07/11: the hand-over to an outside party is one button.
  | 'request_decision'
  | 'outsource'

/** What the Edit form can do to the status (the server lists the ones allowed now: CaseDetail.statusChanges). */
export type EditStatusChange = 'continue' | 'close' | 'reopen'

export const STATUS_CHANGE_LABELS: Record<EditStatusChange, string> = {
  continue: 'Let the technician continue (back to In progress)',
  close: 'Verified: close the case',
  reopen: 'Not OK: reopen for the technician',
}

/** O09: why a deadline moves (a fixed list, counted in the statistics). */
export const EXTEND_REASONS = [
  'waiting_parts',
  'customer_rescheduled',
  'larger_than_expected',
  'other',
] as const
export type ExtendDueReason = (typeof EXTEND_REASONS)[number]
export const EXTEND_REASON_LABELS: Record<ExtendDueReason, string> = {
  waiting_parts: 'Waiting for parts',
  customer_rescheduled: 'The customer asked for another time',
  larger_than_expected: 'The work is larger than expected',
  other: 'Other reason',
}

// ---------------------------------------------------------------------------
// Reference data (F1 — Huy owns the real catalogue). Shapes are what GET /api/service-types, /buildings, /assets return.
// ---------------------------------------------------------------------------

export interface PersonRef {
  id: number
  fullName: string
}

/**
 * A fault category the customer can choose. ERD: CaseCategories(Name, Description).
 * NOT IN ERD v5: groupName, assetTypeId (empty = it applies to every asset type).
 */
export interface ServiceType {
  id: number
  groupName: string
  name: string
  assetTypeId: number | null
}

/**
 * ERD: Rooms -> Floors -> Buildings. Room name = Rooms.RoomNumber, floor name = "Floor " + Floors.FloorNumber.
 */
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
  typeId: number
}

/** The asset a case is about. */
export interface CaseAsset extends AssetRef {
  typeName: string
  categoryName: string
}

/** A customer in the followers dialog. */
export interface FollowerOption {
  id: number
  fullName: string
  username: string
  email: string | null
}

export interface FollowerOptions {
  /** Who follows the case now. */
  current: FollowerOption[]
  /** The customers the search found. */
  matches: FollowerOption[]
}

/** The AI's choice for a case (or the top of the ranked list when the AI is off). Only a suggestion. */
export interface TechnicianSuggestion {
  technicianId: number | null
  reason: string
  source: 'ai' | 'rules'
}

/** An asset found by its code (GET /asset-register/{id}/qr-target). */
export interface AssetLookup {
  id: number
  name: string
  typeId: number
  location: CaseLocation
  isRetired: boolean
  canReport: boolean
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
  /** ERD: Cases.RequestingCustomer. Empty for a case generated by preventive maintenance (F2). */
  requester: PersonRef | null
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
  /** Generated by a preventive maintenance plan (F2): a checklist instead of a repair record. */
  isPreventive: boolean
  /** F4: set while (and after) the case is an emergency; empty for F2 / F3 and after a downgrade. NOT IN ERD v5. */
  emergency: EmergencyInfo | null
  /** Why it is on hold (only while On Hold). */
  holdReason: HoldReason | null
  /** F5 way 2: a survey the staff opened for an asset that fails often (no customer, not in the KPI). */
  isSurvey: boolean
  /** F5: the latest outside party of the case (old flow, before 07/11). */
  outsideWork: OutsideWorkBrief | null
  /** Meeting 07/11: when the staff plans the work to start / be finished. Not the SLA deadline. NOT IN ERD v5. */
  plannedStartAt: string | null
  plannedEndAt: string | null
  /** Until the work starts: planned start + the urgency's SLA hours. It only informs; dueAt is the real deadline. */
  expectedDueAt: string | null
  /** The fault category. ERD: Cases.Category */
  serviceTypeId: number
  asset: CaseAsset | null
  /** Rejected because it repeats this case: it can be opened, but no figure counts it. */
  duplicateOf: CaseRef | null
}

// ---------------------------------------------------------------------------
// F5 repair or replace / outside party. All NOT IN ERD v5 (only Cases.TimeOutsourced).
// ---------------------------------------------------------------------------

export type TechnicianRecommendation = 'repairable' | 'replace' | 'specialist'
export const RECOMMENDATION_LABELS: Record<TechnicianRecommendation, string> = {
  repairable: 'Repairable, with more time or parts',
  replace: 'Replace the asset',
  specialist: 'Needs an outside specialist',
}

export type OutsideWorkKind = 'warranty' | 'contract' | 'quote'
export const OUTSIDE_KIND_LABELS: Record<OutsideWorkKind, string> = {
  warranty: 'Manufacturer, under warranty',
  contract: 'Contractor with a maintenance contract',
  quote: 'Contractor quoting for this job',
}

export type OutsideWorkJob = 'repair' | 'replace'

export type OutsideWorkStatus =
  'quoting' | 'awaiting_decision' | 'working' | 'failed_check' | 'declined' | 'done' | 'cancelled'
export const OUTSIDE_STATUS_LABELS: Record<OutsideWorkStatus, string> = {
  quoting: 'Preparing a quote',
  awaiting_decision: 'Quote waits for a decision',
  working: 'Working',
  failed_check: 'Check failed',
  declined: 'Declined',
  done: 'Done',
  cancelled: 'Stopped',
}

export interface OutsideWorkBrief {
  kind: OutsideWorkKind
  job: OutsideWorkJob
  status: OutsideWorkStatus
  companyName: string
  appointmentAt: string | null
}

export interface ThirdParty extends OutsideWorkBrief {
  id: number
  contact: string
  note: string | null
  redoCount: number
  quoteBearer: CostBearer | null
  quoteSentTo: string | null
  quoteSentAt: string | null
  decisionGo: boolean | null
  decidedBy: string | null
  decidedAt: string | null
  evidenceUrl: string | null
  /** The outside phase, reported separately in F6 (N16). */
  startedAt: string
  finishedAt: string | null
}

/** Meeting 07/11: the technician's request for an outside party (condition = description, cause = reason). */
export interface Assessment {
  condition: string
  cause: string
  /** Only on requests made before 07/11. */
  recommendation: TechnicianRecommendation | null
  assessedAt: string
}

/** F5 step ③: facts for the staff's decision (no money). The staff decide; `suggestReplace` is only a hint. */
export interface DecisionInfo {
  repairsLast12Months: number
  reopensLast12Months: number
  assetAgeYears: number
  expectedLifeYears: number | null
  warrantyEndDate: string | null
  underWarranty: boolean
  manufacturerOnly: boolean
  manufacturerName: string | null
  manufacturerPhone: string | null
  recommendation: TechnicianRecommendation | null
  suggestReplace: boolean
  reasons: string[]
}

/** A link to another case. */
export interface CaseRef {
  id: number
  code: string
}

/**
 * F4 milestones. The 24/7 clock started at `startedAt` (the report, or when the case was raised to Emergency);
 * first response = acknowledged, then fixed (completedAt).
 */
export interface EmergencyInfo {
  startedAt: string
  acknowledgedAt: string | null
  /** This report was merged into that case (same incident, N5). */
  mergedInto: CaseRef | null
}

/** F4 step ⑫ (K08): what the post-incident report proposes. */
export type IncidentFollowUp = 'none' | 'preventive_plan' | 'replacement_review'
export const FOLLOW_UP_LABELS: Record<IncidentFollowUp, string> = {
  none: 'Nothing more to do',
  preventive_plan: 'Make a preventive maintenance plan (F2)',
  replacement_review: 'Consider replacing the asset (F5)',
}

export interface IncidentReport {
  damage: string
  lessons: string
  followUp: IncidentFollowUp
  reportedAt: string
}

/** What a line of the emergency log is about. */
export type EmergencyLogKind = 'third_party' | 'workaround' | 'customer_update' | 'other'
export const LOG_KIND_LABELS: Record<EmergencyLogKind, string> = {
  third_party: 'Called a contracted third party',
  workaround: 'Temporary measure for the people affected',
  customer_update: 'Update for the customer',
  other: 'Other note',
}

/** Result of one checklist item. ERD: TaskStatuses. */
/** 'moved': not finished when the case went to another technician; it goes on in the new case (meeting note 18/11). */
export type TaskStatus = 'pending' | 'pass' | 'fail' | 'not_applicable' | 'moved'
export const TASK_STATUS_LABELS: Record<TaskStatus, string> = {
  pending: 'Not done',
  pass: 'Successful',
  fail: 'Fail',
  not_applicable: 'N/A',
  moved: 'Moved to the new case',
}

/** What the technician does with a failed item (BR-07). */
export type FailAction = 'fixed_on_site' | 'create_case'

/** Who finished a task (meeting 07/11, for the KPI and the costs). NOT IN ERD v5. */
export type TaskDoneBy = 'technician' | 'third_party'
export const DONE_BY_LABELS: Record<TaskDoneBy, string> = {
  technician: 'Technician',
  third_party: 'Outside party',
}

/**
 * One task of a case: a checklist item of a work order (F2), or a task the staff wrote on approval (meeting 07/11:
 * every case has tasks). ERD: CaseTasks + TaskNotes + TaskImages.
 */
export interface CaseTask {
  id: number
  name: string
  description: string | null
  status: TaskStatus
  notes: { note: string; at: string }[]
  /** Photos with their caption (ERD: TaskImages(Image, Notes)). */
  photos: { url: string; caption: string | null }[]
  /** The repair case made from this failed item. NOT IN ERD v5. */
  repairCaseId: number | null
  repairCaseCode: string | null
  /** Who finished it and when (empty while not done). NOT IN ERD v5. */
  doneBy: TaskDoneBy | null
  doneAt: string | null
}

/** Photo attached to a case. ERD: CaseImages(Image, Notes). NOT IN ERD v5: kind (report / before / after). */
export interface CasePhoto {
  id: number
  url: string
  kind: 'report' | 'before' | 'after' | 'assessment'
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
 * A technician declined the job (N10: the manager sees declines). ERD: TechnicianRejections.
 * Who got the job is in the history (the "Approved" row says "Assigned to ..."): the ERD keeps only the last technician.
 */
export interface Rejection {
  technician: PersonRef
  rejectedAt: string
  reason: string
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
  expectedResumeOn: string | null
}

export interface CaseDetail extends CaseSummary {
  /** ERD: Cases.Description */
  description: string
  /** ERD: Cases.Asset (optional: the customer may not know which asset) */
  asset: CaseAsset | null
  serviceTypeId: number
  /** Meeting 07/11: the case this one continues after a change of technician. NOT IN ERD v5. */
  previousCase: CaseRef | null
  /** ... and the case that continues this one. */
  nextCase: CaseRef | null
  /** Meeting 07/11: other customers who follow the case (read only). NOT IN ERD v5. */
  followers: PersonRef[]
  photos: CasePhoto[]
  history: StatusChange[]
  rejections: Rejection[]
  /** Empty for the customer (meeting 07/11). */
  repairReport: RepairReport | null
  rating: Rating | null
  hold: Hold | null
  /** ERD: Costs. Null for the customer (meeting 07/11). */
  costs: Cost[] | null
  totalCost: number | null
  /** Meeting 07/11: the work hours so far, and when the running clock started (empty when stopped). */
  workedHours: number | null
  clockStartedAt: string | null
  /** The customer's "not fixed" report waiting for the staff (within 7 days of closing). NOT IN ERD v5. */
  notFixed: { reason: string; reportedAt: string } | null
  /** F2: the maintenance plan that made this work order. ERD: Cases.TriggerWorkOrder */
  planId: number | null
  /** Every case has tasks (meeting 07/11). Empty for the customer. */
  tasks: CaseTask[]
  /** F4: reports of the same incident merged into this case (before 07/11). */
  mergedCases: CaseRef[]
  /** F4 step ⑫: the post-incident report, once written. */
  incidentReport: IncidentReport | null
  /** F5: the technician's assessment when the job is beyond them. */
  assessment: Assessment | null
  /** F5: outside parties of the old flow (before 07/11). */
  thirdParties: ThirdParty[]
  /** F5: the decision board (staff and managers only). */
  decisionInfo: DecisionInfo | null
  /** F5: the new asset an outside party installed. */
  /** The status changes the staff's Edit form may offer now (empty for everyone else). */
  statusChanges: EditStatusChange[]
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
  /** The customer's choice (meeting 07/11). "Emergency" stays Pending until the staff confirms it. */
  urgency: Urgency
}

/** One task line of the work order form. id null = a new task; a task left out is removed. */
export interface TaskLine {
  id: number | null
  name: string
  description: string | null
}

/** DLG-01 Approve = the work order form (meeting 07/11): the request as corrected + technician, window, tasks. */
export interface ApproveCaseRequest {
  urgency: Urgency
  technicianId: number
  serviceTypeId: number
  title: string
  description: string
  /** ISO date-times */
  plannedStartAt: string | null
  plannedEndAt: string | null
  tasks: TaskLine[]
}

/** The staff's one Edit (meeting 07/11). */
export interface EditCaseRequest {
  urgency: Urgency
  serviceTypeId: number
  title: string
  description: string
  plannedStartAt: string | null
  plannedEndAt: string | null
  tasks: TaskLine[]
  statusChange: EditStatusChange | null
  /** Required to reopen. */
  reason: string | null
}

/** Meeting 07/11 (replaces merge): what a repeated request hands to the case it repeats. */
export interface DuplicateOf {
  caseId: number
  addAsTask: boolean
  copyPhotos: boolean
  addFollower: boolean
}

export interface RejectCaseRequest {
  reason: RejectReason
  note: string
  duplicate: DuplicateOf | null
}

export interface ReasonRequest {
  reason: string
}

/** One cost line. ERD: Costs(Name, Description, Amount). Amount in VND. id = a line the staff keeps (its receipt stays). */
export interface CostLine {
  id?: number | null
  name: string
  description: string | null
  amount: number
}

/** A cost line as the API returns it, with the receipt the technician gave (meeting 07/11). */
export interface Cost extends CostLine {
  id: number
  receiptCode: string | null
  receiptUrl: string | null
  /** Who entered the line; a technician may edit only their own. */
  addedById: number | null
}

/** Meeting 07/11: the technician types the total work hours. */
export interface LogHoursRequest {
  hours: number
}

/** Meeting 07/11: a cost the technician paid, with the receipt number and / or a photo of it (one is required). */
export interface AddCostRequest {
  name: string
  description: string | null
  amount: number
  receiptCode: string | null
  /** Data URL */
  receiptPhoto: string | null
}

export interface ExtendDueRequest {
  /** ISO date-time, later than the current deadline */
  newDueAt: string
  reason: ExtendDueReason
  note: string | null
}

export interface UpdateCostsRequest {
  lines: CostLine[]
}

/**
 * TEC-05, meeting 07/11: status (still to do is fine, to add a note), a note, photos with captions. A Fail needs a
 * note and a photo (unless the task has one); on a work order it may make a repair case.
 */
export interface UpdateTaskRequest {
  status: TaskStatus
  note: string | null
  photos: { image: string; caption: string | null }[]
  failAction: FailAction | null
  /** A dangerous failure makes an emergency repair case (F4). */
  dangerous: boolean
}

/** Meeting 07/11: ask the staff to hand the job to an outside party — what is wrong, why, photos. */
export interface RequestDecisionRequest {
  description: string
  reason: string
  photos: string[]
}

export interface OpenSurveyRequest {
  assetId: number
  serviceTypeId: number
  note: string | null
}

export interface EmergencyLogRequest {
  kind: EmergencyLogKind
  note: string
}

export interface IncidentReportRequest {
  damage: string
  lessons: string
  followUp: IncidentFollowUp
}

export interface RateCaseRequest {
  stars: number
  comment: string
}

/** One open job of a technician. Title is empty for a case in a building the staff does not manage. */
export interface TechnicianJob {
  id: number
  code: string
  title: string | null
  status: CaseStatus
  urgency: Urgency
  plannedStartAt: string | null
  plannedEndAt: string | null
  dueAt: string | null
}

/** A technician the staff can choose (meeting 07/11): specialties and schedule. */
export interface TechnicianOption extends PersonRef {
  openCases: number
  specialties: string[]
  jobs: TechnicianJob[]
  /** A specialty of theirs names the trade of the case being planned. */
  fits: boolean
}

/** AI suggestion while the customer writes the request (F3 section 4). The customer decides. */
export interface ServiceTypeSuggestion {
  serviceTypeId: number | null
  /** The AI's urgency for this description; null = use the service type's default. */
  urgency: Urgency | null
  /** Hints to avoid a "Missing information" rejection (O05). */
  tips: string[]
  /** "ai" = Replicate answered; "rules" = the AI is off or failed, so the web matched keywords itself. */
  source: 'ai' | 'rules'
}

/** POST /api/ai/request-suggestion. available = false: no AI right now (no token, timeout, error). */
export interface RequestSuggestionResponse {
  available: boolean
  serviceTypeId: number | null
  urgency: Urgency | null
  tips: string[]
}
