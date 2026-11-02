import { apiClient } from '@/lib/api-client'
import type {
  AssetOption,
  Checklist,
  ChecklistRequest,
  PlanDetail,
  PlanRequest,
  PlanSummary,
  ServiceTypeOption,
  TechnicianOption,
} from './types'

type Id = { id: number }

/** One function per endpoint of MaintenanceController (+ reference data for the forms). */
export const maintenanceApi = {
  /** GET /api/maintenance/checklists */
  checklists: () => apiClient.get<Checklist[]>('/maintenance/checklists').then((r) => r.data),
  /** POST / PUT /api/maintenance/checklists[/{id}] — Admin, Staff */
  saveChecklist: ({ id, ...body }: { id: number | null } & ChecklistRequest) =>
    (id === null
      ? apiClient.post<Checklist>('/maintenance/checklists', body)
      : apiClient.put<Checklist>(`/maintenance/checklists/${id}`, body)
    ).then((r) => r.data),
  /** DELETE /api/maintenance/checklists/{id} */
  deleteChecklist: ({ id }: Id) =>
    apiClient.delete(`/maintenance/checklists/${id}`).then(() => undefined),

  /** GET /api/maintenance/plans — PM-01 */
  plans: () => apiClient.get<PlanSummary[]>('/maintenance/plans').then((r) => r.data),
  /** GET /api/maintenance/plans/{id} — PM-03 */
  plan: (id: number) => apiClient.get<PlanDetail>(`/maintenance/plans/${id}`).then((r) => r.data),
  /** POST /api/maintenance/plans — PM-02, Staff */
  createPlan: (body: PlanRequest) =>
    apiClient.post<PlanDetail>('/maintenance/plans', body).then((r) => r.data),
  /** PUT /api/maintenance/plans/{id} — PM-02, Staff (back to approval) */
  updatePlan: ({ id, ...body }: Id & PlanRequest) =>
    apiClient.put<PlanDetail>(`/maintenance/plans/${id}`, body).then((r) => r.data),
  /** POST /api/maintenance/plans/{id}/approve — Manager */
  approve: ({ id }: Id) =>
    apiClient.post<PlanDetail>(`/maintenance/plans/${id}/approve`).then((r) => r.data),
  /** POST /api/maintenance/plans/{id}/reject — Manager */
  reject: ({ id, reason }: Id & { reason: string }) =>
    apiClient.post<PlanDetail>(`/maintenance/plans/${id}/reject`, { reason }).then((r) => r.data),
  /** POST /api/maintenance/plans/{id}/pause | resume | end — Staff */
  pause: ({ id }: Id) =>
    apiClient.post<PlanDetail>(`/maintenance/plans/${id}/pause`).then((r) => r.data),
  resume: ({ id }: Id) =>
    apiClient.post<PlanDetail>(`/maintenance/plans/${id}/resume`).then((r) => r.data),
  end: ({ id }: Id) =>
    apiClient.post<PlanDetail>(`/maintenance/plans/${id}/end`).then((r) => r.data),

  /** GET /api/asset-register (the caller's buildings) */
  assets: () =>
    apiClient
      .get<{ items: AssetOption[] }>('/asset-register', { params: { page: 1, pageSize: 100 } })
      .then((r) => r.data.items),
  /** GET /api/technicians */
  technicians: () => apiClient.get<TechnicianOption[]>('/technicians').then((r) => r.data),
  /** GET /api/service-types */
  serviceTypes: () => apiClient.get<ServiceTypeOption[]>('/service-types').then((r) => r.data),
}

export const maintenanceKeys = {
  all: ['maintenance'] as const,
  checklists: ['maintenance', 'checklists'] as const,
  plans: ['maintenance', 'plans'] as const,
  plan: (id: number) => ['maintenance', 'plan', id] as const,
  assets: ['maintenance', 'ref', 'assets'] as const,
  technicians: ['maintenance', 'ref', 'technicians'] as const,
  serviceTypes: ['maintenance', 'ref', 'service-types'] as const,
}
