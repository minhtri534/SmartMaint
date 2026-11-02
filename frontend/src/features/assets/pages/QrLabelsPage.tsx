import { useState } from 'react'
import { useSearchParams } from 'react-router'
import { paths, qrLandingPath } from '@/app/paths'
import {
  Button,
  EmptyBlock,
  ErrorBlock,
  LinkButton,
  LoadingBlock,
  PageHeader,
  Select,
} from '@/shared/ui'
import { QrCode } from '../components/QrCode'
import { qrUrl } from '../qr-url'
import { useAssetsForLabels, usePlaces } from '../hooks'

/**
 * AST-06 QR labels (staff and Administrator). One label per asset in service: QR + name + place. The QR opens
 * /qr/assets/{id}, which sends a tenant to the report form and staff to the asset (AST-07). Print with the browser.
 */
export function QrLabelsPage() {
  const [params] = useSearchParams()
  const onlyAsset = Number(params.get('assetId')) || null
  const [buildingId, setBuildingId] = useState<number | undefined>()
  const places = usePlaces()
  const assets = useAssetsForLabels({ buildingId })
  const labels = (assets.data ?? []).filter((a) => !onlyAsset || a.id === onlyAsset)

  return (
    <div className="flex flex-col gap-5">
      <div className="print:hidden">
        <PageHeader
          above={
            <LinkButton variant="ghost" to={paths.assets} className="-ml-3">
              Assets
            </LinkButton>
          }
          title="QR labels"
          description={
            assets.data
              ? `${labels.length} ${labels.length === 1 ? 'label' : 'labels'} ready to print`
              : undefined
          }
          actions={
            <Button variant="primary" disabled={labels.length === 0} onClick={() => window.print()}>
              Print
            </Button>
          }
        />
        {!onlyAsset && (
          <Select
            label="Building"
            placeholder="All my buildings (first 100 assets)"
            className="mt-4 max-w-80"
            value={buildingId ?? ''}
            onChange={(e) => setBuildingId(Number(e.target.value) || undefined)}
          >
            {places.data?.map((b) => (
              <option key={b.id} value={b.id}>
                {b.name}
              </option>
            ))}
          </Select>
        )}
      </div>

      {assets.isPending ? (
        <LoadingBlock rows={4} />
      ) : assets.error ? (
        <ErrorBlock
          what="the assets"
          message={assets.error.message}
          onRetry={() => void assets.refetch()}
        />
      ) : labels.length === 0 ? (
        <EmptyBlock>No asset in service here.</EmptyBlock>
      ) : (
        <ul className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4 print:grid-cols-3">
          {labels.map((a) => (
            <li
              key={a.id}
              className="border-border flex break-inside-avoid flex-col items-center gap-2 rounded-lg border p-3 text-center"
            >
              <QrCode
                text={qrUrl(qrLandingPath(a.id))}
                size={128}
                label={`QR code for ${a.name}`}
              />
              <div className="text-sm leading-tight font-medium">{a.name}</div>
              <div className="text-muted text-xs leading-tight">
                {a.location.buildingName} · {a.location.floorName} · {a.location.roomName}
              </div>
              <div className="text-muted text-[11px]">Scan to report a problem</div>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
