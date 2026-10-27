/** The 5 roles, same text values as the API and the database (D1). */
export const ROLES = ['customer', 'officer', 'technician', 'manager', 'admin'] as const

export type Role = (typeof ROLES)[number]

export const ROLE_LABELS: Record<Role, string> = {
  customer: 'Customer',
  officer: 'Facility Officer',
  technician: 'Maintenance Technician',
  manager: 'Facility Manager',
  admin: 'Administrator',
}
