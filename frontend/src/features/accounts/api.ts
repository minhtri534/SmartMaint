import { apiClient } from '@/lib/api-client'
import type {
  AccountDetail,
  AccountListFilters,
  AccountPage,
  AccountWithPassword,
  AdminOverview,
  BuildingOption,
  ChangeRoleRequest,
  CreateAccountRequest,
  SpecialtyRef,
  UpdateAccountRequest,
} from './types'

type Id = { id: number }

/** One function per endpoint of AccountsController (+ the reference data the form needs). Admin only. */
export const accountsApi = {
  /** GET /api/accounts/overview — ADM-01 */
  overview: () => apiClient.get<AdminOverview>('/accounts/overview').then((r) => r.data),
  /** GET /api/accounts — ADM-02 */
  list: ({ page, ...filters }: AccountListFilters) =>
    apiClient
      .get<AccountPage>('/accounts', { params: { ...filters, page, pageSize: 20 } })
      .then((r) => r.data),
  /** GET /api/accounts/{id} — ADM-04 */
  get: (id: number) => apiClient.get<AccountDetail>(`/accounts/${id}`).then((r) => r.data),
  /** POST /api/accounts — ADM-03 */
  create: (body: CreateAccountRequest) =>
    apiClient.post<AccountWithPassword>('/accounts', body).then((r) => r.data),
  /** PUT /api/accounts/{id} — ADM-03 */
  update: ({ id, ...body }: Id & UpdateAccountRequest) =>
    apiClient.put<AccountDetail>(`/accounts/${id}`, body).then((r) => r.data),
  /** POST /api/accounts/{id}/change-role — DLG-16; the account comes back with a NEW id. */
  changeRole: ({ id, ...body }: Id & ChangeRoleRequest) =>
    apiClient.post<AccountDetail>(`/accounts/${id}/change-role`, body).then((r) => r.data),
  /** POST /api/accounts/{id}/deactivate — DLG-16 */
  deactivate: ({ id }: Id) =>
    apiClient.post<AccountDetail>(`/accounts/${id}/deactivate`).then((r) => r.data),
  /** POST /api/accounts/{id}/reactivate — DLG-16 */
  reactivate: ({ id }: Id) =>
    apiClient.post<AccountDetail>(`/accounts/${id}/reactivate`).then((r) => r.data),
  /** POST /api/accounts/{id}/reset-password — DLG-16 */
  resetPassword: ({ id }: Id) =>
    apiClient
      .post<{ temporaryPassword: string }>(`/accounts/${id}/reset-password`)
      .then((r) => r.data),
  /** POST /api/accounts/{id}/unlock — DLG-16 */
  unlock: ({ id }: Id) =>
    apiClient.post<AccountDetail>(`/accounts/${id}/unlock`).then((r) => r.data),

  /** GET /api/buildings (with floors and rooms) */
  buildings: () => apiClient.get<BuildingOption[]>('/buildings').then((r) => r.data),
  /** GET /api/specialties */
  specialties: () => apiClient.get<SpecialtyRef[]>('/specialties').then((r) => r.data),
}

export const accountKeys = {
  all: ['accounts'] as const,
  overview: () => [...accountKeys.all, 'overview'] as const,
  list: (filters: AccountListFilters) => [...accountKeys.all, 'list', filters] as const,
  detail: (id: number) => [...accountKeys.all, 'detail', id] as const,
  buildings: ['accounts', 'ref', 'buildings'] as const,
  specialties: ['accounts', 'ref', 'specialties'] as const,
}
