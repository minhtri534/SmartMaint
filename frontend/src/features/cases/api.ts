import { apiClient } from '@/lib/api-client'
import type {
  ApproveCaseRequest,
  ChangeUrgencyRequest,
  ExtendDueRequest,
  ReassignCaseRequest,
  UpdateCostsRequest,
  AssignCaseRequest,
  AssetRef,
  Building,
  CaseDetail,
  CasePage,
  CompleteChecklistRequest,
  CompleteWorkRequest,
  CreateCaseRequest,
  UpdateTaskRequest,
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

  // Part 4: staff steps in, "not fixed", costs
  /** POST /api/cases/{id}/reassign — another technician (back to Assigned) */
  reassign: ({ id, ...body }: Id & ReassignCaseRequest) =>
    post<CaseDetail>(`/cases/${id}/reassign`, body),
  /** POST /api/cases/{id}/urgency — O08 */
  changeUrgency: ({ id, ...body }: Id & ChangeUrgencyRequest) =>
    post<CaseDetail>(`/cases/${id}/urgency`, body),
  /** POST /api/cases/{id}/due — O09 */
  extendDue: ({ id, ...body }: Id & ExtendDueRequest) => post<CaseDetail>(`/cases/${id}/due`, body),
  /** POST /api/cases/{id}/not-fixed — customer, within 7 days of closing */
  reportNotFixed: ({ id, ...body }: Id & ReasonRequest) =>
    post<CaseDetail>(`/cases/${id}/not-fixed`, body),
  /** POST /api/cases/{id}/not-fixed/accept — staff: reopen */
  acceptNotFixed: ({ id }: Id) => post<CaseDetail>(`/cases/${id}/not-fixed/accept`),
  /** POST /api/cases/{id}/not-fixed/decline — staff, with a reason for the customer */
  declineNotFixed: ({ id, ...body }: Id & ReasonRequest) =>
    post<CaseDetail>(`/cases/${id}/not-fixed/decline`, body),
  /** PUT /api/cases/{id}/costs — staff, before closing */
  updateCosts: ({ id, ...body }: Id & UpdateCostsRequest) =>
    apiClient.put<CaseDetail>(`/cases/${id}/costs`, body).then((r) => r.data),

  // Part 5: checklist of a preventive work order (F2)
  /** POST /api/cases/{id}/tasks/{taskId} — TEC-05, technician */
  updateTask: ({ id, taskId, ...body }: Id & { taskId: number } & UpdateTaskRequest) =>
    post<CaseDetail>(`/cases/${id}/tasks/${taskId}`, body),
  /** POST /api/cases/{id}/complete-checklist — technician, every item has a result */
  completeChecklist: ({ id, ...body }: Id & CompleteChecklistRequest) =>
    post<CaseDetail>(`/cases/${id}/complete-checklist`, body),
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
