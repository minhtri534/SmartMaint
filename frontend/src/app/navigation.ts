import type { Role } from '@/shared/types/role'
import { paths } from './paths'

export interface NavItem {
  label: string
  to: string
  /** Roles that see this item. Must match the `roles` of the route in router.tsx. */
  roles: readonly Role[]
}

const ALL_ROLES: readonly Role[] = ['customer', 'officer', 'technician', 'manager', 'admin']

/**
 * Sidebar items, in display order. Adding a page = add its path in paths.ts, its route in router.tsx,
 * and (if it belongs in the sidebar) one entry here. Role access follows Screen_Catalog / SRS 3.x matrix.
 */
export const NAV_ITEMS: readonly NavItem[] = [
  { label: 'Home', to: paths.home, roles: ALL_ROLES },
  // F3
  { label: 'My requests', to: paths.myRequests, roles: ['customer'] },
  { label: 'New request', to: paths.newRequest, roles: ['customer'] },
  { label: 'Cases', to: paths.cases, roles: ['officer', 'manager'] },
  { label: 'My work orders', to: paths.workOrders, roles: ['technician'] },
]

export function navItemsFor(role: Role): NavItem[] {
  return NAV_ITEMS.filter((item) => item.roles.includes(role))
}
