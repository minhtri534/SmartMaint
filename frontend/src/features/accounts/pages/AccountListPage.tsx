import { useState } from 'react'
import { Link } from 'react-router'
import { accountDetailPath, paths } from '@/app/paths'
import { ROLE_LABELS, ROLES, type Role } from '@/shared/types/role'
import {
  Button,
  EmptyBlock,
  ErrorBlock,
  FilterSelect,
  Icon,
  LinkButton,
  LoadingBlock,
  PageHeader,
  SearchInput,
} from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { formatRelative } from '@/shared/utils/format'
import { AccountStatusBadge } from '../components/AccountBadges'
import { useAccounts } from '../hooks'
import { STATUS_LABELS, type AccountListFilters, type AccountStatus } from '../types'

/** ADM-02 User list. Question: "Who has an account, in which role, and can they sign in?" */
export function AccountListPage() {
  const [filters, setFilters] = useState<AccountListFilters>({ page: 1 })
  const accounts = useAccounts(filters)
  const set = (patch: Partial<AccountListFilters>) =>
    setFilters((f) => ({ ...f, ...patch, page: 1 }))

  const data = accounts.data
  const lastPage = data ? Math.max(1, Math.ceil(data.totalCount / data.pageSize)) : 1
  const locked = data?.items.filter((a) => a.isLocked).length ?? 0

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title="Accounts"
        description={
          data
            ? `${data.totalCount} ${data.totalCount === 1 ? 'account matches' : 'accounts match'}` +
              (locked > 0 ? `, ${locked} locked on this page` : '')
            : undefined
        }
        actions={
          <LinkButton to={paths.accountNew} variant="primary">
            New account
          </LinkButton>
        }
      />

      {/* One 32px filter row, like Cases (16/11): search, then the narrowing filters. */}
      <div className="flex flex-wrap items-center gap-2">
        <SearchInput
          label="Search accounts"
          placeholder="Name, e-mail or phone"
          value={filters.search ?? ''}
          onChange={(e) => set({ search: e.target.value || undefined })}
        />
        <FilterSelect
          label="Role"
          value={filters.role ?? ''}
          onChange={(e) => set({ role: (e.target.value || undefined) as Role | undefined })}
        >
          <option value="">All</option>
          {ROLES.map((r) => (
            <option key={r} value={r}>
              {ROLE_LABELS[r]}
            </option>
          ))}
        </FilterSelect>
        <FilterSelect
          label="Status"
          value={filters.status ?? ''}
          onChange={(e) =>
            set({ status: (e.target.value || undefined) as AccountStatus | undefined })
          }
        >
          <option value="">Any</option>
          {(['active', 'deactivated'] as const).map((s) => (
            <option key={s} value={s}>
              {STATUS_LABELS[s]}
            </option>
          ))}
        </FilterSelect>
      </div>

      {accounts.isPending ? (
        <LoadingBlock rows={6} />
      ) : accounts.error ? (
        <ErrorBlock
          what="the accounts"
          message={accounts.error.message}
          onRetry={() => void accounts.refetch()}
        />
      ) : data!.items.length === 0 ? (
        <EmptyBlock>No account matches these filters.</EmptyBlock>
      ) : (
        <>
          <table className="hidden w-full text-left text-sm md:table">
            <thead>
              <tr className="border-border text-muted border-b text-xs">
                <th className="h-9 pr-4 font-normal">Name</th>
                <th className="h-9 pr-4 font-normal">Role</th>
                <th className="h-9 pr-4 font-normal">Status</th>
                <th
                  className="h-9 text-right font-normal"
                  aria-sort={
                    filters.lastSignIn === 'asc'
                      ? 'ascending'
                      : filters.lastSignIn === 'desc'
                        ? 'descending'
                        : 'none'
                  }
                >
                  <button
                    type="button"
                    onClick={() =>
                      set({ lastSignIn: filters.lastSignIn === 'desc' ? 'asc' : 'desc' })
                    }
                    className="hover:text-ink inline-flex items-center gap-1 transition-colors duration-150"
                  >
                    Last sign-in
                    <Icon
                      name="back"
                      className={cn(
                        'size-3.5 transition-transform duration-150',
                        filters.lastSignIn === 'asc' ? 'rotate-90' : '-rotate-90',
                        !filters.lastSignIn && 'opacity-40',
                      )}
                    />
                  </button>
                </th>
              </tr>
            </thead>
            <tbody>
              {data!.items.map((a) => (
                <tr key={a.id} className="border-rule border-b align-top last:border-0">
                  <td className="py-3 pr-4">
                    <Link
                      to={accountDetailPath(a.id)}
                      className="text-ink font-medium underline-offset-2 hover:underline"
                    >
                      {a.fullName}
                    </Link>
                    <div className="text-muted mt-0.5 text-[13px]">
                      {/* meeting note 18/11: the e-mail / phone signs in, not the username */}
                      {[a.email, a.phoneNumber].filter(Boolean).join(' · ') || 'No e-mail'}
                    </div>
                  </td>
                  <td className="py-3 pr-4">{ROLE_LABELS[a.role]}</td>
                  <td className="py-3 pr-4">
                    <AccountStatusBadge account={a} />
                  </td>
                  <td className="text-muted py-3 text-right tabular-nums">
                    {a.lastLoginAt ? formatRelative(a.lastLoginAt) : '—'}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          <ul className="flex flex-col md:hidden">
            {data!.items.map((a) => (
              <li key={a.id} className="border-rule border-b py-3 last:border-0">
                <div className="flex items-start justify-between gap-3">
                  <Link to={accountDetailPath(a.id)} className="min-w-0 font-medium">
                    {a.fullName}
                  </Link>
                  <AccountStatusBadge account={a} />
                </div>
                <div className="text-muted mt-1 text-[13px]">
                  {a.email ?? a.phoneNumber ?? 'No e-mail'} · {ROLE_LABELS[a.role]} ·{' '}
                  {a.lastLoginAt ? formatRelative(a.lastLoginAt) : 'never signed in'}
                </div>
              </li>
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
