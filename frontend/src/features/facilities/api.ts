import { apiClient } from '@/lib/api-client'
import type {
  BuildingNode,
  BuildingRequest,
  FloorNode,
  FloorRequest,
  RoomNode,
  RoomRequest,
} from './types'

type Id = { id: number }

/** One function per endpoint of FacilitiesController. Writes are Admin only. */
export const facilitiesApi = {
  /** GET /api/facilities/tree — AST-01 (staff / manager: own buildings) */
  tree: () => apiClient.get<BuildingNode[]>('/facilities/tree').then((r) => r.data),
  /** POST /api/facilities/buildings */
  createBuilding: (body: BuildingRequest) =>
    apiClient.post<BuildingNode>('/facilities/buildings', body).then((r) => r.data),
  /** PUT /api/facilities/buildings/{id} */
  updateBuilding: ({ id, ...body }: Id & BuildingRequest) =>
    apiClient.put<BuildingNode>(`/facilities/buildings/${id}`, body).then((r) => r.data),
  /** DELETE /api/facilities/buildings/{id} */
  deleteBuilding: ({ id }: Id) =>
    apiClient.delete(`/facilities/buildings/${id}`).then(() => undefined),
  /** POST /api/facilities/floors */
  createFloor: (body: FloorRequest) =>
    apiClient.post<FloorNode>('/facilities/floors', body).then((r) => r.data),
  /** PUT /api/facilities/floors/{id} */
  updateFloor: ({ id, ...body }: Id & FloorRequest) =>
    apiClient.put<FloorNode>(`/facilities/floors/${id}`, body).then((r) => r.data),
  /** DELETE /api/facilities/floors/{id} */
  deleteFloor: ({ id }: Id) => apiClient.delete(`/facilities/floors/${id}`).then(() => undefined),
  /** POST /api/facilities/rooms */
  createRoom: (body: RoomRequest) =>
    apiClient.post<RoomNode>('/facilities/rooms', body).then((r) => r.data),
  /** PUT /api/facilities/rooms/{id} */
  updateRoom: ({ id, ...body }: Id & RoomRequest) =>
    apiClient.put<RoomNode>(`/facilities/rooms/${id}`, body).then((r) => r.data),
  /** DELETE /api/facilities/rooms/{id} */
  deleteRoom: ({ id }: Id) => apiClient.delete(`/facilities/rooms/${id}`).then(() => undefined),
}

export const facilityKeys = {
  tree: ['facilities', 'tree'] as const,
}
