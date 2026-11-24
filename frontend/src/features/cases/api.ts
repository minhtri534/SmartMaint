import { apiClient } from '@/lib/api-client'
import type {
  ApproveCaseRequest,
  AssetLookup,
  AssetRef,
  Building,
  CaseDetail,
  CasePage,
  AddCostRequest,
  CreateCaseRequest,
  EditCaseRequest,
  EmergencyLogRequest,
  FollowerOptions,
  ExtendDueRequest,
  LogHoursRequest,
  IncidentReportRequest,
  OpenSurveyRequest,
  RateCaseRequest,
  ReasonRequest,
  RejectCaseRequest,
  RequestDecisionRequest,
  RequestSuggestionResponse,
  ServiceType,
  TechnicianOption,
  TechnicianSuggestion,
  UpdateCostsRequest,
  UpdateTaskRequest,
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
  /** The asset behind a typed code AS-000012 (same endpoint as the QR label, AST-07). */
  assetByCode: (id: number) =>
    apiClient.get<AssetLookup>(`/asset-register/${id}/qr-target`).then((r) => r.data),
  /** Staff only: the active technicians; for a case, the ones who fit it come first. */
  technicians: (caseId?: number) =>
    apiClient.get<TechnicianOption[]>('/technicians', { params: { caseId } }).then((r) => r.data),
  /** Staff only: who should take this case. A suggestion: nothing changes. */
  technicianSuggestion: (caseId: number) =>
    apiClient
      .post<TechnicianSuggestion>('/ai/technician-suggestion', { caseId })
      .then((r) => r.data),
  /** Staff only: who follows the case and the customers found by username or e-mail. */
  followerOptions: (id: number, search: string) =>
    apiClient
      .get<FollowerOptions>(`/cases/${id}/follower-options`, { params: { search } })
      .then((r) => r.data),

  // Cases: the server filters by role (own + followed cases / own buildings / own work orders, B3).
  // Every page: the screens count and filter the whole list themselves.
  list: async () => {
    const pageSize = 100
    const items: CasePage['items'] = []
    for (let page = 1; ; page++) {
      const { data } = await apiClient.get<CasePage>('/cases', { params: { page, pageSize } })
      items.push(...data.items)
      if (data.items.length < pageSize || items.length >= data.totalCount) return items
    }
  },
  get: (id: number) => apiClient.get<CaseDetail>(`/cases/${id}`).then((r) => r.data),
  /** EMP-02 */
  create: (body: CreateCaseRequest) => post<CaseDetail>('/cases', body),
  /** EMP-02 helper (AiController): the AI's suggestion for the description; never changes data. */
  requestSuggestion: (description: string) =>
    apiClient
      .post<RequestSuggestionResponse>('/ai/request-suggestion', { description })
      .then((r) => r.data),

  // Status changes: one endpoint per arrow of the state machine. Each returns the updated case.
  /** DLG-12 */
  cancel: ({ id, ...body }: Id & ReasonRequest) => post<CaseDetail>(`/cases/${id}/cancel`, body),
  /** DLG-01: the work order form (technician, window, tasks; meeting 07/11) */
  approve: ({ id, ...body }: Id & ApproveCaseRequest) =>
    post<CaseDetail>(`/cases/${id}/approve`, body),
  /** DLG-02 (a duplicate may hand its content to the case it repeats) */
  reject: ({ id, ...body }: Id & RejectCaseRequest) =>
    post<CaseDetail>(`/cases/${id}/reject`, body),
  /** PUT /api/cases/{id} — the staff's one Edit (meeting 07/11) */
  edit: ({ id, ...body }: Id & EditCaseRequest) =>
    apiClient.put<CaseDetail>(`/cases/${id}`, body).then((r) => r.data),
  /** PUT /api/cases/{id}/followers — the followers become exactly this list */
  setFollowers: ({ id, customerIds }: Id & { customerIds: number[] }) =>
    apiClient.put<CaseDetail>(`/cases/${id}/followers`, { customerIds }).then((r) => r.data),
  /** POST /api/cases/{id}/reassign — taken work continues in a new case (see nextCase) */
  reassign: ({ id, technicianId }: Id & { technicianId: number }) =>
    post<CaseDetail>(`/cases/${id}/reassign`, { technicianId }),
  accept: ({ id }: Id) => post<CaseDetail>(`/cases/${id}/accept`),
  /** DLG-09 */
  decline: ({ id, ...body }: Id & ReasonRequest) => post<CaseDetail>(`/cases/${id}/decline`, body),
  /** The SLA clock starts here. */
  start: ({ id }: Id) => post<CaseDetail>(`/cases/${id}/start`),
  /** DLG-10 */
  /** Status → On hold (meeting 07/11: no reason, no date); the work clock stops. */
  hold: ({ id }: Id) => post<CaseDetail>(`/cases/${id}/hold`),
  /** The technician continues after a hold. */
  resume: ({ id }: Id) => post<CaseDetail>(`/cases/${id}/resume`),
  /** DLG-11 */
  complete: ({ id }: Id) => post<CaseDetail>(`/cases/${id}/complete`, {}),
  /** DLG-14 */
  rate: ({ id, ...body }: Id & RateCaseRequest) => post<CaseDetail>(`/cases/${id}/rating`, body),

  // Part 4: the deadline, "not fixed", costs
  /** POST /api/cases/{id}/due — O09 */
  extendDue: ({ id, ...body }: Id & ExtendDueRequest) => post<CaseDetail>(`/cases/${id}/due`, body),
  /** POST /api/cases/{id}/not-fixed — customer, within 7 days of closing */
  reportNotFixed: ({ id, ...body }: Id & ReasonRequest) =>
    post<CaseDetail>(`/cases/${id}/not-fixed`, body),
  /** POST /api/cases/{id}/not-fixed/accept — staff: reopen */
  acceptNotFixed: ({ id }: Id) => post<CaseDetail>(`/cases/${id}/not-fixed/accept`),
  /** POST /api/cases/{id}/reopen — staff: a closed case (meeting note 18/11), with the reason */
  reopen: ({ id, ...body }: Id & ReasonRequest) => post<CaseDetail>(`/cases/${id}/reopen`, body),
  /** POST /api/cases/{id}/not-fixed/decline — staff, with a reason for the customer */
  declineNotFixed: ({ id, ...body }: Id & ReasonRequest) =>
    post<CaseDetail>(`/cases/${id}/not-fixed/decline`, body),
  /** PUT /api/cases/{id}/costs — staff records the cost lines (meeting 07/11) */
  /** PUT /api/cases/{id}/hours — technician types the total work hours (meeting 07/11) */
  logHours: ({ id, ...body }: Id & LogHoursRequest) =>
    apiClient.put<CaseDetail>(`/cases/${id}/hours`, body).then((r) => r.data),
  /** POST /api/cases/{id}/costs — technician records a cost they paid, with the receipt */
  addCost: ({ id, ...body }: Id & AddCostRequest) => post<CaseDetail>(`/cases/${id}/costs`, body),
  /** PUT /api/cases/{id}/costs/{costId} — technician corrects a line they added */
  editCost: ({ id, costId, ...body }: Id & { costId: number } & AddCostRequest) =>
    apiClient.put<CaseDetail>(`/cases/${id}/costs/${costId}`, body).then((r) => r.data),
  updateCosts: ({ id, ...body }: Id & UpdateCostsRequest) =>
    apiClient.put<CaseDetail>(`/cases/${id}/costs`, body).then((r) => r.data),

  // Tasks (every case has tasks, meeting 07/11)
  /** POST /api/cases/{id}/tasks/{taskId} — TEC-05, technician */
  updateTask: ({ id, taskId, ...body }: Id & { taskId: number } & UpdateTaskRequest) =>
    post<CaseDetail>(`/cases/${id}/tasks/${taskId}`, body),
  /** POST /api/cases/{id}/tasks/{taskId}/third-party — staff: done by an outside party (or back) */
  setTaskThirdParty: ({ id, taskId, thirdParty }: Id & { taskId: number; thirdParty: boolean }) =>
    post<CaseDetail>(`/cases/${id}/tasks/${taskId}/third-party`, { thirdParty }),

  // Part 6: emergency (F4)
  /** POST /api/cases/{id}/acknowledge — staff, first response */
  acknowledge: ({ id }: Id) => post<CaseDetail>(`/cases/${id}/acknowledge`),
  /** POST /api/cases/{id}/log — staff, emergency log line */
  addLog: ({ id, ...body }: Id & EmergencyLogRequest) => post<CaseDetail>(`/cases/${id}/log`, body),
  /** PUT /api/cases/{id}/incident-report — staff (K08) */
  writeIncidentReport: ({ id, ...body }: Id & IncidentReportRequest) =>
    apiClient.put<CaseDetail>(`/cases/${id}/incident-report`, body).then((r) => r.data),

  // Part 7: repair or replace (F5)
  /** POST /api/cases/{id}/request-decision — technician, steps ①–② */
  requestDecision: ({ id, ...body }: Id & RequestDecisionRequest) =>
    post<CaseDetail>(`/cases/${id}/request-decision`, body),
  /** POST /api/cases/{id}/outsource — staff: handed to an outside party (meeting 07/11: nothing else recorded) */
  outsource: ({ id }: Id) => post<CaseDetail>(`/cases/${id}/outsource`),
  /** POST /api/cases/surveys — staff, F5 way 2 */
  openSurvey: (body: OpenSurveyRequest) => post<CaseDetail>('/cases/surveys', body),
}

/** Query keys of this feature, in one place so invalidation never misses one. */
export const caseKeys = {
  all: ['cases'] as const,
  list: () => [...caseKeys.all, 'list'] as const,
  detail: (id: number) => [...caseKeys.all, 'detail', id] as const,
  followerOptions: (id: number, search: string) =>
    [...caseKeys.all, 'follower-options', id, search] as const,
  serviceTypes: ['service-types'] as const,
  buildings: ['buildings'] as const,
  assets: (roomId: number) => ['assets', roomId] as const,
  technicians: (caseId?: number) => [...caseKeys.all, 'technicians', caseId ?? 0] as const,
}
