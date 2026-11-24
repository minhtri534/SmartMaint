import { apiClient } from '@/lib/api-client'
import type { BuildingNode } from './types'

/** One function per endpoint of FacilitiesController (read only: every change goes through asset-changes). */
export const facilitiesApi = {
  /** GET /api/facilities/tree — AST-01 (staff / manager: own buildings) */
  tree: () => apiClient.get<BuildingNode[]>('/facilities/tree').then((r) => r.data),
}

export const facilityKeys = {
  tree: ['facilities', 'tree'] as const,
}
