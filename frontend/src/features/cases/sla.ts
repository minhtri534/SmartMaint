import { FINAL_STATUSES, type CaseSummary } from './types'

/*
 * The SLA clock of a case (N8, N16). It starts when the officer approves (or reopens) the case and runs to
 * the effective deadline = the later of the SLA deadline and the officer's extension.
 * The mock counts wall-clock hours; the real API counts the building's office hours (BR-08, O04).
 */

export type SlaState =
  /** Not approved yet: the clock has not started. */
  | 'waiting'
  /** Clock running, deadline ahead. */
  | 'running'
  /** Clock running, deadline passed — the one red mark on the case. */
  | 'overdue'
  /** Technician reported done before the deadline. */
  | 'met'
  /** Technician reported done after the deadline. */
  | 'late'
  /** No SLA: rejected, cancelled, or handled by the emergency flow F4. */
  | 'none'

export interface Sla {
  state: SlaState
  /** Share of the window used, 0..1 (1 when overdue). */
  used: number
  /** Window length in ms (start -> deadline). */
  window: number
  /** Time left (negative when overdue), or the margin / delay for met / late. */
  remaining: number
}

const NONE: Sla = { state: 'none', used: 0, window: 0, remaining: 0 }

export function slaOf(c: CaseSummary, now = Date.now()): Sla {
  if (c.isEmergencyFlow || c.status === 'rejected' || c.status === 'cancelled') return NONE
  if (!c.dueAt) return { ...NONE, state: 'waiting' }

  const due = new Date(c.dueAt).getTime()
  const start = new Date(c.slaStartedAt ?? c.submittedAt).getTime()
  const window = Math.max(1, due - start)

  if ((c.status === 'completed' || c.status === 'closed') && c.completedAt) {
    const done = new Date(c.completedAt).getTime()
    return {
      state: done <= due ? 'met' : 'late',
      used: Math.min(1, (done - start) / window),
      window,
      remaining: due - done,
    }
  }
  if (FINAL_STATUSES.includes(c.status)) return NONE

  const used = Math.min(1, Math.max(0, (now - start) / window))
  return { state: now > due ? 'overdue' : 'running', used, window, remaining: due - now }
}

/** Overdue = past the effective deadline and nobody has reported it done yet (N16). */
export function isOverdue(c: CaseSummary, now = Date.now()): boolean {
  return slaOf(c, now).state === 'overdue'
}
