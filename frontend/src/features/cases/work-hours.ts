import type { CaseDetail } from './types'

/** Work hours so far: the stopped total plus the running stretch (meeting 07/11). */
export function workedHoursNow(c: Pick<CaseDetail, 'workedHours' | 'clockStartedAt'>, now: number) {
  const running = c.clockStartedAt ? (now - new Date(c.clockStartedAt).getTime()) / 3_600_000 : 0
  return Math.max(0, (c.workedHours ?? 0) + running)
}
