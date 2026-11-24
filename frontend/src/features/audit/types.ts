/** Mirrors backend Modules/Audit/Dtos. ERD: AuditLog (+ lookup tables AuditLogTargetType / AuditLogTargetOperation). */

// Same values as backend Entities/Core/AuditLog.cs (AuditTarget / AuditOperation, snake_case on the wire).
// 16/11: F1 added building … specialty and delete / retire / import on the backend; the page crashed on them.
export const AUDIT_TARGETS = [
  'account',
  'priority_level',
  'building',
  'floor',
  'room',
  'asset',
  'case_category',
  'asset_category',
  'asset_type',
  'manufacturer',
  'specialty',
] as const
export type AuditTarget = (typeof AUDIT_TARGETS)[number]

export const AUDIT_OPERATIONS = [
  'sign_in',
  'create',
  'update',
  'change_role',
  'deactivate',
  'reactivate',
  'reset_password',
  'unlock',
  'delete',
  'retire',
  'import',
] as const
export type AuditOperation = (typeof AUDIT_OPERATIONS)[number]

export const TARGET_LABELS: Record<AuditTarget, string> = {
  account: 'Account',
  priority_level: 'SLA level',
  building: 'Building',
  floor: 'Floor',
  room: 'Room',
  asset: 'Asset',
  case_category: 'Service type',
  asset_category: 'Asset category',
  asset_type: 'Asset type',
  manufacturer: 'Manufacturer',
  specialty: 'Specialty',
}

export const OPERATION_LABELS: Record<AuditOperation, string> = {
  sign_in: 'Sign-in',
  create: 'Create',
  update: 'Update',
  change_role: 'Change role',
  deactivate: 'Deactivate',
  reactivate: 'Reactivate',
  reset_password: 'Reset password',
  unlock: 'Unlock',
  delete: 'Delete',
  retire: 'Retire',
  import: 'Import',
}

export interface AuditRow {
  id: number
  occurredAt: string
  adminId: number
  adminName: string
  targetType: AuditTarget
  operation: AuditOperation
  /** JSON of the record before / after the change. */
  oldValue: string | null
  newValue: string | null
}

export interface AuditPage {
  items: AuditRow[]
  page: number
  pageSize: number
  totalCount: number
}

export interface AuditFilters {
  targetType?: AuditTarget
  operation?: AuditOperation
  /** yyyy-MM-dd */
  from?: string
  to?: string
  oldestFirst?: boolean
  page: number
}
