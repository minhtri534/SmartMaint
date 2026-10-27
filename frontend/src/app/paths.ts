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
} as const

export const caseDetailPath = (id: number) => `/cases/${id}`

/** New request form pre-filled from a rejected case ("Submit again", N6). */
export const resubmitPath = (fromCaseId: number) => `${paths.newRequest}?from=${fromCaseId}`
