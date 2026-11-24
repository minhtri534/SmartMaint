import { apiClient } from '@/lib/api-client'
import type {
  AnalyticsFilters,
  AttentionItem,
  Chart,
  ChartQuery,
  ExportQuery,
  Failures,
  Overview,
  TechnicianRow,
} from './types'

/** One function per endpoint of AnalyticsController (F6). Staff and managers; technicians: managers only. */
export const analyticsApi = {
  /** GET /api/analytics/overview — ANA-01, MGR-01 */
  overview: (filters: AnalyticsFilters) =>
    apiClient.get<Overview>('/analytics/overview', { params: filters }).then((r) => r.data),
  /** GET /api/analytics/technicians — ANA-02 */
  technicians: (filters: AnalyticsFilters) =>
    apiClient
      .get<TechnicianRow[]>('/analytics/technicians', { params: filters })
      .then((r) => r.data),
  /** GET /api/analytics/failures — ANA-03 */
  failures: (filters: AnalyticsFilters) =>
    apiClient.get<Failures>('/analytics/failures', { params: filters }).then((r) => r.data),
  /** GET /api/analytics/attention — F6 step ④ */
  attention: () => apiClient.get<AttentionItem[]>('/analytics/attention').then((r) => r.data),
  /** GET /api/analytics/chart — the Charts tab, managers */
  chart: (query: ChartQuery) =>
    apiClient.get<Chart>('/analytics/chart', { params: query }).then((r) => r.data),
  /** GET /api/analytics/export — the tab on screen as an .xlsx file */
  download: async (query: ExportQuery) => {
    const response = await apiClient.get<Blob>('/analytics/export', {
      params: query,
      responseType: 'blob',
    })
    const name =
      /filename="?([^";]+)"?/.exec(String(response.headers['content-disposition'] ?? ''))?.[1] ??
      'smartmaint-analytics.xlsx'
    const url = URL.createObjectURL(response.data)
    const link = document.createElement('a')
    link.href = url
    link.download = name
    link.click()
    URL.revokeObjectURL(url)
  },
}

export const analyticsKeys = {
  all: ['analytics'] as const,
  overview: (f: AnalyticsFilters) => ['analytics', 'overview', f] as const,
  technicians: (f: AnalyticsFilters) => ['analytics', 'technicians', f] as const,
  failures: (f: AnalyticsFilters) => ['analytics', 'failures', f] as const,
  attention: ['analytics', 'attention'] as const,
  chart: (q: ChartQuery) => ['analytics', 'chart', q] as const,
}
