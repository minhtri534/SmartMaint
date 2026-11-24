import { useState, type ReactNode } from 'react'
import { Link, useSearchParams } from 'react-router'
import { assetChangeDraftPath, paths } from '@/app/paths'
import { useAuth } from '@/features/auth'
import {
  EmptyBlock,
  ErrorBlock,
  Icon,
  LinkButton,
  LoadingBlock,
  PageHeader,
  SearchInput,
  SortHeader,
  sortRows,
  useTableSort,
  type SortState,
} from '@/shared/ui'
import { useFacilityTree } from '../hooks'
import type { BuildingNode, RoomNode } from '../types'

const roomsOf = (b: BuildingNode) => b.floors.flatMap((f) => f.rooms)
const assetsIn = (rooms: RoomNode[]) => rooms.reduce((n, r) => n + r.assetCount, 0)
const count = (n: number, noun: string) => `${n} ${noun}${n === 1 ? '' : 's'}`

/**
 * AST-01 Buildings, one level at a time (meeting note 18/11): the buildings, then the floors of the chosen building,
 * then the rooms of the chosen floor. The level is in the URL, so Back and a reload keep it. Read only: staff and
 * managers see their own buildings, the Administrator all. Buildings, floors, rooms and assets change only through a
 * staff change list a manager approves (meeting note 18/11, tree version 24/11): the staff get "Propose changes".
 */
export function FacilityTreePage() {
  const { user } = useAuth()
  const tree = useFacilityTree()
  const [params] = useSearchParams()
  const [search, setSearch] = useState('')
  const { sort, toggle } = useTableSort<'name'>()
  const isStaff = user?.role === 'staff'

  const buildings = tree.data ?? []
  const building = buildings.find((b) => String(b.id) === params.get('building'))
  const floor = building?.floors.find((f) => String(f.id) === params.get('floor'))

  // The search and the order apply to the level on screen.
  const level = floor?.id ?? building?.id ?? 0
  const [seenLevel, setSeenLevel] = useState(level)
  if (seenLevel !== level) {
    setSeenLevel(level)
    setSearch('')
  }
  const words = search.trim().toLowerCase()
  const pick = <T,>(rows: readonly T[], name: (row: T) => string) =>
    sortRows(
      rows.filter((r) => !words || name(r).toLowerCase().includes(words)),
      sort,
      { name },
    )
  const shownRooms = pick(floor?.rooms ?? [], (r) => r.roomNumber)
  const shownFloors = pick(building?.floors ?? [], (f) => f.name)
  const shownBuildings = pick(buildings, (b) => b.name)
  const noMatch = <EmptyBlock>{`Nothing matches “${search}”.`}</EmptyBlock>
  const tableSort = { sort, onSort: toggle }

  const crumbs = building && (
    <nav aria-label="Location" className="flex flex-wrap items-center gap-1">
      <Link to={paths.facilities} className="hover:text-ink">
        Buildings
      </Link>
      {floor && (
        <>
          <Icon name="back" className="size-3.5 rotate-180" />
          <Link to={`${paths.facilities}?building=${building.id}`} className="hover:text-ink">
            {building.name}
          </Link>
        </>
      )}
    </nav>
  )

  const header = floor ? (
    <PageHeader
      above={crumbs}
      title={floor.name}
      description={`${building!.name}, ${count(floor.rooms.length, 'room')}, ${count(assetsIn(floor.rooms), 'asset')} in service`}
      actions={
        isStaff && (
          <LinkButton
            variant="primary"
            to={assetChangeDraftPath({ buildingId: building!.id, floorId: floor.id })}
          >
            Propose changes
          </LinkButton>
        )
      }
    />
  ) : building ? (
    <PageHeader
      above={crumbs}
      title={building.name}
      description={`${building.address ?? 'No address'}, ${count(building.floors.length, 'floor')}, ${count(roomsOf(building).length, 'room')}`}
      actions={
        isStaff && (
          <LinkButton variant="primary" to={assetChangeDraftPath({ buildingId: building.id })}>
            Propose changes
          </LinkButton>
        )
      }
    />
  ) : (
    <PageHeader
      title="Buildings"
      description={
        tree.data
          ? `${count(buildings.length, 'building')}, ${count(buildings.flatMap(roomsOf).length, 'room')}, ${count(assetsIn(buildings.flatMap(roomsOf)), 'asset')} in service`
          : undefined
      }
      actions={
        isStaff && (
          <LinkButton variant="primary" to={assetChangeDraftPath()}>
            Propose changes
          </LinkButton>
        )
      }
    />
  )

  return (
    <div className="flex flex-col gap-5">
      {header}

      {tree.data && buildings.length > 0 && (
        <SearchInput
          label={`Search ${floor ? 'rooms' : building ? 'floors' : 'buildings'}`}
          placeholder={`Search ${floor ? 'rooms' : building ? 'floors' : 'buildings'}`}
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      )}

      {tree.isPending ? (
        <LoadingBlock rows={6} />
      ) : tree.error ? (
        <ErrorBlock
          what="the buildings"
          message={tree.error.message}
          onRetry={() => void tree.refetch()}
        />
      ) : floor ? (
        floor.rooms.length === 0 ? (
          <EmptyBlock>No room on this floor yet.</EmptyBlock>
        ) : shownRooms.length === 0 ? (
          noMatch
        ) : (
          <LevelTable heads={['Room', 'Assets']} actions={false} {...tableSort}>
            {shownRooms.map((r) => (
              <tr key={r.id} className="border-rule border-b last:border-0">
                <td className="py-3 pr-4 font-medium">{r.roomNumber}</td>
                <td className="py-3 pr-4 text-right tabular-nums">
                  <Link
                    to={`${paths.assets}?roomId=${r.id}`}
                    className="text-muted underline-offset-2 hover:underline"
                  >
                    {count(r.assetCount, 'asset')}
                  </Link>
                </td>
              </tr>
            ))}
          </LevelTable>
        )
      ) : building ? (
        building.floors.length === 0 ? (
          <EmptyBlock>No floor in this building yet.</EmptyBlock>
        ) : shownFloors.length === 0 ? (
          noMatch
        ) : (
          <LevelTable heads={['Floor', 'Rooms and assets']} actions={false} {...tableSort}>
            {shownFloors.map((f) => (
              <tr key={f.id} className="border-rule border-b last:border-0">
                <td className="py-3 pr-4">
                  <Link
                    to={`${paths.facilities}?building=${building.id}&floor=${f.id}`}
                    className="text-ink font-medium underline-offset-2 hover:underline"
                  >
                    {f.name}
                  </Link>
                </td>
                <td className="text-muted py-3 pr-4 text-right tabular-nums">
                  {count(f.rooms.length, 'room')}, {count(assetsIn(f.rooms), 'asset')}
                </td>
              </tr>
            ))}
          </LevelTable>
        )
      ) : buildings.length === 0 ? (
        <EmptyBlock>
          {user?.role === 'admin'
            ? 'No building yet: the staff add them in a change list.'
            : 'No building is assigned to you.'}
        </EmptyBlock>
      ) : shownBuildings.length === 0 ? (
        noMatch
      ) : (
        <LevelTable heads={['Building', 'Floors, rooms and assets']} actions={false} {...tableSort}>
          {shownBuildings.map((b) => (
            <tr key={b.id} className="border-rule border-b last:border-0">
              <td className="py-3 pr-4">
                <Link
                  to={`${paths.facilities}?building=${b.id}`}
                  className="text-ink font-medium underline-offset-2 hover:underline"
                >
                  {b.name}
                </Link>
                <div className="text-muted mt-0.5 text-[13px]">{b.address ?? 'No address'}</div>
              </td>
              <td className="text-muted py-3 pr-4 text-right tabular-nums">
                {count(b.floors.length, 'floor')}, {count(roomsOf(b).length, 'room')},{' '}
                {count(assetsIn(roomsOf(b)), 'asset')}
              </td>
            </tr>
          ))}
        </LevelTable>
      )}
    </div>
  )
}

/** The same table at each of the three levels: a name, what it holds, the Administrator's building actions. */
function LevelTable({
  heads,
  actions,
  sort,
  onSort,
  children,
}: {
  heads: [string, string]
  actions: boolean
  sort: SortState<'name'> | null
  onSort: (key: 'name') => void
  children: ReactNode
}) {
  return (
    <table className="w-full text-left text-sm">
      <thead>
        <tr className="border-border text-muted border-b text-xs">
          <SortHeader sortKey="name" sort={sort} onSort={onSort}>
            {heads[0]}
          </SortHeader>
          <th className="h-9 pr-4 text-right font-normal">{heads[1]}</th>
          {actions && <th className="h-9" />}
        </tr>
      </thead>
      <tbody>{children}</tbody>
    </table>
  )
}
