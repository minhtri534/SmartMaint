import { apiClient } from '@/lib/api-client'
import type { AuditFilters, AuditPage } from './types'

export const auditApi = {
  /** GET /api/audit-logs — ADM-08 (Admin only) */
  list: ({ page, ...filters }: AuditFilters) =>
    apiClient
      .get<AuditPage>('/audit-logs', { params: { ...filters, page, pageSize: 25 } })
      .then((r) => r.data),
}

export const auditKeys = {
  all: ['audit'] as const,
  list: (filters: AuditFilters) => [...auditKeys.all, 'list', filters] as const,
}
