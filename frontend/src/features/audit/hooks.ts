import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { auditApi, auditKeys } from './api'
import type { AuditFilters } from './types'

export function useAuditLog(filters: AuditFilters) {
  return useQuery({
    queryKey: auditKeys.list(filters),
    queryFn: () => auditApi.list(filters),
    placeholderData: keepPreviousData,
  })
}
