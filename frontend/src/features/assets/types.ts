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

export type AssetAction = 'edit' | 'retire' | 'delete'

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
