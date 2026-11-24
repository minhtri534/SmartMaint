/**
 * Mirrors backend Modules/AssetChanges/Dtos (meeting note 18/11, decision 7; tree version 24/11): a staff member edits
 * buildings, floors, rooms and assets on the building tree; every edit is a line of their draft (saved after each
 * edit). A manager of the buildings it touches approves it (every line applied at once) or rejects it with a reason.
 */

export type ChangeListStatus = 'draft' | 'pending' | 'approved' | 'rejected'
export const CHANGE_LIST_STATUS_LABELS: Record<ChangeListStatus, string> = {
  draft: 'Draft',
  pending: 'Waiting for approval',
  approved: 'Approved',
  rejected: 'Rejected',
}

export type ChangeKind =
  | 'add_building'
  | 'edit_building'
  | 'delete_building'
  | 'add_floor'
  | 'rename_floor'
  | 'delete_floor'
  | 'add_room'
  | 'rename_room'
  | 'delete_room'
  | 'add_asset'
  | 'edit_asset'
  | 'retire_asset'
  | 'delete_asset'

export const CHANGE_KIND_LABELS: Record<ChangeKind, string> = {
  add_building: 'New building',
  edit_building: 'Edit building',
  delete_building: 'Delete building',
  add_floor: 'New floor',
  rename_floor: 'Rename floor',
  delete_floor: 'Delete floor',
  add_room: 'New room',
  rename_room: 'Rename room',
  delete_room: 'Delete room',
  add_asset: 'New asset',
  edit_asset: 'Edit or move asset',
  retire_asset: 'Delete asset',
  delete_asset: 'Delete asset',
}

export type ChangeListAction = 'edit' | 'submit' | 'delete' | 'approve' | 'reject'

export interface PersonRef {
  id: number
  fullName: string
}

/** One line as the API takes it. */
export interface ChangeItemRequest {
  kind: ChangeKind
  /** "n1", "n2"… unique in the list. */
  key: string
  /** The key of the new building / floor / room line this line goes in. */
  parentKey: string | null
  buildingId: number | null
  floorId: number | null
  roomId: number | null
  assetId: number | null
  name: string | null
  address: string | null
  typeId: number | null
  manufacturerId: number | null
  hasCopyright: boolean | null
  /** yyyy-MM-dd */
  warrantyEndDate: string | null
}

/** Where the existing thing a line works on is now. */
export interface ChangePlace {
  buildingId: number
  floorId: number | null
  roomId: number | null
}

export interface ChangeItem extends ChangeItemRequest {
  id: number
  position: number
  typeName: string | null
  manufacturerName: string | null
  /** What the line changes, as it read when saved. */
  targetLabel: string | null
  at: ChangePlace | null
  /** Why it cannot be applied now; null = fine. */
  problem: string | null
}

export interface ChangeListRequest {
  title: string
  note: string | null
  items: ChangeItemRequest[]
}

export interface ChangeListSummary {
  id: number
  code: string
  title: string
  status: ChangeListStatus
  /** The buildings it touches, new ones included. */
  buildingNames: string[]
  itemCount: number
  staff: PersonRef
  reviewer: PersonRef | null
  updatedAt: string
  submittedAt: string | null
  reviewedAt: string | null
}

export interface ChangeListDetail extends ChangeListSummary {
  note: string | null
  rejectReason: string | null
  items: ChangeItem[]
  allowedActions: ChangeListAction[]
}

/** Reference data for the asset dialog. */
export interface AssetTypeOption {
  id: number
  name: string
  categoryName: string
}

export interface ManufacturerOption {
  id: number
  name: string
}

/** An asset of a room, from the asset register. */
export interface RoomAsset {
  id: number
  name: string
  typeId: number
  typeName: string
  manufacturerName: string | null
  hasCopyright: boolean
  warrantyEndDate: string | null
  openCases: number
}
