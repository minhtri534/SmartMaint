import { apiClient } from '@/lib/api-client'
import type { NotificationPage } from './types'

export const notificationsApi = {
  /** GET /api/notifications — the caller's own, last 90 days, newest first */
  list: () =>
    apiClient
      .get<NotificationPage>('/notifications', { params: { page: 1, pageSize: 50 } })
      .then((r) => r.data),
}

export const notificationKeys = {
  list: ['notifications'] as const,
}
