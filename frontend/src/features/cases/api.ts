import type {
  ApproveCaseRequest,
  AssignCaseRequest,
  CompleteWorkRequest,
  CreateCaseRequest,
  RateCaseRequest,
  ReasonRequest,
  RejectCaseRequest,
} from './types'
import { mock } from './mock'

/**
 * One function per endpoint of the (future) CasesController. Pages call these through hooks.ts.
 * TODAY every function calls the mock backend (mock.ts). When the API is ready, replace each body
 * with the apiClient call written in its comment — pages and hooks do not change.
 */
export const casesApi = {
  // Reference data for the forms (F1 endpoints, owned by Huy)
  /** GET /api/service-types */
  serviceTypes: () => mock.serviceTypes(),
  /** GET /api/buildings?include=floors,rooms */
  buildings: () => mock.buildings(),
  /** GET /api/assets?roomId= */
  assets: (roomId: number) => mock.assets(roomId),
  /** POST /api/ai/service-type-suggestions — Replicate behind the AI gateway; only a suggestion. */
  suggestServiceType: (description: string) => mock.suggestServiceType(description),
  /** GET /api/technicians (officer only) */
  technicians: () => mock.technicians(),

  // Cases — the server filters by role: own cases / own buildings / own work orders (B3)
  /** GET /api/cases */
  list: () => mock.list(),
  /** GET /api/cases/{id} */
  get: (id: number) => mock.get(id),
  /** POST /api/cases  (EMP-02) */
  create: (body: CreateCaseRequest) => mock.create(body),

  // Status changes — one endpoint per arrow of the state machine
  /** POST /api/cases/{id}/cancel  (DLG-12) */
  cancel: ({ id, ...body }: { id: number } & ReasonRequest) => mock.cancel(id, body),
  /** POST /api/cases/{id}/approve  (DLG-01) */
  approve: ({ id, ...body }: { id: number } & ApproveCaseRequest) => mock.approve(id, body),
  /** POST /api/cases/{id}/reject  (DLG-02) */
  reject: ({ id, ...body }: { id: number } & RejectCaseRequest) => mock.reject(id, body),
  /** POST /api/cases/{id}/assignments  (DLG-04) */
  assign: ({ id, ...body }: { id: number } & AssignCaseRequest) => mock.assign(id, body),
  /** POST /api/cases/{id}/accept */
  accept: ({ id }: { id: number }) => mock.accept(id),
  /** POST /api/cases/{id}/decline  (DLG-09) */
  decline: ({ id, ...body }: { id: number } & ReasonRequest) => mock.decline(id, body),
  /** POST /api/cases/{id}/start */
  start: ({ id }: { id: number }) => mock.start(id),
  /** POST /api/cases/{id}/complete  (DLG-11, with the repair record) */
  complete: ({ id, ...body }: { id: number } & CompleteWorkRequest) => mock.complete(id, body),
  /** POST /api/cases/{id}/close  (DLG-06) */
  close: ({ id }: { id: number }) => mock.close(id),
  /** POST /api/cases/{id}/reopen  (DLG-07) */
  reopen: ({ id, ...body }: { id: number } & ReasonRequest) => mock.reopen(id, body),
  /** POST /api/cases/{id}/rating  (DLG-14) */
  rate: ({ id, ...body }: { id: number } & RateCaseRequest) => mock.rate(id, body),
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
