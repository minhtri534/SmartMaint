const dateTime = new Intl.DateTimeFormat('en-GB', { dateStyle: 'medium', timeStyle: 'short' })
const dateOnly = new Intl.DateTimeFormat('en-GB', { dateStyle: 'medium' })

/** "27 Oct 2026, 14:28" in the viewer's time zone. The API always sends UTC ISO strings. */
export function formatDateTime(iso: string | null | undefined): string {
  return iso ? dateTime.format(new Date(iso)) : '—'
}

/** "27 Oct 2026". */
export function formatDate(iso: string | null | undefined): string {
  return iso ? dateOnly.format(new Date(iso)) : '—'
}

const MINUTE = 60_000
const DAY = 24 * 60 * MINUTE

/** A length of time in words: "45 m", "3 h 10 m", "2 d 4 h". Rounded to the minute first, so never "60 m". */
export function formatDuration(ms: number): string {
  const minutes = Math.round(Math.abs(ms) / MINUTE)
  if (minutes < 1) return '< 1 m'
  if (minutes < 60) return `${minutes} m`
  const hours = Math.floor(minutes / 60)
  if (hours < 24) return minutes % 60 ? `${hours} h ${minutes % 60} m` : `${hours} h`
  const days = Math.floor(hours / 24)
  return hours % 24 ? `${days} d ${hours % 24} h` : `${days} d`
}

/** Recency in words: "Just now", "12 m ago", "3 h ago", "Yesterday", "4 d ago", then the date. */
export function formatRelative(iso: string, now = Date.now()): string {
  const diff = now - new Date(iso).getTime()
  if (diff < MINUTE) return 'Just now'
  if (diff < DAY) return `${formatDuration(diff).split(' ').slice(0, 2).join(' ')} ago`
  if (diff < 2 * DAY) return 'Yesterday'
  if (diff < 7 * DAY) return `${Math.floor(diff / DAY)} d ago`
  return formatDate(iso)
}
