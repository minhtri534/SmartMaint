import { Link } from 'react-router'
import { paths } from '@/app/paths'
import { FINAL_STATUSES, useCases } from '@/features/cases'
import { Icon } from '@/shared/ui'

/** Where each role handles an emergency: the staff in Cases (filtered), the manager on the emergency board. */
const TARGET = {
  staff: `${paths.cases}?tab=all&urgency=emergency`,
  manager: paths.emergencies,
}

/**
 * The one count kept in the top bar (Leader 18/11): an emergency nobody has acknowledged yet. It is there only while
 * that is true; every other count lives on Home and the case lists.
 */
export function EmergencyAlert({ role }: { role: keyof typeof TARGET }) {
  const cases = useCases(60_000)
  const count = (cases.data ?? []).filter(
    (c) =>
      c.isEmergencyFlow &&
      !FINAL_STATUSES.includes(c.status) &&
      c.status !== 'completed' &&
      !c.emergency?.acknowledgedAt &&
      !c.emergency?.mergedInto,
  ).length
  if (count === 0) return null

  const label = count === 1 ? 'emergency' : 'emergencies'
  return (
    <Link
      to={TARGET[role]}
      aria-label={`${count} ${label} not acknowledged yet`}
      title="Not acknowledged yet"
      className="bg-pill-negative-bg text-pill-negative-ink ease-snap flex h-10 flex-none items-center gap-2 rounded-full px-3 text-[13px] font-medium whitespace-nowrap transition-transform duration-150 active:scale-[0.97]"
    >
      <Icon name="siren" className="size-4" />
      <span className="tabular-nums">
        {count}
        <span className="hidden sm:inline"> {label}</span>
      </span>
    </Link>
  )
}
