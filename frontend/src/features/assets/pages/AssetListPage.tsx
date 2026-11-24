import { useState } from 'react'
import { Link, useSearchParams } from 'react-router'
import { assetDetailPath, paths } from '@/app/paths'
import {
  Button,
  EmptyBlock,
  ErrorBlock,
  FilterSelect,
  LinkButton,
  LoadingBlock,
  PageHeader,
  SearchInput,
  SortHeader,
  type SortState,
} from '@/shared/ui'
import { formatDate } from '@/shared/utils/format'
import { AssetStatus } from '../components/AssetStatus'
import { useAssetList, useAssetTypes, usePlaces } from '../hooks'
import type { AssetFilters, AssetListStatus } from '../types'

/** AST-02 Asset list. Question: "What is installed where, and what is broken or out of warranty?" */
export function AssetListPage() {
  const [params] = useSearchParams()
  const [filters, setFilters] = useState<AssetFilters>({
    page: 1,
    includeRetired: true,
    roomId: Number(params.get('roomId')) || undefined,
  })
  const assets = useAssetList(filters)
  const types = useAssetTypes()
  const places = usePlaces()
  const categories = [
    ...new Map(types.data?.map((t) => [t.categoryId, t.categoryName]) ?? []).entries(),
  ]
  const set = (patch: Partial<AssetFilters>) => setFilters((f) => ({ ...f, ...patch, page: 1 }))

  // The server sorts: the list comes in pages.
  const [sortKey, sortDir] = (filters.sort ?? '').split('_')
  const sort = sortKey ? ({ key: sortKey, dir: sortDir } as SortState<'name' | 'warranty'>) : null
  const toggleSort = (key: 'name' | 'warranty') =>
    set({ sort: `${key}_${sort?.key === key && sort.dir === 'asc' ? 'desc' : 'asc'}` })

  const data = assets.data
  const lastPage = data ? Math.max(1, Math.ceil(data.totalCount / data.pageSize)) : 1
  const broken = data?.items.filter((a) => a.openCases > 0).length ?? 0
  const roomName = filters.roomId
    ? places.data
        ?.flatMap((b) => b.floors.flatMap((f) => f.rooms.map((r) => ({ ...r, b: b.name }))))
        .find((r) => r.id === filters.roomId)
    : undefined

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title="Assets"
        description={
          data
            ? `${data.totalCount} ${data.totalCount === 1 ? 'asset' : 'assets'}${roomName ? ` in ${roomName.b} · ${roomName.name}` : ''}, ${broken} with an open case on this page`
            : undefined
        }
        // Meeting note 18/11: no "New asset" or "Import" here any more; the staff propose changes in a change list.
        actions={<LinkButton to={paths.qrLabels}>QR labels</LinkButton>}
      />

      {/* One 32px filter row, like Cases (16/11): search, the narrowing filters, then the status. */}
      <div className="flex flex-wrap items-center gap-2 text-sm">
        <SearchInput
          label="Search assets"
          placeholder="Asset name"
          value={filters.search ?? ''}
          onChange={(e) => set({ search: e.target.value || undefined })}
        />
        <FilterSelect
          label="Building"
          value={String(filters.buildingId ?? '')}
          onChange={(e) =>
            set({ buildingId: Number(e.target.value) || undefined, roomId: undefined })
          }
        >
          <option value="">All</option>
          {places.data?.map((b) => (
            <option key={b.id} value={b.id}>
              {b.name}
            </option>
          ))}
        </FilterSelect>
        <FilterSelect
          label="Category"
          value={String(filters.categoryId ?? '')}
          onChange={(e) =>
            set({ categoryId: Number(e.target.value) || undefined, typeId: undefined })
          }
        >
          <option value="">All</option>
          {categories.map(([id, name]) => (
            <option key={id} value={id}>
              {name}
            </option>
          ))}
        </FilterSelect>
        <FilterSelect
          label="Type"
          value={String(filters.typeId ?? '')}
          onChange={(e) => set({ typeId: Number(e.target.value) || undefined })}
        >
          <option value="">All</option>
          {types.data
            ?.filter((t) => !filters.categoryId || t.categoryId === filters.categoryId)
            .map((t) => (
              <option key={t.id} value={t.id}>
                {t.name}
              </option>
            ))}
        </FilterSelect>
        <FilterSelect
          label="Status"
          value={filters.status ?? ''}
          onChange={(e) => set({ status: (e.target.value || undefined) as AssetListStatus })}
        >
          <option value="">All</option>
          <option value="InService">In service</option>
          <option value="OpenCase">Open case</option>
          <option value="Retired">Retired</option>
        </FilterSelect>
        {filters.roomId && (
          <Button variant="ghost" onClick={() => set({ roomId: undefined })}>
            Show every room
          </Button>
        )}
      </div>

      {assets.isPending ? (
        <LoadingBlock rows={8} />
      ) : assets.error ? (
        <ErrorBlock
          what="the assets"
          message={assets.error.message}
          onRetry={() => void assets.refetch()}
        />
      ) : data!.items.length === 0 ? (
        <EmptyBlock>No asset matches these filters.</EmptyBlock>
      ) : (
        <>
          <table className="hidden w-full text-left text-sm md:table">
            <thead>
              <tr className="border-border text-muted border-b text-xs">
                <SortHeader sortKey="name" sort={sort} onSort={toggleSort}>
                  Asset
                </SortHeader>
                <th className="h-9 pr-4 font-normal">Location</th>
                <th className="h-9 pr-4 font-normal">Status</th>
                <SortHeader
                  sortKey="warranty"
                  sort={sort}
                  onSort={toggleSort}
                  align="right"
                  className="pr-0"
                >
                  Warranty until
                </SortHeader>
              </tr>
            </thead>
            <tbody>
              {data!.items.map((a) => (
                <tr key={a.id} className="border-rule border-b align-top last:border-0">
                  <td className="py-3 pr-4">
                    <Link
                      to={assetDetailPath(a.id)}
                      className="text-ink font-medium underline-offset-2 hover:underline"
                    >
                      {a.name}
                    </Link>
                    <div className="text-muted mt-0.5 text-[13px]">
                      {a.categoryName} · {a.typeName}
                      {a.manufacturerName && <> · {a.manufacturerName}</>}
                    </div>
                  </td>
                  <td className="py-3 pr-4">
                    {a.location.roomName}
                    <div className="text-muted mt-0.5 text-[13px]">
                      {a.location.buildingName} · {a.location.floorName}
                    </div>
                  </td>
                  <td className="py-3 pr-4">
                    <AssetStatus a={a} />
                  </td>
                  <td className="text-muted py-3 text-right tabular-nums">
                    {a.warrantyEndDate ? formatDate(a.warrantyEndDate) : '—'}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          <ul className="flex flex-col md:hidden">
            {data!.items.map((a) => (
              <li key={a.id} className="border-rule border-b py-3 last:border-0">
                <div className="flex items-start justify-between gap-3">
                  <Link to={assetDetailPath(a.id)} className="min-w-0 font-medium">
                    {a.name}
                  </Link>
                  <AssetStatus a={a} />
                </div>
                <div className="text-muted mt-1 text-[13px]">
                  {a.categoryName} · {a.typeName} · {a.location.buildingName} ·{' '}
                  {a.location.roomName}
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
