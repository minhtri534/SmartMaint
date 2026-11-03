import { keepPreviousData, useMutation, useQuery } from '@tanstack/react-query'
import { analyticsApi, analyticsKeys } from './api'
import type { AnalyticsFilters, ExportReport } from './types'

export function useOverview(filters: AnalyticsFilters) {
  return useQuery({
    queryKey: analyticsKeys.overview(filters),
    queryFn: () => analyticsApi.overview(filters),
    placeholderData: keepPreviousData,
  })
}

export function useTechnicianStats(filters: AnalyticsFilters, enabled: boolean) {
  return useQuery({
    queryKey: analyticsKeys.technicians(filters),
    queryFn: () => analyticsApi.technicians(filters),
    placeholderData: keepPreviousData,
    enabled,
  })
}

export function useFailures(filters: AnalyticsFilters) {
  return useQuery({
    queryKey: analyticsKeys.failures(filters),
    queryFn: () => analyticsApi.failures(filters),
    placeholderData: keepPreviousData,
  })
}

/** "Needs attention" changes with every case: refreshed every minute while a home page is open. */
export function useAttention() {
  return useQuery({
    queryKey: analyticsKeys.attention,
    queryFn: analyticsApi.attention,
    refetchInterval: 60_000,
  })
}

export function useExport() {
  return useMutation({
    mutationFn: ({ report, filters }: { report: ExportReport; filters: AnalyticsFilters }) =>
      analyticsApi.download(report, filters),
  })
}
