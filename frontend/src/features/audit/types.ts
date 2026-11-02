/** Mirrors backend Modules/Audit/Dtos. ERD: AuditLog (+ lookup tables AuditLogTargetType / AuditLogTargetOperation). */

export const AUDIT_TARGETS = ['account', 'priority_level'] as const
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
] as const
export type AuditOperation = (typeof AUDIT_OPERATIONS)[number]

export const TARGET_LABELS: Record<AuditTarget, string> = {
  account: 'Account',
  priority_level: 'SLA level',
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
  page: number
}
