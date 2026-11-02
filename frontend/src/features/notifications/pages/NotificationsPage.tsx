import { EmptyBlock, ErrorBlock, LoadingBlock, PageHeader } from '@/shared/ui'
import { formatDateTime, formatRelative } from '@/shared/utils/format'
import { useNotifications } from '../hooks'

const DAY = 24 * 60 * 60 * 1000

/**
 * COM-05 Notification center, every role. In-app only, kept 90 days, refreshed every 30 seconds.
 * The ERD has no "read" flag, so there is no unread count: the page shows what happened, newest first.
 */
export function NotificationsPage() {
  const notifications = useNotifications()
  const items = notifications.data?.items ?? []
  // "Now" = when the list was fetched, so the count is stable between renders.
  const fetchedAt = notifications.dataUpdatedAt
  const today = items.filter((n) => fetchedAt - new Date(n.sentAt).getTime() < DAY).length

  return (
    <div className="flex max-w-[760px] flex-col gap-5">
      <PageHeader
        title="Notifications"
        description={notifications.data ? `${today} in the last 24 hours` : undefined}
      />
      {notifications.isPending ? (
        <LoadingBlock rows={5} />
      ) : notifications.error ? (
        <ErrorBlock
          what="your notifications"
          message={notifications.error.message}
          onRetry={() => void notifications.refetch()}
        />
      ) : items.length === 0 ? (
        <EmptyBlock>Nothing yet. Changes to your cases show up here.</EmptyBlock>
      ) : (
        <ul className="flex flex-col">
          {items.map((n) => (
            <li
              key={n.id}
              className="border-rule flex items-start justify-between gap-4 border-b py-3 last:border-0"
            >
              <p className="text-sm">{n.details}</p>
              <time
                dateTime={n.sentAt}
                title={formatDateTime(n.sentAt)}
                className="text-muted flex-none text-xs tabular-nums"
              >
                {formatRelative(n.sentAt)}
              </time>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
