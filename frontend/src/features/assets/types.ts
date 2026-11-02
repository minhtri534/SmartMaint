/** Mirrors backend Modules/Assets/Dtos. ERD: Assets (+ NOT IN ERD v5 RetiredAt). */

export interface AssetLocation {
  buildingId: number
  buildingName: string
  floorName: string
  roomId: number
  roomName: string
}

export interface AssetSummary {
  id: number
  name: string
  typeId: number
  typeName: string
  categoryName: string
  manufacturerName: string | null
  location: AssetLocation
  /** yyyy-MM-dd */
  warrantyEndDate: string | null
  underWarranty: boolean
  /** Only the manufacturer may service it (E4). */
  hasCopyright: boolean
  retiredAt: string | null
  openCases: number
}

/** 'open_survey': staff, F5 way 2 (the asset fails often). */
export type AssetAction = 'edit' | 'retire' | 'delete' | 'open_survey'

/** A case of the asset (G5). Status values as in features/cases. */
export interface AssetCase {
  id: number
  code: string
  title: string
  status: string
  submittedAt: string
  closedAt: string | null
  rootCause: string | null
}

export interface AssetDetail extends AssetSummary {
  manufacturerId: number | null
  createdAt: string
  caseCount: number
  /** Null for the Administrator: no case content (BR-11). */
  history: AssetCase[] | null
  /** F1 page 3 / F5: repairs in the last 12 months; from 3 the asset "fails often". */
  repairsLast12Months: number
  frequentFailure: boolean
  /** NOT IN ERD v5 (R03): normal life of its type, in years. */
  expectedLifeYears: number | null
  /** NOT IN ERD v5 (A07): installed in place of / replaced by. */
  replaces: AssetRef | null
  replacedBy: AssetRef | null
  allowedActions: AssetAction[]
}

export interface AssetPage {
  items: AssetSummary[]
  page: number
  pageSize: number
  totalCount: number
}

export interface AssetFilters {
  buildingId?: number
  roomId?: number
  typeId?: number
  search?: string
  includeRetired?: boolean
  page: number
}

export interface AssetRequest {
  name: string
  roomId: number
  typeId: number
  manufacturerId: number | null
  hasCopyright: boolean
  warrantyEndDate: string | null
}

export interface ImportRow {
  line: number
  name: string
  errors: string[]
}

export interface ImportResult {
  rowCount: number
  errorCount: number
  imported: boolean
  rows: ImportRow[]
}

export interface QrTarget {
  id: number
  name: string
  location: AssetLocation
  isRetired: boolean
  canReport: boolean
  canOpenDetail: boolean
}

/** Reference data. */
export interface AssetTypeOption {
  id: number
  name: string
  categoryId: number
  categoryName: string
}

export interface ManufacturerOption {
  id: number
  name: string
}

export interface PlaceOption {
  id: number
  name: string
  floors: {
    id: number
    name: string
    rooms: { id: number; name: string; isCommonArea: boolean }[]
  }[]
}

export interface AssetRef {
  id: number
  name: string
}

// ---------------------------------------------------------------------------
// A06 / A07 replacement requests (F5 → F1). Mirrors backend Modules/Assets/Dtos/ReplacementDtos.cs.
// ---------------------------------------------------------------------------

export type ReplacementStatus = 'open' | 'done' | 'dismissed'

export interface ReplacementRequest {
  id: number
  /** A reference to quote; the Administrator cannot open the case (BR-11). */
  caseCode: string
  /** Empty for the Administrator. */
  caseId: number | null
  oldAsset: AssetRef
  oldAssetTypeId: number
  oldAssetTypeName: string
  oldManufacturerId: number | null
  location: AssetLocation
  newModel: string
  newSerial: string | null
  requestedBy: string
  requestedAt: string
  status: ReplacementStatus
  newAsset: AssetRef | null
  finishedAt: string | null
  dismissReason: string | null
}

export interface CompleteReplacementRequest {
  name: string
  typeId: number
  manufacturerId: number | null
  hasCopyright: boolean
  warrantyEndDate: string | null
}

export interface ServiceTypeOption {
  id: number
  groupName: string
  name: string
}
