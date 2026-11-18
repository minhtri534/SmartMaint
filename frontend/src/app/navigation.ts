import type { Role } from '@/shared/types/role'
import type { IconName } from '@/shared/ui'
import { paths } from './paths'

/** The named groups of the sidebar, in display order. */
export const NAV_GROUPS = ['Menu', 'Maintenance', 'Property', 'System'] as const
export type NavGroup = (typeof NAV_GROUPS)[number]

export interface NavItem {
  label: string
  to: string
  icon: IconName
  group: NavGroup
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
  // Every role: its Home (F6)
  {
    label: 'Home',
    group: 'Menu',
    to: paths.home,
    icon: 'home',
    roles: ['customer', 'staff', 'technician', 'manager', 'admin'],
  },
  // F3
  {
    label: 'My requests',
    group: 'Menu',
    to: paths.myRequests,
    icon: 'requests',
    also: ['/cases/'],
    roles: ['customer'],
  },
  { label: 'New request', group: 'Menu', to: paths.newRequest, icon: 'plus', roles: ['customer'] },
  {
    label: 'Cases',
    group: 'Menu',
    to: paths.cases,
    icon: 'queue',
    also: ['/cases/'],
    roles: ['staff', 'manager'],
  },
  // F4 (meeting 07/11: the staff find emergencies in Cases with the urgency filter)
  { label: 'Emergencies', group: 'Menu', to: paths.emergencies, icon: 'siren', roles: ['manager'] },
  {
    label: 'My work orders',
    group: 'Menu',
    to: paths.workOrders,
    icon: 'wrench',
    also: ['/cases/'],
    roles: ['technician'],
  },
  // F2
  {
    label: 'Maintenance plans',
    group: 'Maintenance',
    to: paths.plans,
    icon: 'calendar',
    also: ['/maintenance/plans/'],
    roles: ['staff', 'manager'],
  },
  {
    label: 'Checklists',
    group: 'Maintenance',
    to: paths.checklists,
    icon: 'checklist',
    roles: ['staff', 'manager', 'admin'],
  },
  // F6
  { label: 'Analytics', group: 'Menu', to: paths.analytics, icon: 'chart', roles: ['manager'] }, // meeting 07/11: not for the staff
  // F1 — buildings and assets (read for staff / manager, edit for admin)
  {
    label: 'Buildings',
    group: 'Property',
    to: paths.facilities,
    icon: 'building',
    roles: ['staff', 'manager', 'admin'],
  },
  {
    label: 'Assets',
    group: 'Property',
    to: paths.assets,
    icon: 'box',
    also: ['/assets/'],
    // Meeting 07/11: the staff no longer browse the asset list (they still open an asset from a case).
    roles: ['manager', 'admin'],
  },
  { label: 'Catalog', group: 'Property', to: paths.catalog, icon: 'list', roles: ['admin'] },
  {
    label: 'Accounts',
    group: 'System',
    to: paths.accounts,
    icon: 'users',
    also: ['/admin/accounts/'],
    roles: ['admin'],
  },
  {
    label: 'Replacements',
    group: 'Property',
    to: paths.replacements,
    icon: 'swap',
    roles: ['admin'],
  },
  {
    label: 'SLA settings',
    group: 'System',
    to: paths.slaSettings,
    icon: 'clock',
    roles: ['admin'],
  },
  { label: 'Audit log', group: 'System', to: paths.auditLog, icon: 'log', roles: ['admin'] },
  { label: 'Roles', group: 'System', to: paths.roles, icon: 'shield', roles: ['admin'] },
]

export function navItemsFor(role: Role): NavItem[] {
  return NAV_ITEMS.filter((item) => item.roles.includes(role))
}

/** The role's pages by group; a group with nothing for this role is left out. */
export function navGroupsFor(role: Role) {
  const items = navItemsFor(role)
  return NAV_GROUPS.map((label) => ({
    label,
    items: items.filter((item) => item.group === label),
  })).filter((group) => group.items.length > 0)
}

/** Where each role lands after signing in: its own Home (F6, Screen catalog rule 15). */
export const HOME_BY_ROLE: Record<Role, string> = {
  customer: paths.home,
  staff: paths.home,
  manager: paths.home,
  technician: paths.home,
  admin: paths.home,
}
