import { useState, type ReactNode } from 'react'
import { useSearchParams } from 'react-router'
import type { BuildingNode } from '@/features/facilities'
import {
  Badge,
  Button,
  ConfirmDialog,
  Dialog,
  DialogActions,
  EmptyBlock,
  ErrorBlock,
  Icon,
  LoadingBlock,
  SearchInput,
  Select,
  TextField,
} from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { useRoomAssets } from '../hooks'
import {
  assetRows,
  buildingRows,
  dirtyRefs,
  emptyLine,
  floorRows,
  indexTree,
  nextKey,
  parentRef,
  placedIn,
  placeLabel,
  roomRows,
  withoutLine,
  type Line,
  type Row,
  type TreeIndex,
} from '../tree'
import type { AssetTypeOption, ManufacturerOption, RoomAsset } from '../types'

type Level = 'building' | 'floor' | 'room' | 'asset'
const NOUN: Record<Level, string> = {
  building: 'building',
  floor: 'floor',
  room: 'room',
  asset: 'asset',
}

/**
 * The building tree with a change list laid over it (meeting note 18/11, tree version 24/11): buildings, then the floors
 * of one, its rooms, then the assets of a room. The level is in the URL (?building=b1&floor=f3&room=n4). New, edited,
 * deleted and moved things are marked where they are; a red dot shows a change somewhere below. With onChange the staff
 * edit it (each edit changes the lines); without, it is the manager's read-only view.
 */
export function ChangeTree({
  tree,
  lines,
  onChange,
  types,
  makers,
}: {
  tree: BuildingNode[]
  lines: Line[]
  onChange?: (lines: Line[]) => void
  types: AssetTypeOption[]
  makers: ManufacturerOption[]
}) {
  const [params, setParams] = useSearchParams()
  const [search, setSearch] = useState('')
  const [dialog, setDialog] = useState<DialogState | null>(null)
  const index = indexTree(tree)
  const dirty = dirtyRefs(lines, index)
  const editable = onChange !== undefined

  const buildingRef = params.get('building')
  const roomRef = params.get('room')
  // Opened from an asset (building + room only): the floor comes from the tree.
  const floorRef =
    params.get('floor') ??
    (roomRef && !roomRef.startsWith('n') ? (index.parent.get(roomRef) ?? null) : null)
  const level: Level = roomRef ? 'asset' : floorRef ? 'room' : buildingRef ? 'floor' : 'building'
  const roomId = roomRef && !roomRef.startsWith('n') ? Number(roomRef.slice(1)) : null
  const roomAssets = useRoomAssets(level === 'asset' ? roomId : null)

  const go = (next: { building?: string; floor?: string; room?: string }) => {
    const p = new URLSearchParams(params)
    for (const key of ['building', 'floor', 'room'] as const) {
      if (next[key]) p.set(key, next[key]!)
      else p.delete(key)
    }
    setSearch('')
    setParams(p, { replace: false })
  }

  const rows: Row[] =
    level === 'building'
      ? buildingRows(tree, lines, dirty)
      : level === 'floor'
        ? floorRows(tree, buildingRef!, lines, dirty)
        : level === 'room'
          ? roomRows(tree, floorRef!, lines, dirty)
          : assetRows(roomRef!, roomAssets.data ?? [], lines, index)
  const words = search.trim().toLowerCase()
  const shown = rows.filter((r) => !words || r.name.toLowerCase().includes(words))

  // The place on screen is itself marked deleted: nothing new goes in it.
  const here =
    level === 'building'
      ? null
      : level === 'floor'
        ? buildingRef
        : level === 'room'
          ? floorRef
          : roomRef
  const hereDeleted =
    here !== null &&
    lines.some(
      (l) =>
        (l.kind === 'delete_building' && `b${l.buildingId}` === here) ||
        (l.kind === 'delete_floor' && `f${l.floorId}` === here) ||
        (l.kind === 'delete_room' && `r${l.roomId}` === here),
    )

  const crumbs = [
    { label: 'Buildings', to: {} },
    buildingRef && { label: placeLabel(buildingRef, lines, index), to: { building: buildingRef } },
    floorRef && {
      label: placeLabel(floorRef, lines, index).split(' · ').pop()!,
      to: { building: buildingRef!, floor: floorRef },
    },
    roomRef && { label: placeLabel(roomRef, lines, index).split(' · ').pop()!, to: null },
  ].filter(Boolean) as { label: string; to: Parameters<typeof go>[0] | null }[]

  const change = (next: Line[]) => onChange?.(next)

  return (
    <div className="flex flex-col gap-4">
      <nav aria-label="Location" className="flex flex-wrap items-center gap-1 text-sm">
        {crumbs.map((c, i) => (
          <span key={i} className="flex items-center gap-1">
            {i > 0 && <Icon name="back" className="text-muted size-3.5 rotate-180" />}
            {c.to ? (
              <button
                type="button"
                onClick={() => go(c.to!)}
                className="text-muted hover:text-ink underline-offset-2 hover:underline"
              >
                {c.label}
              </button>
            ) : (
              <span className="font-medium">{c.label}</span>
            )}
          </span>
        ))}
      </nav>

      <div className="flex flex-wrap items-center gap-2">
        <SearchInput
          label={`Search ${NOUN[level]}s`}
          placeholder={`Search ${NOUN[level]}s`}
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        {editable && !hereDeleted && (
          <Button variant="secondary" onClick={() => setDialog({ type: 'add', level })}>
            <Icon name="plus" className="size-4" />
            New {NOUN[level]}
          </Button>
        )}
      </div>

      {level === 'asset' && roomAssets.isPending && roomId !== null ? (
        <LoadingBlock rows={3} />
      ) : level === 'asset' && roomAssets.error ? (
        <ErrorBlock
          what="the assets"
          message={roomAssets.error.message}
          onRetry={() => void roomAssets.refetch()}
        />
      ) : shown.length === 0 ? (
        <EmptyBlock>
          {rows.length === 0 ? `No ${NOUN[level]} here yet.` : 'Nothing matches.'}
        </EmptyBlock>
      ) : (
        <ul className="border-border bg-surface rounded-card flex flex-col border">
          {shown.map((row) => (
            <TreeRow
              key={row.ref}
              row={row}
              onOpen={
                level === 'asset'
                  ? undefined
                  : () =>
                      go(
                        level === 'building'
                          ? { building: row.ref }
                          : level === 'floor'
                            ? { building: buildingRef!, floor: row.ref }
                            : { building: buildingRef!, floor: floorRef!, room: row.ref },
                      )
              }
              actions={
                editable && (
                  <RowActions
                    row={row}
                    level={level}
                    onEdit={() => setDialog({ type: 'edit', level, row })}
                    onMove={() => setDialog({ type: 'move', row })}
                    onDelete={() => setDialog({ type: 'delete', level, row })}
                    onUndo={() => change(withoutLine(lines, row.line!.key))}
                  />
                )
              }
            />
          ))}
        </ul>
      )}

      {dialog && editable && (
        <ChangeDialog
          state={dialog}
          lines={lines}
          tree={tree}
          index={index}
          parent={here}
          roomAssets={roomAssets.data ?? []}
          types={types}
          makers={makers}
          onClose={() => setDialog(null)}
          onDone={(next) => {
            change(next)
            setDialog(null)
          }}
        />
      )}
    </div>
  )
}

const MARKS: Record<
  NonNullable<Row['mark']>,
  { label: string; tone: 'positive' | 'accent' | 'negative' | 'warning' }
> = {
  new: { label: 'New', tone: 'positive' },
  edited: { label: 'Edited', tone: 'accent' },
  deleted: { label: 'Will be deleted', tone: 'negative' },
  'moving-out': { label: 'Moving out', tone: 'warning' },
  'moving-in': { label: 'Moving here', tone: 'positive' },
}

function TreeRow({ row, onOpen, actions }: { row: Row; onOpen?: () => void; actions: ReactNode }) {
  return (
    <li className="border-rule flex items-center gap-3 border-b px-4 py-3 last:border-0">
      <div className="flex min-w-0 flex-1 flex-col gap-0.5">
        <div className="flex flex-wrap items-center gap-2">
          {onOpen ? (
            <button
              type="button"
              onClick={onOpen}
              className={cn(
                'text-ink text-left font-medium underline-offset-2 hover:underline',
                row.mark === 'deleted' && 'line-through',
              )}
            >
              {row.name}
            </button>
          ) : (
            <span className={cn('font-medium', row.mark === 'deleted' && 'line-through')}>
              {row.name}
            </span>
          )}
          {row.mark && <Badge tone={MARKS[row.mark].tone}>{MARKS[row.mark].label}</Badge>}
          {row.dirty && !row.mark && (
            <span
              className="bg-negative size-2 rounded-full"
              title="Something inside changes"
              aria-label="Something inside changes"
            />
          )}
        </div>
        {(row.note ?? row.sub) && <p className="text-muted text-[13px]">{row.note ?? row.sub}</p>}
        {row.problem && <p className="text-negative text-[13px]">{row.problem}</p>}
      </div>
      {actions}
    </li>
  )
}

function RowActions({
  row,
  level,
  onEdit,
  onMove,
  onDelete,
  onUndo,
}: {
  row: Row
  level: Level
  onEdit: () => void
  onMove: () => void
  onDelete: () => void
  onUndo: () => void
}) {
  const deleted = row.mark === 'deleted'
  // A new one is removed with its line; a changed one gets its line undone.
  const undoable = row.line !== null && row.mark !== 'new'
  return (
    <div className="flex flex-none items-center gap-1">
      {!deleted && (
        <Button variant="ghost" onClick={onEdit}>
          {level === 'floor' || level === 'room' ? 'Rename' : 'Edit'}
        </Button>
      )}
      {level === 'asset' && !deleted && (
        <Button variant="ghost" onClick={onMove}>
          Move
        </Button>
      )}
      {undoable ? (
        <Button variant="ghost" onClick={onUndo}>
          Undo
        </Button>
      ) : (
        <Button variant="ghost" className="text-muted! hover:text-negative!" onClick={onDelete}>
          {row.mark === 'new' ? 'Remove' : 'Delete'}
        </Button>
      )}
    </div>
  )
}

type DialogState =
  | { type: 'add'; level: Level }
  | { type: 'edit'; level: Level; row: Row }
  | { type: 'move'; row: Row }
  | { type: 'delete'; level: Level; row: Row }

const DELETE_KIND = {
  building: 'delete_building',
  floor: 'delete_floor',
  room: 'delete_room',
  asset: 'retire_asset',
} as const
const EDIT_KIND = {
  building: 'edit_building',
  floor: 'rename_floor',
  room: 'rename_room',
  asset: 'edit_asset',
} as const
const ADD_KIND = {
  building: 'add_building',
  floor: 'add_floor',
  room: 'add_room',
  asset: 'add_asset',
} as const

/** The edit line of an existing asset as it is now (an edit or a move starts from it). */
function assetLine(a: RoomAsset, roomRef: string, key: string, makers: ManufacturerOption[]): Line {
  return placedIn(
    {
      ...emptyLine('edit_asset', key),
      assetId: a.id,
      name: a.name,
      typeId: a.typeId,
      manufacturerId: makers.find((m) => m.name === a.manufacturerName)?.id ?? null,
      hasCopyright: a.hasCopyright,
      warrantyEndDate: a.warrantyEndDate,
    },
    roomRef,
  )
}

/** Puts a line in the list in place of the one with the same key (or of the one replaced). */
function put(lines: Line[], line: Line, replacing?: Line | null): Line[] {
  const key = replacing?.key ?? line.key
  return lines.some((l) => l.key === key)
    ? lines.map((l) => (l.key === key ? { ...line, key } : l))
    : [...lines, line]
}

function ChangeDialog({
  state,
  lines,
  tree,
  index,
  parent,
  roomAssets,
  types,
  makers,
  onClose,
  onDone,
}: {
  state: DialogState
  lines: Line[]
  tree: BuildingNode[]
  index: TreeIndex
  /** The place on screen (null on the building list). */
  parent: string | null
  roomAssets: RoomAsset[]
  types: AssetTypeOption[]
  makers: ManufacturerOption[]
  onClose: () => void
  onDone: (lines: Line[]) => void
}) {
  const key = nextKey(lines)

  if (state.type === 'delete') {
    const { row, level } = state
    const removeNew = row.mark === 'new'
    return (
      <ConfirmDialog
        open
        onClose={onClose}
        title={removeNew ? `Remove the new ${NOUN[level]}` : `Delete ${NOUN[level]}`}
        description={
          removeNew
            ? `${row.name} leaves this list, with anything you added inside it.`
            : level === 'asset'
              ? `${row.name} is marked for deletion: once approved it leaves the lists, its history stays.`
              : `${row.name} is marked for deletion: once approved it is hidden everywhere (its history stays). Everything inside it must be deleted or moved in this list too.`
        }
        confirmLabel={removeNew ? 'Remove' : 'Mark for deletion'}
        onConfirm={() => {
          if (removeNew) return onDone(withoutLine(lines, row.line!.key))
          const target =
            level === 'building'
              ? { buildingId: row.id }
              : level === 'floor'
                ? { floorId: row.id }
                : level === 'room'
                  ? { roomId: row.id }
                  : { assetId: row.id }
          onDone(put(lines, { ...emptyLine(DELETE_KIND[level], key), ...target }, row.line))
        }}
      />
    )
  }

  if (state.type === 'move') {
    const { row } = state
    const base =
      row.line ??
      assetLine(
        roomAssets.find((a) => a.id === row.id)!,
        parent!,
        key,
        makers,
      )
    return (
      <MoveDialog
        tree={tree}
        lines={lines}
        index={index}
        current={parentRef(base)}
        name={row.name}
        onClose={onClose}
        onMove={(room) => onDone(put(lines, placedIn(base, room), row.line))}
      />
    )
  }

  const { level } = state
  const row = state.type === 'edit' ? state.row : null
  // The line the dialog fills: the row's own line (a new one, or an earlier edit), or a fresh one.
  const start: Line =
    row?.line && row.line.kind !== 'retire_asset'
      ? row.line
      : row
        ? level === 'asset'
          ? assetLine(
              roomAssets.find((a) => a.id === row.id)!,
              parent!,
              key,
              makers,
            )
          : {
              ...emptyLine(EDIT_KIND[level], key),
              ...(level === 'building'
                ? {
                    buildingId: row.id,
                    name: row.name,
                    address: row.sub === 'No address' ? null : row.sub,
                  }
                : level === 'floor'
                  ? { floorId: row.id, name: row.name.replace(/^Floor /, '') }
                  : { roomId: row.id, name: row.name }),
            }
        : level === 'building'
          ? emptyLine(ADD_KIND.building, key)
          : placedIn(emptyLine(ADD_KIND[level], key), parent!)

  return level === 'asset' ? (
    <AssetDialog
      title={row ? `Edit ${row.name}` : 'New asset'}
      line={start}
      types={types}
      makers={makers}
      onClose={onClose}
      onSave={(line) => onDone(put(lines, line, row?.line))}
    />
  ) : (
    <PlaceDialog
      level={level}
      title={row ? `${level === 'building' ? 'Edit' : 'Rename'} ${row.name}` : `New ${NOUN[level]}`}
      line={start}
      onClose={onClose}
      onSave={(line) => onDone(put(lines, line, row?.line))}
    />
  )
}

function PlaceDialog({
  level,
  title,
  line,
  onClose,
  onSave,
}: {
  level: Level
  title: string
  line: Line
  onClose: () => void
  onSave: (line: Line) => void
}) {
  const [name, setName] = useState(line.name ?? '')
  const [address, setAddress] = useState(line.address ?? '')
  const value = name.trim()
  const error =
    level === 'floor'
      ? /^[A-Za-z0-9]{1,10}$/.test(value)
        ? null
        : 'Letters and digits, e.g. G, B1, 12.'
      : level === 'building'
        ? value.length >= 2 && value.length <= 100
          ? null
          : '2 to 100 characters.'
        : value.length >= 1 && value.length <= 50
          ? null
          : '1 to 50 characters.'
  const [tried, setTried] = useState(false)
  return (
    <Dialog open onClose={onClose} title={title}>
      <form
        noValidate
        className="flex flex-col gap-4"
        onSubmit={(e) => {
          e.preventDefault()
          setTried(true)
          if (!error) onSave({ ...line, name: value, address: address.trim() || null })
        }}
      >
        <TextField
          label={
            level === 'floor' ? 'Floor number' : level === 'room' ? 'Room number or name' : 'Name'
          }
          hint={
            level === 'floor'
              ? 'G for ground, B1 for the first basement, then 1, 2, 3…'
              : level === 'room'
                ? 'e.g. 503, Lobby or Restroom G'
                : undefined
          }
          value={name}
          error={tried ? (error ?? undefined) : undefined}
          onChange={(e) => setName(e.target.value)}
        />
        {level === 'building' && (
          <TextField
            label="Address"
            hint="Optional"
            value={address}
            onChange={(e) => setAddress(e.target.value)}
          />
        )}
        <DialogActions>
          <Button variant="ghost" onClick={onClose}>
            Back
          </Button>
          <Button type="submit" variant="primary">
            Save
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}

function AssetDialog({
  title,
  line,
  types,
  makers,
  onClose,
  onSave,
}: {
  title: string
  line: Line
  types: AssetTypeOption[]
  makers: ManufacturerOption[]
  onClose: () => void
  onSave: (line: Line) => void
}) {
  const [name, setName] = useState(line.name ?? '')
  const [typeId, setTypeId] = useState(line.typeId ? String(line.typeId) : '')
  const [makerId, setMakerId] = useState(line.manufacturerId ? String(line.manufacturerId) : '')
  const [warranty, setWarranty] = useState(line.warrantyEndDate ?? '')
  const [copyright, setCopyright] = useState(line.hasCopyright ?? false)
  const [tried, setTried] = useState(false)
  const nameError =
    name.trim().length < 2 || name.trim().length > 150 ? '2 to 150 characters.' : null
  const typeError = typeId === '' ? 'Choose the asset type.' : null
  return (
    <Dialog open onClose={onClose} title={title}>
      <form
        noValidate
        className="flex flex-col gap-4"
        onSubmit={(e) => {
          e.preventDefault()
          setTried(true)
          if (nameError || typeError) return
          onSave({
            ...line,
            name: name.trim(),
            typeId: Number(typeId),
            manufacturerId: makerId ? Number(makerId) : null,
            warrantyEndDate: warranty || null,
            hasCopyright: copyright,
          })
        }}
      >
        <TextField
          label="Name"
          placeholder="e.g. Split AC Daikin 1.5HP (#AC-503)"
          value={name}
          error={tried ? (nameError ?? undefined) : undefined}
          onChange={(e) => setName(e.target.value)}
        />
        <Select
          label="Type"
          placeholder="Choose…"
          value={typeId}
          error={tried ? (typeError ?? undefined) : undefined}
          onChange={(e) => setTypeId(e.target.value)}
        >
          {types.map((t) => (
            <option key={t.id} value={t.id}>
              {t.categoryName} · {t.name}
            </option>
          ))}
        </Select>
        <div className="grid gap-4 sm:grid-cols-2">
          <Select label="Manufacturer" value={makerId} onChange={(e) => setMakerId(e.target.value)}>
            <option value="">Unknown</option>
            {makers.map((m) => (
              <option key={m.id} value={m.id}>
                {m.name}
              </option>
            ))}
          </Select>
          <TextField
            label="Warranty until"
            type="date"
            value={warranty}
            onChange={(e) => setWarranty(e.target.value)}
          />
        </div>
        <label className="flex items-center gap-2 text-sm">
          <input
            type="checkbox"
            className="accent-primary size-4"
            checked={copyright}
            onChange={(e) => setCopyright(e.target.checked)}
          />
          Copyrighted: only the manufacturer may service it
        </label>
        <DialogActions>
          <Button variant="ghost" onClick={onClose}>
            Back
          </Button>
          <Button type="submit" variant="primary">
            Save
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}

/** Chooses another room (existing or new in this list) for an asset: building, then floor, then room. */
function MoveDialog({
  tree,
  lines,
  index,
  current,
  name,
  onClose,
  onMove,
}: {
  tree: BuildingNode[]
  lines: Line[]
  index: TreeIndex
  current: string | null
  name: string
  onClose: () => void
  onMove: (roomRef: string) => void
}) {
  const none = new Set<string>()
  const usable = (rows: Row[]) => rows.filter((r) => r.mark !== 'deleted')
  const [building, setBuilding] = useState('')
  const [floor, setFloor] = useState('')
  const [room, setRoom] = useState('')
  const buildings = usable(buildingRows(tree, lines, none))
  const floors = building ? usable(floorRows(tree, building, lines, none)) : []
  const rooms = floor
    ? usable(roomRows(tree, floor, lines, none)).filter((r) => r.ref !== current)
    : []
  return (
    <Dialog
      open
      onClose={onClose}
      title={`Move ${name}`}
      description={current ? `Now in ${placeLabel(current, lines, index)}.` : undefined}
    >
      <div className="flex flex-col gap-4">
        <Select
          label="Building"
          placeholder="Choose…"
          value={building}
          onChange={(e) => (setBuilding(e.target.value), setFloor(''), setRoom(''))}
        >
          {buildings.map((b) => (
            <option key={b.ref} value={b.ref}>
              {b.name}
            </option>
          ))}
        </Select>
        <Select
          label="Floor"
          placeholder="Choose…"
          value={floor}
          disabled={!building}
          onChange={(e) => (setFloor(e.target.value), setRoom(''))}
        >
          {floors.map((f) => (
            <option key={f.ref} value={f.ref}>
              {f.name}
            </option>
          ))}
        </Select>
        <Select
          label="Room"
          placeholder="Choose…"
          value={room}
          disabled={!floor}
          onChange={(e) => setRoom(e.target.value)}
        >
          {rooms.map((r) => (
            <option key={r.ref} value={r.ref}>
              {r.name}
            </option>
          ))}
        </Select>
        <DialogActions>
          <Button variant="ghost" onClick={onClose}>
            Back
          </Button>
          <Button variant="primary" disabled={!room} onClick={() => onMove(room)}>
            Move here
          </Button>
        </DialogActions>
      </div>
    </Dialog>
  )
}
