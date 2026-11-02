/** Every URL of the web app. Use these constants in <Link>, navigate() and the router — never string literals. */
export const paths = {
  login: '/login',
  changePassword: '/change-password',
  home: '/',
  forbidden: '/403',
  // F3 — cases
  myRequests: '/requests',
  newRequest: '/requests/new',
  cases: '/cases',
  caseDetail: '/cases/:id',
  workOrders: '/work-orders',
  // Core — every role
  notifications: '/notifications',
  // Core — Administrator (ADM-01 is paths.home)
  accounts: '/admin/accounts',
  accountNew: '/admin/accounts/new',
  accountDetail: '/admin/accounts/:id',
  accountEdit: '/admin/accounts/:id/edit',
  accountChangeRole: '/admin/accounts/:id/change-role',
  roles: '/admin/roles',
  auditLog: '/admin/audit-log',
  slaSettings: '/admin/sla',
} as const

export const caseDetailPath = (id: number) => `/cases/${id}`
export const accountDetailPath = (id: number) => `/admin/accounts/${id}`
export const accountEditPath = (id: number) => `/admin/accounts/${id}/edit`
export const accountChangeRolePath = (id: number) => `/admin/accounts/${id}/change-role`

/** New request form pre-filled from a rejected case ("Submit again", N6). */
export const resubmitPath = (fromCaseId: number) => `${paths.newRequest}?from=${fromCaseId}`
