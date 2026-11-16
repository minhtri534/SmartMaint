import { useState } from 'react'
import { Link } from 'react-router'
import { caseDetailPath, paths } from '@/app/paths'
import { FINAL_STATUSES, isOverdue, type CaseSummary } from '@/features/cases'
import { useFacilityTree, type BuildingNode } from '@/features/facilities'
import { Card, CardHeader, ErrorBlock, LoadingBlock, Tabs } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'

/*
 * SmartMaint's own element on Home (16/11): each building drawn as its floors, top floor first, each floor a row
 * of its rooms — the lit-window idea of the logo work. A room with open cases is tinted (accent), a room with an
 * overdue case is red (its one red mark). Answers "where in my buildings is work open?" at a glance.
 */

interface RoomState {
  open: CaseSummary[]
  overdue: number
}

function roomStates(cases: CaseSummary[]) {
  const map = new Map<number, RoomState>()
  for (const c of cases) {
    if (FINAL_STATUSES.includes(c.status)) continue
    const s = map.get(c.location.roomId) ?? { open: [], overdue: 0 }
    s.open.push(c)
    if (isOverdue(c)) s.overdue += 1
    map.set(c.location.roomId, s)
  }
  return map
}

const openIn = (b: BuildingNode, states: Map<number, RoomState>) =>
  b.floors.reduce(
    (n, f) => n + f.rooms.reduce((m, r) => m + (states.get(r.id)?.open.length ?? 0), 0),
    0,
  )

export function BuildingFloors({ cases }: { cases: CaseSummary[] }) {
  const tree = useFacilityTree()
  const [picked, setPicked] = useState<string | null>(null)

  if (tree.isPending) return <LoadingBlock rows={4} />
  if (tree.error)
    return (
      <ErrorBlock
        what="your buildings"
        message={tree.error.message}
        onRetry={() => void tree.refetch()}
      />
    )
  if (tree.data.length === 0) return null

  const states = roomStates(cases)
  const buildings = tree.data
  // Until one is picked, show the building with the most open work.
  const busiest = [...buildings].sort((a, b) => openIn(b, states) - openIn(a, states))[0]
  const building = buildings.find((b) => String(b.id) === picked) ?? busiest
  if (!building) return null

  return (
    <Card>
      <CardHeader title={buildings.length === 1 ? building.name : 'Floors'} aside={<Legend />} />
      {buildings.length > 1 && (
        <div className="-mt-2 mb-4">
          <Tabs
            items={buildings.map((b) => ({
              value: String(b.id),
              label: b.name,
              count: openIn(b, states),
            }))}
            value={String(building.id)}
            onChange={setPicked}
          />
        </div>
      )}

      {/* Top floor first, like the building itself. */}
      <ol className="flex flex-col gap-2">
        {[...building.floors].reverse().map((floor) => (
          <li key={floor.id} className="flex items-start gap-3">
            <span className="text-muted w-16 flex-none pt-2 text-right text-xs whitespace-nowrap tabular-nums">
              {/* level marker of a building section drawing */}
              <span aria-hidden="true" className="text-accent">
                ▽{' '}
              </span>
              {floor.floorNumber === 'G' ? 'Ground' : `Floor ${floor.floorNumber}`}
            </span>
            <div className="border-slab flex min-w-0 flex-1 flex-wrap gap-1.5 border-l-2 pl-3">
              {floor.rooms.length === 0 ? (
                <span className="text-muted py-2 text-xs">No rooms</span>
              ) : (
                floor.rooms.map((room) => (
                  <RoomCell key={room.id} name={room.roomNumber} state={states.get(room.id)} />
                ))
              )}
            </div>
          </li>
        ))}
      </ol>
    </Card>
  )
}

function RoomCell({ name, state }: { name: string; state?: RoomState }) {
  const count = state?.open.length ?? 0
  const tone =
    count === 0
      ? 'bg-raised text-muted'
      : state!.overdue > 0
        ? 'bg-pill-negative-bg text-pill-negative-ink'
        : 'bg-pill-accent-bg text-pill-accent-ink'
  const body = (
    <>
      <span className="truncate">{name}</span>
      {count > 0 && <span className="font-semibold tabular-nums">{count}</span>}
    </>
  )
  const cell = cn(
    'rounded-control flex h-8 min-w-14 max-w-40 items-center justify-between gap-2 px-2.5 text-[13px]',
    // colours cross-fade when a refresh opens or closes a case in the room
    'duration-200 ease-snap',
    tone,
  )
  if (count === 0) return <span className={cn(cell, 'transition-colors')}>{body}</span>

  const titles = state!.open.map((c) => `${c.code} ${c.title}`).join('\n')
  return (
    <Link
      to={count === 1 && state?.open[0] ? caseDetailPath(state.open[0].id) : paths.cases}
      title={titles}
      aria-label={`${name}: ${count} open ${count === 1 ? 'case' : 'cases'}${state!.overdue ? `, ${state!.overdue} overdue` : ''}`}
      className={cn(
        cell,
        'transition-[transform,background-color,color] hover:brightness-[0.97] active:scale-[0.97]',
      )}
    >
      {body}
    </Link>
  )
}

/** Each swatch is the cell itself, scaled down. */
function Legend() {
  return (
    <span className="flex items-center gap-3">
      <span className="flex items-center gap-1.5">
        <span className="bg-pill-accent-bg ring-pill-accent-ink/15 size-3 rounded-[3px] ring-1 ring-inset" />{' '}
        open
      </span>
      <span className="flex items-center gap-1.5">
        <span className="bg-pill-negative-bg ring-pill-negative-ink/15 size-3 rounded-[3px] ring-1 ring-inset" />{' '}
        overdue
      </span>
    </span>
  )
}
