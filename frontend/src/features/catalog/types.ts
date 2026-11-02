/** Mirrors backend Modules/CatalogAdmin/Dtos. "usedBy" = rows that point at the item (0 = can delete). */

export type Urgency = 'emergency' | 'high' | 'normal' | 'low'
export type CostBearer = 'building' | 'customer'

/** ERD: CaseCategories (+ NOT IN ERD v5 group, emergency, defaults, photo rule). */
export interface CaseCategory {
  id: number
  groupName: string
  name: string
  description: string | null
  isEmergency: boolean
  defaultPriority: Urgency
  defaultCostBearer: CostBearer
  photoRequired: boolean
  usedBy: number
}

/** ERD: AssetCategory */
export interface AssetCategory {
  id: number
  code: string
  categoryName: string
  description: string | null
  usedBy: number
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
  usedBy: number
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
  usedBy: number
}

/** The five lists, also the URL segment of each endpoint (api/catalog/{list}). */
export type CatalogList =
  'case-categories' | 'asset-categories' | 'asset-types' | 'manufacturers' | 'specialties'
