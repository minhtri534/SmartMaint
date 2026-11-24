import type { Role } from '@/shared/types/role'

/** Mirrors backend Modules/Accounts/Dtos (camelCase JSON, snake_case enums). */

export type AccountStatus = 'active' | 'deactivated'

export const STATUS_LABELS: Record<AccountStatus, string> = {
  active: 'Active',
  deactivated: 'Deactivated',
}

/** DLG-16 actions; the server says which ones the Administrator may use on this account. */
export type AccountAction =
  | 'edit'
  | 'deactivate'
  | 'reactivate'
  | 'reset_password'
  | 'unlock'
  /** Only an account with no data (meeting note 18/11). */
  | 'delete'

/** ADM-02 row. ERD: Account (+ NOT IN ERD v5: fullName, email, status, lock fields). */
export interface AccountSummary {
  id: number
  username: string
  fullName: string
  email: string | null
  phoneNumber: string | null
  role: Role
  status: AccountStatus
  isLocked: boolean
  lastLoginAt: string | null
  createdAt: string
}

export interface SpecialtyRef {
  id: number
  name: string
}

export interface AssignedBuilding {
  buildingId: number
  buildingName: string
  isPrimary: boolean
}

/** ADM-04. */
export interface AccountDetail extends AccountSummary {
  mustChangePassword: boolean
  lockedUntil: string | null
  /** Technician only. ERD: Technicians.HourlyRate */
  hourlyRate: number | null
  /** Technician only. ERD: TechnicianSpecialty */
  specialties: SpecialtyRef[]
  /** Staff (with primary) or manager. ERD: StaffBuildingAssignments / ManagerBuildingAssignments */
  buildings: AssignedBuilding[]
  /** Has a case, a history row or an audit row in its role: the role can no longer change. */
  hasHistory: boolean
  allowedActions: AccountAction[]
}

export interface AccountPage {
  items: AccountSummary[]
  page: number
  pageSize: number
  totalCount: number
}

export interface AccountListFilters {
  role?: Role
  status?: AccountStatus
  search?: string
  /** Newest or oldest sign-in first; accounts that never signed in come last. */
  lastSignIn?: 'asc' | 'desc'
  page: number
}

export interface BuildingAssignmentRequest {
  buildingId: number
  isPrimary: boolean
}

/** Only the fields of the account's role are used by the server. */
export interface RoleDetailsRequest {
  hourlyRate: number | null
  specialtyIds: number[]
  buildings: BuildingAssignmentRequest[]
}

/** Meeting note 18/11: no username to type (the server makes one from the e-mail). */
export interface CreateAccountRequest extends RoleDetailsRequest {
  fullName: string
  email: string | null
  phoneNumber: string | null
  role: Role
}

export interface UpdateAccountRequest extends RoleDetailsRequest {
  fullName: string
  email: string | null
  phoneNumber: string | null
}

/** Create: the temporary password goes by e-mail; it is here only when the e-mail could not be sent. */
export interface AccountWithPassword {
  account: AccountDetail
  emailSent: boolean
  temporaryPassword: string | null
}

/** Reset password: the same rule. */
export interface TemporaryPasswordResult {
  emailSent: boolean
  temporaryPassword: string | null
}

export interface RoleCount {
  role: Role
  active: number
  deactivated: number
}

/** ADM-01. recentActivity rows have the audit log shape. */
export interface AdminOverview {
  accounts: RoleCount[]
  lockedNow: number
  mustChangePassword: number
  recentActivity: {
    id: number
    occurredAt: string
    adminName: string
    targetType: string
    operation: string
  }[]
}

/** Reference data for the form. */
export interface BuildingOption {
  id: number
  name: string
}
