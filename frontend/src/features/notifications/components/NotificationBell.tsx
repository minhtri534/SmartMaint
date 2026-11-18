import { AnimatePresence, motion } from 'motion/react'
import { useEffect, useRef, useState } from 'react'
import { Link } from 'react-router'
import { caseDetailPath } from '@/app/paths'
import {
  Button,
  Dialog,
  DialogActions,
  EmptyBlock,
  ErrorBlock,
  Icon,
  LoadingBlock,
} from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { formatDateTime, formatRelative } from '@/shared/utils/format'
import { EASE_SNAP } from '@/shared/utils/motion'
import { useNotifications } from '../hooks'
import { markNotificationsSeen, useNotificationsSeenAt } from '../seen'
import type { AppNotification } from '../types'

/** How many of the newest notifications the dropdown shows; "View all" opens the rest. */
const LATEST = 6
const DAY = 24 * 60 * 60 * 1000

/** The ERD has no link to the case, but the text carries its code and the code is built from the id. */
function caseIdOf(details: string) {
  const code = /\bCS-(\d{6,})\b/.exec(details)
  return code ? Number(code[1]) : null
}

/**
 * COM-05 Notification center, every role (Leader 18/11): the bell opens the newest ones in a dropdown, "View all"
 * opens the whole list in a popup. A notification about a case opens that case.
 */
export function NotificationBell({ className }: { className: string }) {
  const notifications = useNotifications()
  const seenAt = useNotificationsSeenAt()
  const [open, setOpen] = useState(false)
  const [allOpen, setAllOpen] = useState(false)
  const box = useRef<HTMLDivElement>(null)
  const bell = useRef<HTMLButtonElement>(null)

  const items = notifications.data?.items ?? []
  const unseen = items.filter((n) => n.sentAt > seenAt).length
  const markAll = () => {
    if (items[0]) markNotificationsSeen(items[0].sentAt)
  }

  useEffect(() => {
    if (!open) return
    const onMouseDown = (e: MouseEvent) => {
      if (!box.current?.contains(e.target as Node)) setOpen(false)
    }
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key !== 'Escape') return
      setOpen(false)
      bell.current?.focus()
    }
    document.addEventListener('mousedown', onMouseDown)
    document.addEventListener('keydown', onKeyDown)
    return () => {
      document.removeEventListener('mousedown', onMouseDown)
      document.removeEventListener('keydown', onKeyDown)
    }
  }, [open])

  const list = (limit: number | undefined, onOpenCase: () => void) =>
    notifications.isPending ? (
      <LoadingBlock rows={3} />
    ) : notifications.error ? (
      <ErrorBlock
        what="your notifications"
        message={notifications.error.message}
        onRetry={() => void notifications.refetch()}
      />
    ) : items.length === 0 ? (
      <EmptyBlock>Nothing yet. Changes to your cases show up here.</EmptyBlock>
    ) : (
      <ul className="flex flex-col gap-0.5">
        {items.slice(0, limit).map((n) => (
          <li key={n.id}>
            <Row
              notification={n}
              isNew={n.sentAt > seenAt}
              clamp={limit !== undefined}
              onOpenCase={onOpenCase}
            />
          </li>
        ))}
      </ul>
    )

  const markAllButton = unseen > 0 && (
    <button
      type="button"
      onClick={markAll}
      className="text-accent-ink flex-none text-[13px] font-medium hover:underline"
    >
      Mark all as read
    </button>
  )

  const total = notifications.data?.totalCount ?? 0
  const today = items.filter(
    (n) => notifications.dataUpdatedAt - new Date(n.sentAt).getTime() < DAY,
  ).length

  return (
    <>
      <div ref={box} className="relative">
        <button
          ref={bell}
          type="button"
          aria-haspopup="dialog"
          aria-expanded={open}
          aria-label={unseen > 0 ? `Notifications, ${unseen} new` : 'Notifications'}
          onClick={() => setOpen((o) => !o)}
          className={className}
        >
          <Icon name="bell" className="size-[18px]" />
          {unseen > 0 && (
            <span className="bg-accent ring-surface text-on-accent absolute -top-0.5 -right-0.5 grid h-[17px] min-w-[17px] place-items-center rounded-full px-1 text-[10px] leading-none font-semibold tabular-nums ring-2">
              {unseen > 9 ? '9+' : unseen}
            </span>
          )}
        </button>

        <AnimatePresence>
          {open && (
            <motion.div
              role="dialog"
              aria-label="Notifications"
              initial={{ opacity: 0, scale: 0.96, y: -4 }}
              animate={{ opacity: 1, scale: 1, y: 0 }}
              exit={{ opacity: 0, scale: 0.97, transition: { duration: 0.12 } }}
              transition={{ duration: 0.18, ease: EASE_SNAP }}
              style={{ transformOrigin: 'top right' }}
              className="card-soft absolute top-full right-0 z-40 mt-2 w-[min(380px,calc(100vw-32px))] rounded-2xl"
            >
              <div className="flex items-center justify-between gap-3 px-4 pt-3.5 pb-2">
                <h2 className="text-sm font-semibold">Notifications</h2>
                {markAllButton}
              </div>
              <div className="max-h-[min(60vh,380px)] overflow-y-auto px-1.5 pb-1.5">
                {list(LATEST, () => setOpen(false))}
              </div>
              {items.length > 0 && (
                <>
                  <div className="bg-rule h-px" />
                  <div className="p-1.5">
                    <button
                      type="button"
                      onClick={() => {
                        setOpen(false)
                        setAllOpen(true)
                      }}
                      className="hover:bg-active h-9 w-full rounded-xl text-[13px] font-medium transition-colors duration-150"
                    >
                      View all
                    </button>
                  </div>
                </>
              )}
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      <Dialog
        open={allOpen}
        onClose={() => setAllOpen(false)}
        wide
        title="Notifications"
        description={
          total > items.length
            ? `The latest ${items.length} of ${total}`
            : `${today} in the last 24 hours`
        }
      >
        {/* the browser focuses this list when the popup opens: no ring around the whole list */}
        <div className="max-h-[56vh] overflow-y-auto outline-none">
          {list(undefined, () => setAllOpen(false))}
        </div>
        <DialogActions>
          <span className="mr-auto self-center">{markAllButton}</span>
          <Button variant="ghost" onClick={() => setAllOpen(false)}>
            Close
          </Button>
        </DialogActions>
      </Dialog>
    </>
  )
}

function Row({
  notification: n,
  isNew,
  clamp,
  onOpenCase,
}: {
  notification: AppNotification
  isNew: boolean
  clamp: boolean
  onOpenCase: () => void
}) {
  const caseId = caseIdOf(n.details)
  const body = (
    <>
      <span
        aria-hidden="true"
        className={cn('mt-[7px] size-2 flex-none rounded-full', isNew && 'bg-accent')}
      />
      <span className="min-w-0 flex-1">
        <span
          className={cn(
            'block text-[13px] leading-5',
            isNew ? 'text-ink font-medium' : 'text-muted',
            clamp && 'line-clamp-2',
          )}
        >
          {isNew && <span className="sr-only">New: </span>}
          {n.details}
        </span>
        <time
          dateTime={n.sentAt}
          title={formatDateTime(n.sentAt)}
          className="text-faint mt-0.5 block text-xs tabular-nums"
        >
          {formatRelative(n.sentAt)}
        </time>
      </span>
    </>
  )
  const row = 'flex gap-2.5 rounded-xl px-2.5 py-2'
  return caseId ? (
    <Link
      to={caseDetailPath(caseId)}
      onClick={onOpenCase}
      className={cn(row, 'hover:bg-active transition-colors duration-150')}
    >
      {body}
    </Link>
  ) : (
    <div className={row}>{body}</div>
  )
}
