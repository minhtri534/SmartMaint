import { useState, type ComponentProps, type ReactNode } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { assetDetailPath, assetEditPath, caseDetailPath, paths, qrLandingPath } from '@/app/paths'
import { StatusBadge } from '@/features/cases'
import {
  Badge,
  Button,
  Card,
  CardHeader,
  ConfirmDialog,
  EmptyBlock,
  ErrorBlock,
  LinkButton,
  LoadingBlock,
  PageHeader,
} from '@/shared/ui'
import { formatDate, formatDateTime } from '@/shared/utils/format'
import { assetsApi } from '../api'
import { QrCode } from '../components/QrCode'
import { qrUrl } from '../qr-url'
import { useAsset, useAssetMutation } from '../hooks'
import type { AssetDetail } from '../types'
import { AssetStatus } from '../components/AssetStatus'
import { SurveyDialog } from '../components/SurveyDialog'

/** AST-03 Asset detail: what it is, where, its warranty and its repair history (G5). */
export function AssetDetailPage() {
  const { id } = useParams()
  const asset = useAsset(Number(id))
  if (asset.isPending) return <LoadingBlock rows={5} />
  if (asset.error)
    return (
      <ErrorBlock
        what="the asset"
        message={asset.error.message}
        onRetry={() => void asset.refetch()}
      />
    )
  return <AssetView a={asset.data} />
}

function AssetView({ a }: { a: AssetDetail }) {
  const navigate = useNavigate()
  const [confirm, setConfirm] = useState<'retire' | 'delete' | 'survey' | null>(null)
  const retire = useAssetMutation(assetsApi.retire)
  const remove = useAssetMutation(assetsApi.remove)
  const can = (x: AssetDetail['allowedActions'][number]) => a.allowedActions.includes(x)

  const repairs = a.caseCount
  const finding = [
    a.retiredAt ? `retired on ${formatDate(a.retiredAt)}` : null,
    `${repairs} ${repairs === 1 ? 'case' : 'cases'} so far`,
    a.openCases > 0 ? `${a.openCases} still open` : null,
    a.frequentFailure ? `${a.repairsLast12Months} repairs in 12 months` : null,
    a.warrantyEndDate
      ? a.underWarranty
        ? `under warranty until ${formatDate(a.warrantyEndDate)}`
        : 'warranty ended'
      : null,
  ].filter(Boolean)

  return (
    <div className="flex max-w-[960px] flex-col gap-5">
      <PageHeader
        above={
          <LinkButton variant="ghost" to={paths.assets} className="-ml-3">
            Assets
          </LinkButton>
        }
        title={a.name}
        description={`${a.typeName}: ${finding.join(', ')}.`}
        actions={
          <>
            {can('open_survey') && (
              <Button variant="secondary" onClick={() => setConfirm('survey')}>
                Open survey
              </Button>
            )}
            {can('delete') && (
              <Button variant="danger" onClick={() => setConfirm('delete')}>
                Delete
              </Button>
            )}
            {can('retire') && (
              <Button variant="secondary" onClick={() => setConfirm('retire')}>
                Retire
              </Button>
            )}
            {can('edit') && (
              <LinkButton variant="primary" to={assetEditPath(a.id)}>
                Edit
              </LinkButton>
            )}
          </>
        }
      />

      <div className="grid gap-5 md:grid-cols-[minmax(0,1fr)_220px]">
        <Card>
          <CardHeader title="Asset" aside={<AssetStatus a={a} />} />
          <dl className="flex flex-col gap-3 text-sm">
            <Row label="Type">
              {a.categoryName} · {a.typeName}
            </Row>
            <Row label="Manufacturer">{a.manufacturerName ?? <Dash />}</Row>
            <Row label="Serviced by">
              {a.hasCopyright ? 'The manufacturer only' : 'Our technicians'}
            </Row>
            <Row label="Warranty until">
              {a.warrantyEndDate ? formatDate(a.warrantyEndDate) : <Dash />}
            </Row>
            <Row label="Building">{a.location.buildingName}</Row>
            <Row label="Room">
              {a.location.floorName} · {a.location.roomName}
            </Row>
            <Row label="Added">{formatDateTime(a.createdAt)}</Row>
            <Row label="Normal life">
              {a.expectedLifeYears ? `${a.expectedLifeYears} years` : <Dash />}
            </Row>
            {a.replaces && (
              <Row label="Replaces">
                <Link to={assetDetailPath(a.replaces.id)} className="underline underline-offset-2">
                  {a.replaces.name}
                </Link>
              </Row>
            )}
            {a.replacedBy && (
              <Row label="Replaced by">
                <Link
                  to={assetDetailPath(a.replacedBy.id)}
                  className="underline underline-offset-2"
                >
                  {a.replacedBy.name}
                </Link>
              </Row>
            )}
          </dl>
        </Card>
        <Card>
          <CardHeader title="QR label" />
          {a.retiredAt ? (
            <p className="text-muted text-sm">A retired asset has no working label.</p>
          ) : (
            <div className="flex flex-col items-center gap-2">
              <QrCode text={qrUrl(qrLandingPath(a.id))} label={`QR code for ${a.name}`} />
              <Link
                to={`${paths.qrLabels}?assetId=${a.id}`}
                className="text-sm underline-offset-2 hover:underline"
              >
                Print label
              </Link>
            </div>
          )}
        </Card>
      </div>

      <Card>
        <CardHeader
          title="Repair history"
          aside={
            a.frequentFailure ? (
              <Badge tone="warning">Fails often</Badge>
            ) : (
              `${a.caseCount} in total`
            )
          }
        />
        {a.history === null ? (
          <p className="text-muted text-sm">Case content is not shown to the Administrator.</p>
        ) : a.history.length === 0 ? (
          <EmptyBlock>No case for this asset yet.</EmptyBlock>
        ) : (
          <ul className="flex flex-col">
            {a.history.map((c) => (
              <li
                key={c.id}
                className="border-rule flex items-start justify-between gap-3 border-b py-2.5 first:pt-0 last:border-0"
              >
                <div className="min-w-0 text-sm">
                  <Link
                    to={caseDetailPath(c.id)}
                    className="font-medium underline-offset-2 hover:underline"
                  >
                    {c.title}
                  </Link>
                  <div className="text-muted text-[13px]">
                    <span className="font-mono text-xs">{c.code}</span> ·{' '}
                    {formatDate(c.submittedAt)}
                    {c.rootCause && <> · {c.rootCause}</>}
                  </div>
                </div>
                <StatusBadge status={c.status as ComponentProps<typeof StatusBadge>['status']} />
              </li>
            ))}
          </ul>
        )}
      </Card>

      {confirm === 'survey' && <SurveyDialog asset={a} onClose={() => setConfirm(null)} />}
      {confirm === 'retire' && (
        <ConfirmDialog
          open
          onClose={() => setConfirm(null)}
          title="Retire asset"
          description="The asset leaves the report form and its QR label stops working. Its history stays."
          confirmLabel="Retire"
          onConfirm={() => retire.mutate({ id: a.id }, { onSuccess: () => setConfirm(null) })}
          loading={retire.isPending}
          error={retire.error?.message}
        />
      )}
      {confirm === 'delete' && (
        <ConfirmDialog
          open
          onClose={() => setConfirm(null)}
          title="Delete asset"
          description="Only an asset that never had a case can be deleted, e.g. one added by mistake."
          confirmLabel="Delete"
          onConfirm={() =>
            remove.mutate(
              { id: a.id },
              { onSuccess: () => navigate(paths.assets, { replace: true }) },
            )
          }
          loading={remove.isPending}
          error={remove.error?.message}
        />
      )}
    </div>
  )
}

function Row({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="flex justify-between gap-4">
      <dt className="text-muted">{label}</dt>
      <dd className="text-right">{children}</dd>
    </div>
  )
}

const Dash = () => <span className="text-muted">—</span>
