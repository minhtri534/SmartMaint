import { apiClient } from '@/lib/api-client'
import type {
  AssetTypeOption,
  ChangeListDetail,
  ChangeListRequest,
  ChangeListSummary,
  ManufacturerOption,
  RoomAsset,
} from './types'

type Id = { id: number }

/** One function per endpoint of AssetChangesController (+ the reference data of the tree editor). */
export const assetChangesApi = {
  /** GET /api/asset-changes — Staff (own lists), Manager (sent lists of their buildings) */
  list: () => apiClient.get<ChangeListSummary[]>('/asset-changes').then((r) => r.data),
  /** GET /api/asset-changes/{id} */
  get: (id: number) => apiClient.get<ChangeListDetail>(`/asset-changes/${id}`).then((r) => r.data),
  /** POST / PUT /api/asset-changes[/{id}] — Staff: the tree saves after every edit */
  save: ({ id, ...body }: { id: number | null } & ChangeListRequest) =>
    (id === null
      ? apiClient.post<ChangeListDetail>('/asset-changes', body)
      : apiClient.put<ChangeListDetail>(`/asset-changes/${id}`, body)
    ).then((r) => r.data),
  /** DELETE /api/asset-changes/{id} — Staff, a draft or rejected list */
  remove: ({ id }: Id) => apiClient.delete(`/asset-changes/${id}`).then(() => undefined),
  /** POST /api/asset-changes/{id}/submit — Staff */
  submit: ({ id }: Id) =>
    apiClient.post<ChangeListDetail>(`/asset-changes/${id}/submit`).then((r) => r.data),
  /** POST /api/asset-changes/{id}/approve — Manager: every line is applied */
  approve: ({ id }: Id) =>
    apiClient.post<ChangeListDetail>(`/asset-changes/${id}/approve`).then((r) => r.data),
  /** POST /api/asset-changes/{id}/reject — Manager */
  reject: ({ id, reason }: Id & { reason: string }) =>
    apiClient.post<ChangeListDetail>(`/asset-changes/${id}/reject`, { reason }).then((r) => r.data),

  /** GET /api/asset-register?roomId= — the assets in service in a room */
  roomAssets: (roomId: number) =>
    apiClient
      .get<{ items: RoomAsset[] }>('/asset-register', {
        params: { roomId, page: 1, pageSize: 100, sort: 'name_asc' },
      })
      .then((r) => r.data.items),
  /** GET /api/asset-types */
  types: () => apiClient.get<AssetTypeOption[]>('/asset-types').then((r) => r.data),
  /** GET /api/manufacturers */
  manufacturers: () => apiClient.get<ManufacturerOption[]>('/manufacturers').then((r) => r.data),
}

export const assetChangeKeys = {
  all: ['asset-changes'] as const,
  list: ['asset-changes', 'list'] as const,
  detail: (id: number) => ['asset-changes', 'detail', id] as const,
  roomAssets: (roomId: number) => ['asset-changes', 'ref', 'room', roomId] as const,
  types: ['asset-changes', 'ref', 'types'] as const,
  manufacturers: ['asset-changes', 'ref', 'manufacturers'] as const,
}
