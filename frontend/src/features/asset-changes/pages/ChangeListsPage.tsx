import { useState } from 'react'
import { Link, useSearchParams } from 'react-router'
import { assetChangeDetailPath, paths } from '@/app/paths'
import { useAuth } from '@/features/auth'
import {
  EmptyBlock,
  ErrorBlock,
  LinkButton,
  LoadingBlock,
  PageHeader,
  SearchInput,
  SortHeader,
  sortRows,
  Tabs,
  useTableSort,
} from '@/shared/ui'
import { formatDate } from '@/shared/utils/format'
import { ChangeListStatusBadge } from '../components/ChangeListStatusBadge'
import { useChangeLists } from '../hooks'
import type { ChangeListStatus } from '../types'

const TABS = [
  { value: 'draft', label: 'Drafts', roles: ['staff'] },
  { value: 'pending', label: 'Waiting for approval', roles: ['staff', 'manager'] },
  { value: 'rejected', label: 'Rejected', roles: ['staff', 'manager'] },
  { value: 'approved', label: 'Approved', roles: ['staff', 'manager'] },
] as const satisfies readonly { value: ChangeListStatus; label: string; roles: readonly string[] }[]

/**
 * Asset changes (meeting note 18/11): the lists of changes to floors, rooms and assets. The staff write them (their
 * drafts are theirs alone) and send them; a manager of the building approves or rejects. Opens on what needs a look:
 * the staff on their drafts, the manager on the lists waiting.
 */
export function ChangeListsPage() {
  const { user } = useAuth()
  const lists = useChangeLists()
  const [params, setParams] = useSearchParams()
  const isStaff = user?.role === 'staff'
  const tabs = TABS.filter((t) => (t.roles as readonly string[]).includes(user?.role ?? ''))
  const tab = tabs.find((t) => t.value === params.get('tab')) ?? tabs[0]!
  const [search, setSearch] = useState('')
  const { sort, toggle } = useTableSort<'title' | 'building' | 'updated'>({
    key: 'updated',
    dir: 'desc',
  })
  const words = search.trim().toLowerCase()
  const rows = sortRows(
    (lists.data ?? []).filter(
      (l) =>
        l.status === tab.value &&
        (!words ||
          l.title.toLowerCase().includes(words) ||
          l.code.toLowerCase().includes(words) ||
          l.staff.fullName.toLowerCase().includes(words)),
    ),
    sort,
    {
      title: (l) => l.title,
      building: (l) => l.buildingNames.join(', '),
      updated: (l) => l.updatedAt,
    },
  )
  const waiting = lists.data?.filter((l) => l.status === 'pending').length ?? 0

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title="Asset changes"
        description={
          lists.data &&
          (isStaff
            ? `Floors, rooms and assets change through these lists. ${waiting} waiting for a manager.`
            : `${waiting} ${waiting === 1 ? 'list waits' : 'lists wait'} for your approval.`)
        }
        actions={
          isStaff && (
            <LinkButton variant="primary" to={paths.assetChangeDraft}>
              New change list
            </LinkButton>
          )
        }
      />
      <Tabs
        items={tabs.map((t) => ({
          value: t.value,
          label: t.label,
          count: lists.data?.filter((l) => l.status === t.value).length,
        }))}
        value={tab.value}
        onChange={(v) => setParams({ tab: v }, { replace: true })}
      />
      <SearchInput
        label="Search change lists"
        placeholder="Title, code or staff member"
        value={search}
        onChange={(e) => setSearch(e.target.value)}
      />
      {lists.isPending ? (
        <LoadingBlock rows={4} />
      ) : lists.error ? (
        <ErrorBlock
          what="the change lists"
          message={lists.error.message}
          onRetry={() => void lists.refetch()}
        />
      ) : rows.length === 0 ? (
        <EmptyBlock>No change list here.</EmptyBlock>
      ) : (
        <table className="w-full text-left text-sm">
          <thead>
            <tr className="border-border text-muted border-b text-xs">
              <SortHeader sortKey="title" sort={sort} onSort={toggle}>
                List
              </SortHeader>
              <SortHeader
                sortKey="building"
                sort={sort}
                onSort={toggle}
                className="hidden md:table-cell"
              >
                Building
              </SortHeader>
              <th className="h-9 pr-4 font-normal">Status</th>
              <SortHeader
                sortKey="updated"
                sort={sort}
                onSort={toggle}
                align="right"
                className="pr-0"
              >
                Updated
              </SortHeader>
            </tr>
          </thead>
          <tbody>
            {rows.map((l) => (
              <tr key={l.id} className="border-rule border-b align-top last:border-0">
                <td className="max-w-[420px] py-3 pr-4">
                  <Link
                    to={assetChangeDetailPath(l.id)}
                    className="text-ink block truncate font-medium underline-offset-2 hover:underline"
                  >
                    {l.title}
                  </Link>
                  <div className="text-muted mt-0.5 text-[13px]">
                    <span className="font-mono">{l.code}</span> · {l.itemCount}{' '}
                    {l.itemCount === 1 ? 'change' : 'changes'} · by {l.staff.fullName}
                  </div>
                </td>
                <td className="hidden py-3 pr-4 md:table-cell">{l.buildingNames.join(', ')}</td>
                <td className="py-3 pr-4">
                  <ChangeListStatusBadge status={l.status} />
                </td>
                <td className="py-3 text-right whitespace-nowrap tabular-nums">
                  {formatDate(l.updatedAt)}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}
