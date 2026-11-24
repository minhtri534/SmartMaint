import type { CaseDetail } from './types'

/** "2 of 3 done", for the Tasks button and the Tasks dialog. */
export function taskProgress(c: CaseDetail) {
  return `${c.tasks.filter((t) => t.status !== 'pending' && t.status !== 'moved').length} of ${c.tasks.length} done`
}
