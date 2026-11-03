import { apiClient } from '@/lib/api-client'
import type {
  AnalyticsFilters,
  AttentionItem,
  ExportReport,
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
  /** GET /api/analytics/export/{report} — ANA-04, saved as a .csv file */
  download: async (report: ExportReport, filters: AnalyticsFilters) => {
    const response = await apiClient.get<Blob>(`/analytics/export/${report}`, {
      params: filters,
      responseType: 'blob',
    })
    const name =
      /filename="?([^";]+)"?/.exec(String(response.headers['content-disposition'] ?? ''))?.[1] ??
      `smartmaint-${report}.csv`
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
}
