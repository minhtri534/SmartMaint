import { useState } from 'react'
import { Link, useSearchParams } from 'react-router'
import { assetDetailPath, paths } from '@/app/paths'
import { useAuth } from '@/features/auth'
import {
  Button,
  EmptyBlock,
  ErrorBlock,
  LinkButton,
  LoadingBlock,
  PageHeader,
  Select,
  TextField,
} from '@/shared/ui'
import { formatDate } from '@/shared/utils/format'
import { AssetStatus } from '../components/AssetStatus'
import { useAssetList, useAssetTypes, usePlaces } from '../hooks'
import type { AssetFilters } from '../types'

/** AST-02 Asset list. Question: "What is installed where, and what is broken or out of warranty?" */
export function AssetListPage() {
  const { user } = useAuth()
  const isAdmin = user?.role === 'admin'
  const [params] = useSearchParams()
  const [filters, setFilters] = useState<AssetFilters>({
    page: 1,
    roomId: Number(params.get('roomId')) || undefined,
  })
  const assets = useAssetList(filters)
  const types = useAssetTypes()
  const places = usePlaces()
  const set = (patch: Partial<AssetFilters>) => setFilters((f) => ({ ...f, ...patch, page: 1 }))

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
        actions={
          <>
            <LinkButton to={paths.qrLabels}>QR labels</LinkButton>
            {isAdmin && <LinkButton to={paths.assetImport}>Import</LinkButton>}
            {isAdmin && (
              <LinkButton variant="primary" to={paths.assetNew}>
                New asset
              </LinkButton>
            )}
          </>
        }
      />

      <div className="grid gap-3 sm:grid-cols-[minmax(0,1fr)_200px_200px]">
        <TextField
          label="Search"
          placeholder="Asset name"
          value={filters.search ?? ''}
          onChange={(e) => set({ search: e.target.value || undefined })}
        />
        <Select
          label="Building"
          placeholder="All buildings"
          value={filters.buildingId ?? ''}
          onChange={(e) =>
            set({ buildingId: Number(e.target.value) || undefined, roomId: undefined })
          }
        >
          {places.data?.map((b) => (
            <option key={b.id} value={b.id}>
              {b.name}
            </option>
          ))}
        </Select>
        <Select
          label="Type"
          placeholder="All types"
          value={filters.typeId ?? ''}
          onChange={(e) => set({ typeId: Number(e.target.value) || undefined })}
        >
          {types.data?.map((t) => (
            <option key={t.id} value={t.id}>
              {t.categoryName} · {t.name}
            </option>
          ))}
        </Select>
      </div>
      <div className="flex flex-wrap items-center gap-4 text-sm">
        <label className="flex items-center gap-2">
          <input
            type="checkbox"
            className="accent-primary size-4"
            checked={filters.includeRetired ?? false}
            onChange={(e) => set({ includeRetired: e.target.checked || undefined })}
          />
          Show retired assets
        </label>
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
                <th className="h-9 pr-4 font-normal">Asset</th>
                <th className="h-9 pr-4 font-normal">Location</th>
                <th className="h-9 pr-4 font-normal">Status</th>
                <th className="h-9 text-right font-normal">Warranty until</th>
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
                      {a.typeName}
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
                  {a.typeName} · {a.location.buildingName} · {a.location.roomName}
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
