import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link } from 'react-router'
import { paths } from '@/app/paths'
import { useAuth } from '@/features/auth'
import {
  Alert,
  Button,
  Card,
  ConfirmDialog,
  Dialog,
  DialogActions,
  EmptyBlock,
  ErrorBlock,
  LoadingBlock,
  PageHeader,
  TextField,
} from '@/shared/ui'
import { facilitiesApi } from '../api'
import { useFacilityMutation, useFacilityTree } from '../hooks'
import {
  buildingSchema,
  floorSchema,
  roomSchema,
  type BuildingFormValues,
  type FloorFormValues,
  type RoomFormValues,
} from '../schemas'
import type { BuildingNode, FloorNode, RoomNode } from '../types'

/** What the open dialog edits. "parentId" = the building of a new floor / the floor of a new room. */
type Editing =
  | { kind: 'building'; item?: BuildingNode }
  | { kind: 'floor'; parentId: number; item?: FloorNode }
  | { kind: 'room'; parentId: number; item?: RoomNode }
  | { kind: 'delete'; what: 'building' | 'floor' | 'room'; id: number; label: string }

/**
 * AST-01 Facility tree: building -> floor -> room (three levels, Screen Catalog rule 15). Staff and managers read
 * their own buildings; only the Administrator adds, renames and deletes. A place that still holds something is never
 * deleted (BR-10).
 */
export function FacilityTreePage() {
  const { user } = useAuth()
  const tree = useFacilityTree()
  const [editing, setEditing] = useState<Editing | null>(null)
  const isAdmin = user?.role === 'admin'
  const close = () => setEditing(null)

  const buildings = tree.data ?? []
  const rooms = buildings.flatMap((b) => b.floors.flatMap((f) => f.rooms))
  const assets = rooms.reduce((n, r) => n + r.assetCount, 0)
  const vacant = rooms.filter((r) => !r.isCommonArea && r.tenants.length === 0).length

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title="Buildings"
        description={
          tree.data
            ? `${buildings.length} ${buildings.length === 1 ? 'building' : 'buildings'}, ${rooms.length} rooms, ${assets} assets in service, ${vacant} rooms without a tenant`
            : undefined
        }
        actions={
          isAdmin && (
            <Button variant="primary" onClick={() => setEditing({ kind: 'building' })}>
              New building
            </Button>
          )
        }
      />

      {tree.isPending ? (
        <LoadingBlock rows={6} />
      ) : tree.error ? (
        <ErrorBlock
          what="the buildings"
          message={tree.error.message}
          onRetry={() => void tree.refetch()}
        />
      ) : buildings.length === 0 ? (
        <EmptyBlock>
          {isAdmin ? 'No building yet. Add the first one.' : 'No building is assigned to you.'}
        </EmptyBlock>
      ) : (
        buildings.map((b) => (
          <Card key={b.id} padding="none">
            <div className="border-rule flex flex-wrap items-start justify-between gap-3 border-b px-5 py-4">
              <div>
                <h2 className="text-section font-semibold">{b.name}</h2>
                <p className="text-muted text-[13px]">{b.address ?? 'No address'}</p>
              </div>
              {isAdmin && (
                <div className="flex gap-1">
                  <Button
                    variant="ghost"
                    onClick={() => setEditing({ kind: 'floor', parentId: b.id })}
                  >
                    Add floor
                  </Button>
                  <Button variant="ghost" onClick={() => setEditing({ kind: 'building', item: b })}>
                    Edit
                  </Button>
                  <Button
                    variant="ghost"
                    onClick={() =>
                      setEditing({ kind: 'delete', what: 'building', id: b.id, label: b.name })
                    }
                  >
                    Delete
                  </Button>
                </div>
              )}
            </div>
            {b.floors.length === 0 ? (
              <p className="text-muted px-5 py-4 text-sm">No floor yet.</p>
            ) : (
              b.floors.map((f) => (
                <section key={f.id} className="border-rule border-b px-5 py-3 last:border-0">
                  <div className="flex items-center justify-between gap-3">
                    <h3 className="text-sm font-medium">{f.name}</h3>
                    {isAdmin && (
                      <div className="flex gap-1">
                        <Button
                          variant="ghost"
                          onClick={() => setEditing({ kind: 'room', parentId: f.id })}
                        >
                          Add room
                        </Button>
                        <Button
                          variant="ghost"
                          onClick={() => setEditing({ kind: 'floor', parentId: b.id, item: f })}
                        >
                          Rename
                        </Button>
                        <Button
                          variant="ghost"
                          onClick={() =>
                            setEditing({ kind: 'delete', what: 'floor', id: f.id, label: f.name })
                          }
                        >
                          Delete
                        </Button>
                      </div>
                    )}
                  </div>
                  {f.rooms.length === 0 ? (
                    <p className="text-muted py-2 text-sm">No room yet.</p>
                  ) : (
                    <ul className="flex flex-col">
                      {f.rooms.map((r) => (
                        <li
                          key={r.id}
                          className="flex flex-wrap items-center justify-between gap-2 py-1.5 text-sm"
                        >
                          <span className="min-w-0">
                            <span className="font-medium">{r.roomNumber}</span>
                            <span className="text-muted">
                              {' · '}
                              {r.isCommonArea
                                ? 'Common area'
                                : r.tenants.length > 0
                                  ? r.tenants.join(', ')
                                  : 'No tenant'}
                            </span>
                          </span>
                          <span className="flex items-center gap-1">
                            <Link
                              to={`${paths.assets}?roomId=${r.id}`}
                              className="text-muted px-2 tabular-nums underline-offset-2 hover:underline"
                            >
                              {r.assetCount} {r.assetCount === 1 ? 'asset' : 'assets'}
                            </Link>
                            {isAdmin && (
                              <>
                                <Button
                                  variant="ghost"
                                  onClick={() =>
                                    setEditing({ kind: 'room', parentId: f.id, item: r })
                                  }
                                >
                                  Edit
                                </Button>
                                <Button
                                  variant="ghost"
                                  onClick={() =>
                                    setEditing({
                                      kind: 'delete',
                                      what: 'room',
                                      id: r.id,
                                      label: r.roomNumber,
                                    })
                                  }
                                >
                                  Delete
                                </Button>
                              </>
                            )}
                          </span>
                        </li>
                      ))}
                    </ul>
                  )}
                </section>
              ))
            )}
          </Card>
        ))
      )}

      {editing?.kind === 'building' && <BuildingDialog item={editing.item} onClose={close} />}
      {editing?.kind === 'floor' && (
        <FloorDialog buildingId={editing.parentId} item={editing.item} onClose={close} />
      )}
      {editing?.kind === 'room' && (
        <RoomDialog floorId={editing.parentId} item={editing.item} onClose={close} />
      )}
      {editing?.kind === 'delete' && (
        <DeleteDialog what={editing.what} id={editing.id} label={editing.label} onClose={close} />
      )}
    </div>
  )
}

function BuildingDialog({ item, onClose }: { item?: BuildingNode; onClose: () => void }) {
  const save = useFacilityMutation((v: BuildingFormValues) =>
    item
      ? facilitiesApi.updateBuilding({ id: item.id, name: v.name, address: v.address || null })
      : facilitiesApi.createBuilding({ name: v.name, address: v.address || null }),
  )
  const { register, handleSubmit, formState } = useForm<BuildingFormValues>({
    resolver: zodResolver(buildingSchema),
    defaultValues: { name: item?.name ?? '', address: item?.address ?? '' },
  })
  return (
    <Dialog open onClose={onClose} title={item ? 'Edit building' : 'New building'}>
      <form
        onSubmit={handleSubmit((v) => save.mutate(v, { onSuccess: onClose }))}
        noValidate
        className="flex flex-col gap-4"
      >
        <TextField label="Name" error={formState.errors.name?.message} {...register('name')} />
        <TextField
          label="Address"
          hint="Optional"
          error={formState.errors.address?.message}
          {...register('address')}
        />
        {save.error && <Alert>{save.error.message}</Alert>}
        <DialogActions>
          <Button variant="ghost" onClick={onClose}>
            Back
          </Button>
          <Button type="submit" variant="primary" loading={save.isPending}>
            Save
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}

function FloorDialog({
  buildingId,
  item,
  onClose,
}: {
  buildingId: number
  item?: FloorNode
  onClose: () => void
}) {
  const save = useFacilityMutation((v: FloorFormValues) =>
    item
      ? facilitiesApi.updateFloor({ id: item.id, buildingId, floorNumber: v.floorNumber })
      : facilitiesApi.createFloor({ buildingId, floorNumber: v.floorNumber }),
  )
  const { register, handleSubmit, formState } = useForm<FloorFormValues>({
    resolver: zodResolver(floorSchema),
    defaultValues: { floorNumber: item?.floorNumber ?? '' },
  })
  return (
    <Dialog open onClose={onClose} title={item ? 'Rename floor' : 'New floor'}>
      <form
        onSubmit={handleSubmit((v) => save.mutate(v, { onSuccess: onClose }))}
        noValidate
        className="flex flex-col gap-4"
      >
        <TextField
          label="Floor number"
          hint="G for ground, B1 for the first basement, then 1, 2, 3..."
          error={formState.errors.floorNumber?.message}
          {...register('floorNumber')}
        />
        {save.error && <Alert>{save.error.message}</Alert>}
        <DialogActions>
          <Button variant="ghost" onClick={onClose}>
            Back
          </Button>
          <Button type="submit" variant="primary" loading={save.isPending}>
            Save
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}

function RoomDialog({
  floorId,
  item,
  onClose,
}: {
  floorId: number
  item?: RoomNode
  onClose: () => void
}) {
  const save = useFacilityMutation((v: RoomFormValues) =>
    item
      ? facilitiesApi.updateRoom({ id: item.id, floorId, ...v })
      : facilitiesApi.createRoom({ floorId, ...v }),
  )
  const { register, handleSubmit, formState } = useForm<RoomFormValues>({
    resolver: zodResolver(roomSchema),
    defaultValues: {
      roomNumber: item?.roomNumber ?? '',
      isCommonArea: item?.isCommonArea ?? false,
    },
  })
  return (
    <Dialog open onClose={onClose} title={item ? 'Edit room' : 'New room'}>
      <form
        onSubmit={handleSubmit((v) => save.mutate(v, { onSuccess: onClose }))}
        noValidate
        className="flex flex-col gap-4"
      >
        <TextField
          label="Room number or name"
          hint="501 for a rented room, Lobby or Restroom G for a common area"
          error={formState.errors.roomNumber?.message}
          {...register('roomNumber')}
        />
        <label className="flex items-center gap-2 text-sm">
          <input type="checkbox" className="accent-primary size-4" {...register('isCommonArea')} />
          Common area (every tenant of the building may report problems here)
        </label>
        {save.error && <Alert>{save.error.message}</Alert>}
        <DialogActions>
          <Button variant="ghost" onClick={onClose}>
            Back
          </Button>
          <Button type="submit" variant="primary" loading={save.isPending}>
            Save
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}

const DELETE_API = {
  building: facilitiesApi.deleteBuilding,
  floor: facilitiesApi.deleteFloor,
  room: facilitiesApi.deleteRoom,
}

function DeleteDialog({
  what,
  id,
  label,
  onClose,
}: {
  what: 'building' | 'floor' | 'room'
  id: number
  label: string
  onClose: () => void
}) {
  const remove = useFacilityMutation(DELETE_API[what])
  return (
    <ConfirmDialog
      open
      onClose={onClose}
      title={`Delete ${what}`}
      description={`Delete ${label}? Only an empty ${what} can be deleted.`}
      confirmLabel="Delete"
      onConfirm={() => remove.mutate({ id }, { onSuccess: onClose })}
      loading={remove.isPending}
      error={remove.error?.message}
    />
  )
}
