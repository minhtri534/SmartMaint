import { useState } from 'react'
import {
  Button,
  controlClass,
  EmptyBlock,
  ErrorBlock,
  FilterSelect,
  LoadingBlock,
  PageHeader,
} from '@/shared/ui'
import { formatDateTime } from '@/shared/utils/format'
import { useAuditLog } from '../hooks'
import {
  AUDIT_OPERATIONS,
  AUDIT_TARGETS,
  OPERATION_LABELS,
  TARGET_LABELS,
  type AuditFilters,
  type AuditOperation,
  type AuditRow,
  type AuditTarget,
} from '../types'

/**
 * ADM-08 Audit log: append-only, newest first. As drawn in the ERD it records Administrator actions only
 * (account changes, SLA changes, administrator sign-ins). Each row opens to show the old and new values.
 */
export function AuditLogPage() {
  const [filters, setFilters] = useState<AuditFilters>({ page: 1 })
  const rows = useAuditLog(filters)
  const set = (patch: Partial<AuditFilters>) => setFilters((f) => ({ ...f, ...patch, page: 1 }))
  const data = rows.data
  const lastPage = data ? Math.max(1, Math.ceil(data.totalCount / data.pageSize)) : 1

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title="Audit log"
        description={
          data
            ? `${data.totalCount} recorded ${data.totalCount === 1 ? 'action' : 'actions'} match`
            : undefined
        }
      />

      {/* One 32px filter row (16/11): what, which action, then the day range. */}
      <div className="flex flex-wrap items-center gap-2 text-sm">
        <FilterSelect
          label="Record"
          value={filters.targetType ?? ''}
          onChange={(e) =>
            set({ targetType: (e.target.value || undefined) as AuditTarget | undefined })
          }
        >
          <option value="">Any</option>
          {AUDIT_TARGETS.map((t) => (
            <option key={t} value={t}>
              {TARGET_LABELS[t]}
            </option>
          ))}
        </FilterSelect>
        <FilterSelect
          label="Action"
          value={filters.operation ?? ''}
          onChange={(e) =>
            set({ operation: (e.target.value || undefined) as AuditOperation | undefined })
          }
        >
          <option value="">Any</option>
          {AUDIT_OPERATIONS.map((o) => (
            <option key={o} value={o}>
              {OPERATION_LABELS[o]}
            </option>
          ))}
        </FilterSelect>
        <span className="text-muted ml-1">From</span>
        <input
          type="date"
          aria-label="From"
          value={filters.from ?? ''}
          onChange={(e) => set({ from: e.target.value || undefined })}
          className={controlClass() + ' h-8 max-w-36'}
        />
        <span className="text-muted">to</span>
        <input
          type="date"
          aria-label="To"
          value={filters.to ?? ''}
          onChange={(e) => set({ to: e.target.value || undefined })}
          className={controlClass() + ' h-8 max-w-36'}
        />
        <FilterSelect
          label="Sort"
          neutral
          className="sm:ml-auto"
          value={filters.oldestFirst ? 'oldest' : 'newest'}
          onChange={(e) => set({ oldestFirst: e.target.value === 'oldest' || undefined })}
        >
          <option value="newest">Newest first</option>
          <option value="oldest">Oldest first</option>
        </FilterSelect>
      </div>

      {rows.isPending ? (
        <LoadingBlock rows={8} />
      ) : rows.error ? (
        <ErrorBlock
          what="the audit log"
          message={rows.error.message}
          onRetry={() => void rows.refetch()}
        />
      ) : data!.items.length === 0 ? (
        <EmptyBlock>No recorded action matches these filters.</EmptyBlock>
      ) : (
        <>
          <ul className="flex flex-col">
            {data!.items.map((r) => (
              <AuditItem key={r.id} row={r} />
            ))}
          </ul>
          {lastPage > 1 && (
            <div className="flex items-center justify-end gap-2 text-sm">
              <span className="text-muted tabular-nums">
                Page {filters.page} of {lastPage}
              </span>
              <Button
                variant="ghost"
                disabled={filters.page <= 1}
                onClick={() => setFilters((f) => ({ ...f, page: f.page - 1 }))}
              >
                Previous
              </Button>
              <Button
                variant="ghost"
                disabled={filters.page >= lastPage}
                onClick={() => setFilters((f) => ({ ...f, page: f.page + 1 }))}
              >
                Next
              </Button>
            </div>
          )}
        </>
      )}
    </div>
  )
}

/** The record changed, read from the JSON (the ERD has no TargetID column). */
function subject(row: AuditRow) {
  const value = JSON.parse(row.newValue ?? row.oldValue ?? '{}') as Record<string, unknown>
  if (row.targetType === 'account') return String(value.username ?? value.fullName ?? '')
  return String(value.name ?? value.roomNumber ?? value.floorNumber ?? '')
}

function AuditItem({ row }: { row: AuditRow }) {
  return (
    <li className="border-rule border-b py-3 last:border-0">
      <details>
        <summary className="flex cursor-pointer list-none items-start justify-between gap-3 text-sm">
          <span>
            {/* a value the page does not know yet prints as sent instead of crashing the page */}
            <span className="font-medium">{row.adminName}</span> ·{' '}
            {OPERATION_LABELS[row.operation] ?? row.operation}{' '}
            {(TARGET_LABELS[row.targetType] ?? row.targetType).toLowerCase()}{' '}
            <span className="font-mono text-xs">{subject(row)}</span>
          </span>
          <span className="text-muted flex-none tabular-nums">
            {formatDateTime(row.occurredAt)}
          </span>
        </summary>
        <div className="mt-3 grid gap-3 md:grid-cols-2">
          <JsonBox label="Before" value={row.oldValue} />
          <JsonBox label="After" value={row.newValue} />
        </div>
      </details>
    </li>
  )
}

function JsonBox({ label, value }: { label: string; value: string | null }) {
  return (
    <div>
      <div className="text-muted mb-1 text-xs">{label}</div>
      <pre className="bg-raised rounded-lg p-3 font-mono text-xs whitespace-pre-wrap">
        {value ? JSON.stringify(JSON.parse(value), null, 2) : '—'}
      </pre>
    </div>
  )
}
