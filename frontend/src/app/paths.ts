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
  /** The staff's work order form (meeting 07/11): approve, and Edit. */
  caseApprove: '/cases/:id/approve',
  caseEdit: '/cases/:id/edit',
  workOrders: '/work-orders',
  // F4 — emergency board
  emergencies: '/emergencies',
  // F1 — buildings and assets
  facilities: '/facilities',
  assets: '/assets',
  qrLabels: '/assets/qr-labels',
  assetDetail: '/assets/:id',
  /** Meeting note 18/11: the staff's change lists of floors, rooms and assets, approved by a manager. */
  assetChanges: '/asset-changes',
  /** Meeting note 18/11 (tree version 24/11): the staff's tree editor of their draft. */
  assetChangeDraft: '/asset-changes/draft',
  assetChangeDetail: '/asset-changes/:id',
  assetChangeEdit: '/asset-changes/:id/edit',
  qrLanding: '/qr/assets/:id',
  catalog: '/admin/catalog',
  // F2 — preventive maintenance
  checklists: '/maintenance/checklists',
  plans: '/maintenance/plans',
  planNew: '/maintenance/plans/new',
  planDetail: '/maintenance/plans/:id',
  planEdit: '/maintenance/plans/:id/edit',
  // Core — Administrator (ADM-01 is paths.home)
  accounts: '/admin/accounts',
  accountNew: '/admin/accounts/new',
  accountDetail: '/admin/accounts/:id',
  accountEdit: '/admin/accounts/:id/edit',
  roles: '/admin/roles',
  auditLog: '/admin/audit-log',
  slaSettings: '/admin/sla',
  // F6 — analytics (ANA-01..04)
  analytics: '/analytics',
} as const

export const caseDetailPath = (id: number) => `/cases/${id}`
export const caseApprovePath = (id: number) => `/cases/${id}/approve`
export const caseEditPath = (id: number) => `/cases/${id}/edit`
export const assetDetailPath = (id: number) => `/assets/${id}`
export const assetChangeDetailPath = (id: number) => `/asset-changes/${id}`
export const assetChangeEditPath = (id: number) => `/asset-changes/${id}/edit`
/** A new change list, for a building, or starting with a change of one asset. */
/** The staff's tree editor (their draft), opened at a building, floor or room of the tree. */
export const assetChangeDraftPath = (
  at: { buildingId?: number; floorId?: number; roomId?: number } = {},
) => {
  const p = new URLSearchParams()
  if (at.buildingId) p.set('building', `b${at.buildingId}`)
  if (at.floorId) p.set('floor', `f${at.floorId}`)
  if (at.roomId) p.set('room', `r${at.roomId}`)
  const q = p.toString()
  return q ? `/asset-changes/draft?${q}` : '/asset-changes/draft'
}
/** What a QR label encodes (AST-06 / AST-07); prefixed with the site origin when printed. */
export const qrLandingPath = (id: number) => `/qr/assets/${id}`
/** The customer's report form filled in from a QR label. */
export const reportFromQrPath = (roomId: number, assetId: number) =>
  `/requests/new?roomId=${roomId}&assetId=${assetId}`
export const planDetailPath = (id: number) => `/maintenance/plans/${id}`
export const planEditPath = (id: number) => `/maintenance/plans/${id}/edit`
export const accountDetailPath = (id: number) => `/admin/accounts/${id}`
export const accountEditPath = (id: number) => `/admin/accounts/${id}/edit`

/** New request form pre-filled from a rejected case ("Submit again", N6). */
export const resubmitPath = (fromCaseId: number) => `${paths.newRequest}?from=${fromCaseId}`
