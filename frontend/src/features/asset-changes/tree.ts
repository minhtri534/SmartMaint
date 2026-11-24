import type { BuildingNode } from '@/features/facilities'
import type { ChangeItemRequest, ChangePlace, RoomAsset } from './types'

/**
 * The building tree with a change list laid over it (meeting note 18/11, tree version 24/11). Pure functions, so the
 * staff editor and the manager's review show the same thing.
 *
 * A place is named by a "ref": "b12" / "f3" / "r7" / "a9" for one that exists, or the key of the line that adds it
 * ("n4"). Deleted or moved things stay where they are and are only marked; every level above a change gets a dot.
 */

/** A line of the list on the client: the request, plus what the server said about it. */
export type Line = ChangeItemRequest & {
  at?: ChangePlace | null
  problem?: string | null
  targetLabel?: string | null
}

export type Mark = 'new' | 'edited' | 'deleted' | 'moving-out' | 'moving-in' | null

export interface Row {
  ref: string
  /** The existing building / floor / room / asset id; null for a new one. */
  id: number | null
  name: string
  sub: string | null
  mark: Mark
  /** The line that marks it (the one Undo removes). */
  line: Line | null
  /** Something below it changes. */
  dirty: boolean
  /** "Moving to …" / "Moving here from …". */
  note: string | null
  problem: string | null
}

const isNew = (ref: string) => ref.startsWith('n')
const idOf = (ref: string) => Number(ref.slice(1))

/** Where a new or moved floor / room / asset goes: its existing parent, or the line that adds the parent. */
export function parentRef(l: Line): string | null {
  switch (l.kind) {
    case 'add_floor':
      return l.buildingId ? `b${l.buildingId}` : l.parentKey
    case 'add_room':
      return l.floorId ? `f${l.floorId}` : l.parentKey
    case 'add_asset':
    case 'edit_asset':
      return l.roomId ? `r${l.roomId}` : l.parentKey
    default:
      return null
  }
}

/** Parents of the existing places, from the facility tree. */
export function indexTree(tree: BuildingNode[]) {
  const parent = new Map<string, string>()
  const label = new Map<string, string>()
  for (const b of tree) {
    label.set(`b${b.id}`, b.name)
    for (const f of b.floors) {
      parent.set(`f${f.id}`, `b${b.id}`)
      label.set(`f${f.id}`, f.name)
      for (const r of f.rooms) {
        parent.set(`r${r.id}`, `f${f.id}`)
        label.set(`r${r.id}`, r.roomNumber)
      }
    }
  }
  return { parent, label }
}
export type TreeIndex = ReturnType<typeof indexTree>

/** Every ref above (and including) a place, following new places through their lines. */
function chain(ref: string | null, lines: Line[], index: TreeIndex): string[] {
  const refs: string[] = []
  let current = ref
  while (current && !refs.includes(current)) {
    refs.push(current)
    if (isNew(current)) {
      const line = lines.find((l) => l.key === current)
      current = line ? parentRef(line) : null
    } else {
      current = index.parent.get(current) ?? null
    }
  }
  return refs
}

/** The refs that have a change at or below them. */
export function dirtyRefs(lines: Line[], index: TreeIndex): Set<string> {
  const dirty = new Set<string>()
  const add = (ref: string | null) => chain(ref, lines, index).forEach((r) => dirty.add(r))
  for (const l of lines) {
    if (l.kind === 'add_building') add(l.key)
    add(parentRef(l))
    if (l.buildingId) add(`b${l.buildingId}`)
    if (l.floorId) add(`f${l.floorId}`)
    if (l.roomId) add(`r${l.roomId}`)
    if (l.assetId) dirty.add(`a${l.assetId}`)
    if (l.at) {
      dirty.add(`b${l.at.buildingId}`)
      if (l.at.floorId) dirty.add(`f${l.at.floorId}`)
      if (l.at.roomId) dirty.add(`r${l.at.roomId}`)
    }
  }
  return dirty
}

/** "Tower A · Floor 5 · 503" for a room (existing or new). */
export function placeLabel(ref: string | null, lines: Line[], index: TreeIndex): string {
  return chain(ref, lines, index)
    .reverse()
    .map((r) => {
      if (!isNew(r)) return index.label.get(r) ?? '?'
      const l = lines.find((x) => x.key === r)
      return l?.kind === 'add_floor' ? `Floor ${l.name}` : (l?.name ?? '?')
    })
    .join(' · ')
}

const markOf = (line: Line | undefined, deleteKinds: string[]): Mark =>
  !line ? null : deleteKinds.includes(line.kind) ? 'deleted' : 'edited'

/** The buildings: those that exist, then the new ones. */
export function buildingRows(tree: BuildingNode[], lines: Line[], dirty: Set<string>): Row[] {
  const existing = tree.map((b): Row => {
    const line = lines.find(
      (l) => (l.kind === 'edit_building' || l.kind === 'delete_building') && l.buildingId === b.id,
    )
    const edited = line?.kind === 'edit_building'
    return {
      ref: `b${b.id}`,
      id: b.id,
      name: edited ? line.name! : b.name,
      sub: (edited ? line.address : b.address) ?? 'No address',
      mark: markOf(line, ['delete_building']),
      line: line ?? null,
      dirty: dirty.has(`b${b.id}`),
      note: null,
      problem: line?.problem ?? null,
    }
  })
  const added = lines
    .filter((l) => l.kind === 'add_building')
    .map((l): Row => newRow(l, l.name ?? '', l.address ?? 'No address', dirty))
  return [...existing, ...added]
}

function newRow(l: Line, name: string, sub: string | null, dirty: Set<string>): Row {
  return {
    ref: l.key,
    id: null,
    name,
    sub,
    mark: 'new',
    line: l,
    dirty: dirty.has(l.key),
    note: null,
    problem: l.problem ?? null,
  }
}

/** The floors of a building (existing or new). */
export function floorRows(
  tree: BuildingNode[],
  buildingRef: string,
  lines: Line[],
  dirty: Set<string>,
): Row[] {
  const building = isNew(buildingRef) ? null : tree.find((b) => b.id === idOf(buildingRef))
  const existing = (building?.floors ?? []).map((f): Row => {
    const line = lines.find(
      (l) => (l.kind === 'rename_floor' || l.kind === 'delete_floor') && l.floorId === f.id,
    )
    return {
      ref: `f${f.id}`,
      id: f.id,
      name: line?.kind === 'rename_floor' ? `Floor ${line.name}` : f.name,
      sub: `${f.rooms.length} ${f.rooms.length === 1 ? 'room' : 'rooms'}`,
      mark: markOf(line, ['delete_floor']),
      line: line ?? null,
      dirty: dirty.has(`f${f.id}`),
      note: null,
      problem: line?.problem ?? null,
    }
  })
  const added = lines
    .filter((l) => l.kind === 'add_floor' && parentRef(l) === buildingRef)
    .map((l) => newRow(l, `Floor ${l.name ?? ''}`, null, dirty))
  return [...existing, ...added]
}

/** The rooms of a floor (existing or new). */
export function roomRows(
  tree: BuildingNode[],
  floorRef: string,
  lines: Line[],
  dirty: Set<string>,
): Row[] {
  const floor = isNew(floorRef)
    ? null
    : tree.flatMap((b) => b.floors).find((f) => f.id === idOf(floorRef))
  const existing = (floor?.rooms ?? []).map((r): Row => {
    const line = lines.find(
      (l) => (l.kind === 'rename_room' || l.kind === 'delete_room') && l.roomId === r.id,
    )
    return {
      ref: `r${r.id}`,
      id: r.id,
      name: line?.kind === 'rename_room' ? line.name! : r.roomNumber,
      sub: `${r.assetCount} ${r.assetCount === 1 ? 'asset' : 'assets'}`,
      mark: markOf(line, ['delete_room']),
      line: line ?? null,
      dirty: dirty.has(`r${r.id}`),
      note: null,
      problem: line?.problem ?? null,
    }
  })
  const added = lines
    .filter((l) => l.kind === 'add_room' && parentRef(l) === floorRef)
    .map((l) => newRow(l, l.name ?? '', null, dirty))
  return [...existing, ...added]
}

/**
 * The assets of a room: those in it now (an edit, a move out or a delete only marks them), the new ones, and the ones
 * the list moves in from another room.
 */
export function assetRows(
  roomRef: string,
  roomAssets: RoomAsset[],
  lines: Line[],
  index: TreeIndex,
): Row[] {
  const existing = roomAssets.map((a): Row => {
    const line = lines.find(
      (l) =>
        (l.kind === 'edit_asset' || l.kind === 'retire_asset' || l.kind === 'delete_asset') &&
        l.assetId === a.id,
    )
    const movesTo = line?.kind === 'edit_asset' ? parentRef(line) : null
    const moving = movesTo !== null && movesTo !== roomRef
    return {
      ref: `a${a.id}`,
      id: a.id,
      name: line?.kind === 'edit_asset' ? line.name! : a.name,
      sub: [a.typeName, a.manufacturerName].filter(Boolean).join(' · '),
      mark: moving ? 'moving-out' : markOf(line, ['retire_asset', 'delete_asset']),
      line: line ?? null,
      dirty: false,
      note: moving ? `Moving to ${placeLabel(movesTo, lines, index)}` : null,
      problem: line?.problem ?? null,
    }
  })
  const here = new Set(roomAssets.map((a) => a.id))
  const added = lines
    .filter(
      (l) =>
        parentRef(l) === roomRef &&
        (l.kind === 'add_asset' || (l.kind === 'edit_asset' && !here.has(l.assetId ?? 0))),
    )
    .map((l): Row => ({
      ref: l.kind === 'add_asset' ? l.key : `a${l.assetId}`,
      id: l.assetId,
      name: l.name ?? '',
      sub: null,
      mark: l.kind === 'add_asset' ? 'new' : 'moving-in',
      line: l,
      dirty: false,
      note:
        l.kind === 'edit_asset' && l.at?.roomId
          ? `Moving here from ${placeLabel(`r${l.at.roomId}`, lines, index)}`
          : null,
      problem: l.problem ?? null,
    }))
  return [...existing, ...added]
}

/** The next free line key ("n7"). */
export function nextKey(lines: Line[]): string {
  return `n${Math.max(0, ...lines.map((l) => Number(l.key.slice(1)) || 0)) + 1}`
}

/** Removes a line and every line that builds on it (a new floor's rooms, their assets…). */
export function withoutLine(lines: Line[], key: string): Line[] {
  const gone = new Set([key])
  let grew = true
  while (grew) {
    grew = false
    for (const l of lines) {
      const parent = l.parentKey
      if (parent && gone.has(parent) && !gone.has(l.key)) {
        gone.add(l.key)
        grew = true
      }
    }
  }
  return lines.filter((l) => !gone.has(l.key))
}

/** An empty line of a kind, to fill in. */
export function emptyLine(kind: Line['kind'], key: string): Line {
  return {
    kind,
    key,
    parentKey: null,
    buildingId: null,
    floorId: null,
    roomId: null,
    assetId: null,
    name: null,
    address: null,
    typeId: null,
    manufacturerId: null,
    hasCopyright: null,
    warrantyEndDate: null,
  }
}

/** Points a new floor / room / asset (or a moved asset) at its parent: an existing place or a new one's line. */
export function placedIn(line: Line, ref: string): Line {
  const at = isNew(ref) ? null : idOf(ref)
  const parentKey = isNew(ref) ? ref : null
  if (line.kind === 'add_floor') return { ...line, buildingId: at, parentKey }
  if (line.kind === 'add_room') return { ...line, floorId: at, parentKey }
  return { ...line, roomId: at, parentKey }
}

/** The line as the API takes it (without what the server added). */
export function toRequest({
  at: _at,
  problem: _problem,
  targetLabel: _label,
  ...line
}: Line): ChangeItemRequest {
  return line
}
