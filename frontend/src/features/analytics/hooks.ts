import { keepPreviousData, useMutation, useQuery } from '@tanstack/react-query'
import { analyticsApi, analyticsKeys } from './api'
import type { AnalyticsFilters, ChartQuery, ExportQuery } from './types'

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

export function useChart(query: ChartQuery) {
  return useQuery({
    queryKey: analyticsKeys.chart(query),
    queryFn: () => analyticsApi.chart(query),
    placeholderData: keepPreviousData,
  })
}

export function useExport() {
  return useMutation({ mutationFn: (query: ExportQuery) => analyticsApi.download(query) })
}
