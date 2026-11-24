/** Mirrors backend Modules/CatalogAdmin/Dtos. "usedBy" = rows that point at the item (0 = can delete). */

/** ERD: CaseCategories (+ NOT IN ERD v5 asset type, archive). assetTypeId empty = every asset type. */
export interface CaseCategory {
  id: number
  groupName: string
  name: string
  description: string | null
  assetTypeId: number | null
  assetTypeName: string | null
  archivedAt: string | null
  usedBy: number
}

/** ERD: AssetCategory */
export interface AssetCategory {
  id: number
  code: string
  categoryName: string
  description: string | null
  usedBy: number
  /** Assets of every type in this category (usedBy counts the types, which block a delete). */
  assetCount: number
}

/** ERD: AssetType */
export interface AssetType {
  id: number
  categoryId: number
  categoryName: string
  code: string
  name: string
  description: string | null
  /** NOT IN ERD v5 (F5 R03): normal life in years. */
  expectedLifeYears: number | null
  /** Its assets + the faults linked to it (either blocks a delete). */
  usedBy: number
  assetCount: number
}

/** ERD: Manufacturers */
export interface Manufacturer {
  id: number
  name: string
  phoneNumber: string | null
  email: string | null
  usedBy: number
}

/** ERD: Specialties */
export interface Specialty {
  id: number
  name: string
  archivedAt: string | null
  usedBy: number
}

/** The five lists, also the URL segment of each endpoint (api/catalog/{list}). */
export type CatalogList =
  'case-categories' | 'asset-categories' | 'asset-types' | 'manufacturers' | 'specialties'
