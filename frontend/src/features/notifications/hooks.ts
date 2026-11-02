import { useQuery } from '@tanstack/react-query'
import { notificationKeys, notificationsApi } from './api'

/** Refreshes every 30 seconds, so a new event shows up without reloading (SRS 3.4.2). */
export function useNotifications() {
  return useQuery({
    queryKey: notificationKeys.list,
    queryFn: notificationsApi.list,
    refetchInterval: 30_000,
  })
}
