import { apiClient } from '@/lib/api-client'
import type {
  AssetDetail,
  AssetFilters,
  AssetPage,
  AssetRequest,
  AssetTypeOption,
  ImportResult,
  ManufacturerOption,
  PlaceOption,
  QrTarget,
} from './types'

type Id = { id: number }

/** One function per endpoint of AssetsController (+ reference data). Writes are Admin only. */
export const assetsApi = {
  /** GET /api/asset-register — AST-02 */
  list: ({ page, ...filters }: AssetFilters) =>
    apiClient
      .get<AssetPage>('/asset-register', { params: { ...filters, page, pageSize: 25 } })
      .then((r) => r.data),
  /** The assets matching the filters, for printing QR labels (at most 100). */
  listForLabels: (filters: Omit<AssetFilters, 'page'>) =>
    apiClient
      .get<AssetPage>('/asset-register', { params: { ...filters, page: 1, pageSize: 100 } })
      .then((r) => r.data.items),
  /** GET /api/asset-register/{id} — AST-03 */
  get: (id: number) => apiClient.get<AssetDetail>(`/asset-register/${id}`).then((r) => r.data),
  /** POST /api/asset-register — AST-04 */
  create: (body: AssetRequest) =>
    apiClient.post<AssetDetail>('/asset-register', body).then((r) => r.data),
  /** PUT /api/asset-register/{id} — AST-04 */
  update: ({ id, ...body }: Id & AssetRequest) =>
    apiClient.put<AssetDetail>(`/asset-register/${id}`, body).then((r) => r.data),
  /** POST /api/asset-register/{id}/retire */
  retire: ({ id }: Id) =>
    apiClient.post<AssetDetail>(`/asset-register/${id}/retire`).then((r) => r.data),
  /** DELETE /api/asset-register/{id} */
  remove: ({ id }: Id) => apiClient.delete(`/asset-register/${id}`).then(() => undefined),
  /** POST /api/asset-register/import — AST-05 */
  import: (body: { csv: string; dryRun: boolean }) =>
    apiClient.post<ImportResult>('/asset-register/import', body).then((r) => r.data),
  /** GET /api/asset-register/{id}/qr-target — AST-07, every role */
  qrTarget: (id: number) =>
    apiClient.get<QrTarget>(`/asset-register/${id}/qr-target`).then((r) => r.data),

  /** GET /api/asset-types */
  types: () => apiClient.get<AssetTypeOption[]>('/asset-types').then((r) => r.data),
  /** GET /api/catalog/manufacturers (Admin) */
  manufacturers: () =>
    apiClient.get<ManufacturerOption[]>('/catalog/manufacturers').then((r) => r.data),
  /** GET /api/buildings (floors + rooms) */
  places: () => apiClient.get<PlaceOption[]>('/buildings').then((r) => r.data),
}

export const assetKeys = {
  all: ['assets'] as const,
  list: (filters: AssetFilters) => [...assetKeys.all, 'list', filters] as const,
  labels: (filters: Omit<AssetFilters, 'page'>) => [...assetKeys.all, 'labels', filters] as const,
  detail: (id: number) => [...assetKeys.all, 'detail', id] as const,
  qr: (id: number) => [...assetKeys.all, 'qr', id] as const,
  types: ['assets', 'ref', 'types'] as const,
  manufacturers: ['assets', 'ref', 'manufacturers'] as const,
  places: ['assets', 'ref', 'places'] as const,
}
