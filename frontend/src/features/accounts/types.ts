import type { Role } from '@/shared/types/role'

/** Mirrors backend Modules/Accounts/Dtos (camelCase JSON, snake_case enums). */

export type AccountStatus = 'active' | 'deactivated'

export const STATUS_LABELS: Record<AccountStatus, string> = {
  active: 'Active',
  deactivated: 'Deactivated',
}

/** DLG-16 actions; the server says which ones the Administrator may use on this account. */
export type AccountAction =
  'edit' | 'change_role' | 'deactivate' | 'reactivate' | 'reset_password' | 'unlock'

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

export interface RoomRef {
  id: number
  buildingName: string
  floorName: string
  roomName: string
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
  /** Customer only. ERD: Customers + NOT IN ERD v5 RoomId. */
  room: RoomRef | null
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
  page: number
}

export interface BuildingAssignmentRequest {
  buildingId: number
  isPrimary: boolean
}

/** Only the fields of the account's role are used by the server. */
export interface RoleDetailsRequest {
  roomId: number | null
  hourlyRate: number | null
  specialtyIds: number[]
  buildings: BuildingAssignmentRequest[]
}

export interface CreateAccountRequest extends RoleDetailsRequest {
  username: string
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

export interface ChangeRoleRequest extends RoleDetailsRequest {
  role: Role
}

export interface AccountWithPassword {
  account: AccountDetail
  /** Shown once, never stored readable by the server. */
  temporaryPassword: string
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
  floors: {
    id: number
    name: string
    rooms: { id: number; name: string; isCommonArea: boolean }[]
  }[]
}
