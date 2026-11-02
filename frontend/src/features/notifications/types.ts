/**
 * ERD: Notifications(UserID, Details, TimeSent). As drawn there is no "read" flag and no link to the case,
 * so the text carries the case code (e.g. "CS-000012 was approved ...").
 */
export interface AppNotification {
  id: number
  details: string
  sentAt: string
}

export interface NotificationPage {
  items: AppNotification[]
  page: number
  pageSize: number
  totalCount: number
}
