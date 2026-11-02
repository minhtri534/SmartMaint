import { apiClient } from '@/lib/api-client'
import type { PriorityLevel } from './types'

export const settingsApi = {
  /** GET /api/priority-levels */
  priorityLevels: () => apiClient.get<PriorityLevel[]>('/priority-levels').then((r) => r.data),
  /** PUT /api/priority-levels/{id}/sla — ADM-07 (Admin only) */
  updateSla: ({ id, slaHours }: { id: number; slaHours: number }) =>
    apiClient.put<PriorityLevel>(`/priority-levels/${id}/sla`, { slaHours }).then((r) => r.data),
}

export const settingsKeys = {
  priorityLevels: ['settings', 'priority-levels'] as const,
}
