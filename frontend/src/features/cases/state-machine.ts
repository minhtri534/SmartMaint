import type { Role } from '@/shared/types/role'
import { FINAL_STATUSES, type CaseStatus, type CaseSummary } from './types'

/*
 * F3 state machine (docs/flows/F3_Su_Co_Thong_Thuong/F3_Trang5.png), written as a table.
 * One row = one arrow on the diagram: who may do it, from which statuses, and where the case goes.
 * The backend must enforce the same table (error code "case.invalid_transition"); the web uses it
 * only to decide which buttons to show (W4: hide what the user cannot do).
 *
 * Done in this sketch: the main path + reject, cancel, decline, reopen.
 * Still to add as rows: merge (N5), on hold / resume (DLG-10), beyond capability -> officer decides -> F5,
 * reassign (DLG-05), change urgency / extend deadline, customer "not fixed" (DLG-13) + decline reopen (DLG-08).
 */

export type CaseAction =
  | 'cancel'
  | 'approve'
  | 'reject'
  | 'assign'
  | 'accept'
  | 'decline'
  | 'start'
  | 'complete'
  | 'close'
  | 'reopen'
  | 'rate'

interface TransitionRule {
  action: CaseAction
  role: Role
  from: readonly CaseStatus[]
  /** null = the status does not change (e.g. rating a closed case). */
  to: CaseStatus | null
}

/** Statuses inside the "approved – being handled" frame of page 5, where the officer may still cancel. */
const HANDLING: readonly CaseStatus[] = [
  'approved',
  'assigned',
  'accepted',
  'in_progress',
  'on_hold',
  'reopened',
]

export const TRANSITIONS: readonly TransitionRule[] = [
  // Page 1 — intake and review
  { action: 'cancel', role: 'customer', from: ['pending'], to: 'cancelled' },
  { action: 'cancel', role: 'officer', from: ['pending', ...HANDLING], to: 'cancelled' },
  { action: 'approve', role: 'officer', from: ['pending'], to: 'approved' },
  { action: 'reject', role: 'officer', from: ['pending'], to: 'rejected' },
  // Page 2 — assignment and repair
  { action: 'assign', role: 'officer', from: ['approved'], to: 'assigned' },
  { action: 'accept', role: 'technician', from: ['assigned', 'reopened'], to: 'accepted' },
  { action: 'decline', role: 'technician', from: ['assigned', 'reopened'], to: 'approved' },
  { action: 'start', role: 'technician', from: ['accepted'], to: 'in_progress' },
  { action: 'complete', role: 'technician', from: ['in_progress'], to: 'completed' },
  // Page 4 — verification and after closing
  { action: 'close', role: 'officer', from: ['completed'], to: 'closed' },
  { action: 'reopen', role: 'officer', from: ['completed'], to: 'reopened' },
  { action: 'rate', role: 'customer', from: ['closed'], to: null },
]

export function findTransition(
  action: CaseAction,
  role: Role,
  status: CaseStatus,
): TransitionRule | undefined {
  return TRANSITIONS.find((t) => t.action === action && t.role === role && t.from.includes(status))
}

/** The signed-in user as the case rules need it. */
export interface Actor {
  id: number
  role: Role
  /** Buildings an officer is in charge of (primary or backup). Empty for other roles. */
  buildingIds: readonly number[]
}

/** Row-level rule (B3): may this person act on this particular case at all? */
export function isInvolved(actor: Actor, c: CaseSummary): boolean {
  switch (actor.role) {
    case 'customer':
      return c.requester.id === actor.id
    case 'officer':
      return actor.buildingIds.includes(c.location.buildingId)
    case 'technician':
      return c.technician?.id === actor.id
    default:
      return false
  }
}

/** Actions to show as buttons for this case. Extra per-action conditions live here too. */
export function availableActions(
  actor: Actor,
  c: CaseSummary & { rating?: unknown },
): CaseAction[] {
  if (c.isEmergencyFlow || !isInvolved(actor, c)) return []
  return TRANSITIONS.filter((t) => t.role === actor.role && t.from.includes(c.status))
    .map((t) => t.action)
    .filter((action) => action !== 'rate' || !c.rating)
}

/** Overdue = past the effective deadline and nobody has reported it done yet (N16). */
export function isOverdue(c: CaseSummary, now = Date.now()): boolean {
  return (
    c.dueAt !== null &&
    new Date(c.dueAt).getTime() < now &&
    c.status !== 'completed' &&
    !FINAL_STATUSES.includes(c.status)
  )
}
