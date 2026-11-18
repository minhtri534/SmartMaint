import { useSyncExternalStore } from 'react'

/*
 * "New since you last marked them read" for the bell in the top bar. The ERD has no read flag on Notifications, so
 * this is per browser: the time of the newest notification the user marked as read, kept in localStorage.
 * Nothing is sent to the server; another browser or a cleared one shows the latest notifications as new again.
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

/** "Mark all as read": everything up to this notification stops counting as new. */
export function markNotificationsSeen(latestSentAt: string) {
  try {
    if (latestSentAt > read()) localStorage.setItem(KEY, latestSentAt)
  } catch {
    // storage unavailable: the count stays as it was
  }
  window.dispatchEvent(new Event(EVENT))
}

/** Notifications sent after this time are new. */
export function useNotificationsSeenAt() {
  return useSyncExternalStore(subscribe, read, () => '')
}
