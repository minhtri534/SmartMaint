import { useState } from 'react'
import {
  Button,
  EmptyBlock,
  ErrorBlock,
  LoadingBlock,
  PageHeader,
  Select,
  TextField,
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

      <div className="grid gap-3 sm:grid-cols-4">
        <Select
          label="Record"
          placeholder="Any record"
          value={filters.targetType ?? ''}
          onChange={(e) =>
            set({ targetType: (e.target.value || undefined) as AuditTarget | undefined })
          }
        >
          {AUDIT_TARGETS.map((t) => (
            <option key={t} value={t}>
              {TARGET_LABELS[t]}
            </option>
          ))}
        </Select>
        <Select
          label="Action"
          placeholder="Any action"
          value={filters.operation ?? ''}
          onChange={(e) =>
            set({ operation: (e.target.value || undefined) as AuditOperation | undefined })
          }
        >
          {AUDIT_OPERATIONS.map((o) => (
            <option key={o} value={o}>
              {OPERATION_LABELS[o]}
            </option>
          ))}
        </Select>
        <TextField
          label="From"
          type="date"
          value={filters.from ?? ''}
          onChange={(e) => set({ from: e.target.value || undefined })}
        />
        <TextField
          label="To"
          type="date"
          value={filters.to ?? ''}
          onChange={(e) => set({ to: e.target.value || undefined })}
        />
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
  if (row.targetType === 'priority_level') return String(value.name ?? '')
  return String(value.username ?? value.fullName ?? '')
}

function AuditItem({ row }: { row: AuditRow }) {
  return (
    <li className="border-rule border-b py-3 last:border-0">
      <details>
        <summary className="flex cursor-pointer list-none items-start justify-between gap-3 text-sm">
          <span>
            <span className="font-medium">{row.adminName}</span> · {OPERATION_LABELS[row.operation]}{' '}
            {TARGET_LABELS[row.targetType].toLowerCase()}{' '}
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
