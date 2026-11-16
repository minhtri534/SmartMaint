import { useSyncExternalStore } from 'react'
import { useNotifications } from './hooks'

/*
 * "New since you last looked" for the bell in the top bar (16/11). The ERD has no read flag on Notifications,
 * so this is per browser: the time the user last opened the Notifications page, kept in localStorage.
 * Nothing is sent to the server; a cleared browser simply shows the latest notifications as new again.
 */
const KEY = 'smartmaint.notificationsSeenAt'
const EVENT = 'smartmaint:notifications-seen'

function read(): string {
  try {
    return localStorage.getItem(KEY) ?? ''
  } catch {
    return ''
  }
}

function subscribe(onChange: () => void) {
  window.addEventListener(EVENT, onChange)
  window.addEventListener('storage', onChange)
  return () => {
    window.removeEventListener(EVENT, onChange)
    window.removeEventListener('storage', onChange)
  }
}

/** Called by the Notifications page once the list has loaded. */
export function markNotificationsSeen(latestSentAt: string) {
  try {
    if (latestSentAt > read()) localStorage.setItem(KEY, latestSentAt)
  } catch {
    // storage unavailable: the count stays as it was
  }
  window.dispatchEvent(new Event(EVENT))
}

/** How many of the latest notifications arrived after the user last opened the list. */
export function useUnseenNotifications() {
  const seen = useSyncExternalStore(subscribe, read, () => '')
  const list = useNotifications()
  return (list.data?.items ?? []).filter((n) => n.sentAt > seen).length
}
