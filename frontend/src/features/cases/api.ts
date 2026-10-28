import { apiClient } from '@/lib/api-client'
import type {
  ApproveCaseRequest,
  AssignCaseRequest,
  AssetRef,
  Building,
  CaseDetail,
  CasePage,
  CompleteWorkRequest,
  CreateCaseRequest,
  HoldCaseRequest,
  RateCaseRequest,
  ReasonRequest,
  RejectCaseRequest,
  ServiceType,
  TechnicianOption,
} from './types'

type Id = { id: number }

/** One function per endpoint of CasesController / CatalogController. Pages call these through hooks.ts. */
const post = <T>(path: string, body?: unknown) =>
  apiClient.post<CaseDetail>(path, body).then((r) => r.data as unknown as T)

export const casesApi = {
  // Reference data for the forms (F1 owns the real catalogue)
  serviceTypes: () => apiClient.get<ServiceType[]>('/service-types').then((r) => r.data),
  buildings: () => apiClient.get<Building[]>('/buildings').then((r) => r.data),
  assets: (roomId: number) =>
    apiClient.get<AssetRef[]>('/assets', { params: { roomId } }).then((r) => r.data),
  /** Staff only */
  technicians: () => apiClient.get<TechnicianOption[]>('/technicians').then((r) => r.data),

  // Cases: the server filters by role (own cases / own buildings / own work orders, B3).
  // The first 100 are enough for the demo; the screens count and filter them.
  list: () =>
    apiClient
      .get<CasePage>('/cases', { params: { page: 1, pageSize: 100 } })
      .then((r) => r.data.items),
  get: (id: number) => apiClient.get<CaseDetail>(`/cases/${id}`).then((r) => r.data),
  /** EMP-02 */
  create: (body: CreateCaseRequest) => post<CaseDetail>('/cases', body),

  // Status changes: one endpoint per arrow of the F3 state machine. Each returns the updated case.
  /** DLG-12 */
  cancel: ({ id, ...body }: Id & ReasonRequest) => post<CaseDetail>(`/cases/${id}/cancel`, body),
  /** DLG-01 */
  approve: ({ id, ...body }: Id & ApproveCaseRequest) =>
    post<CaseDetail>(`/cases/${id}/approve`, body),
  /** DLG-02 */
  reject: ({ id, ...body }: Id & RejectCaseRequest) =>
    post<CaseDetail>(`/cases/${id}/reject`, body),
  /** DLG-04 */
  assign: ({ id, ...body }: Id & AssignCaseRequest) =>
    post<CaseDetail>(`/cases/${id}/assign`, body),
  accept: ({ id }: Id) => post<CaseDetail>(`/cases/${id}/accept`),
  /** DLG-09 */
  decline: ({ id, ...body }: Id & ReasonRequest) => post<CaseDetail>(`/cases/${id}/decline`, body),
  /** The SLA clock starts here. */
  start: ({ id }: Id) => post<CaseDetail>(`/cases/${id}/start`),
  /** DLG-10 */
  hold: ({ id, ...body }: Id & HoldCaseRequest) => post<CaseDetail>(`/cases/${id}/hold`, body),
  /** The technician continues, or the staff lets them continue (after "beyond my ability"). */
  resume: ({ id }: Id) => post<CaseDetail>(`/cases/${id}/resume`),
  /** DLG-11 */
  complete: ({ id, ...body }: Id & CompleteWorkRequest) =>
    post<CaseDetail>(`/cases/${id}/complete`, body),
  /** DLG-06 */
  close: ({ id }: Id) => post<CaseDetail>(`/cases/${id}/close`),
  /** DLG-07 */
  reopen: ({ id, ...body }: Id & ReasonRequest) => post<CaseDetail>(`/cases/${id}/reopen`, body),
  /** DLG-14 */
  rate: ({ id, ...body }: Id & RateCaseRequest) => post<CaseDetail>(`/cases/${id}/rating`, body),
}

/** Query keys of this feature, in one place so invalidation never misses one. */
export const caseKeys = {
  all: ['cases'] as const,
  list: () => [...caseKeys.all, 'list'] as const,
  detail: (id: number) => [...caseKeys.all, 'detail', id] as const,
  serviceTypes: ['service-types'] as const,
  buildings: ['buildings'] as const,
  assets: (roomId: number) => ['assets', roomId] as const,
  technicians: ['technicians'] as const,
}
