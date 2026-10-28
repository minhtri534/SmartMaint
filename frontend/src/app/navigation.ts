import type { Role } from '@/shared/types/role'
import type { IconName } from '@/shared/ui'
import { paths } from './paths'

export interface NavItem {
  label: string
  to: string
  icon: IconName
  /** Other URL prefixes that keep this item highlighted (e.g. a case opened from this list). */
  also?: readonly string[]
  /** Roles that see this item. Must match the `roles` of the route in router.tsx. */
  roles: readonly Role[]
}

/**
 * Sidebar items, in display order. Adding a page = add its path in paths.ts, its route in router.tsx,
 * and (if it belongs in the sidebar) one entry here. Role access follows Screen_Catalog / SRS 3.x matrix.
 * Nav items are other screens, never anchors into the current page.
 */
export const NAV_ITEMS: readonly NavItem[] = [
  // F3
  {
    label: 'My requests',
    to: paths.myRequests,
    icon: 'requests',
    also: ['/cases/'],
    roles: ['customer'],
  },
  { label: 'New request', to: paths.newRequest, icon: 'plus', roles: ['customer'] },
  {
    label: 'Cases',
    to: paths.cases,
    icon: 'queue',
    also: ['/cases/'],
    roles: ['officer', 'manager'],
  },
  {
    label: 'My work orders',
    to: paths.workOrders,
    icon: 'wrench',
    also: ['/cases/'],
    roles: ['technician'],
  },
  { label: 'Home', to: paths.home, icon: 'home', roles: ['admin'] },
]

export function navItemsFor(role: Role): NavItem[] {
  return NAV_ITEMS.filter((item) => item.roles.includes(role))
}

/** Where each role lands after signing in (its main screen). */
export const HOME_BY_ROLE: Record<Role, string> = {
  customer: paths.myRequests,
  officer: paths.cases,
  manager: paths.cases,
  technician: paths.workOrders,
  admin: paths.home,
}
