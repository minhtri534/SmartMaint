import { Navigate, useParams } from 'react-router'
import { assetDetailPath, paths, reportFromQrPath } from '@/app/paths'
import { useAuth } from '@/features/auth'
import { Card, LinkButton, LoadingBlock, PageHeader } from '@/shared/ui'
import { useQrTarget } from '../hooks'

/**
 * AST-07 QR landing: decides where a scanned label sends the person. Tenant of that place -> the report form with the
 * room and asset filled in; staff / manager / admin of that building -> the asset; anything else -> one sentence why.
 */
export function QrLandingPage() {
  const { id } = useParams()
  const { user } = useAuth()
  const target = useQrTarget(Number(id))

  if (target.isPending) return <LoadingBlock rows={2} />
  if (target.error)
    return (
      <Explain
        title="Unknown label"
        text="This QR label does not match any asset. Tell the building staff."
      />
    )

  const t = target.data
  if (t.isRetired)
    return (
      <Explain
        title={t.name}
        text="This asset was retired, so its label no longer works. Tell the building staff if it is still in place."
      />
    )
  if (t.canReport) return <Navigate to={reportFromQrPath(t.location.roomId, t.id)} replace />
  if (t.canOpenDetail) return <Navigate to={assetDetailPath(t.id)} replace />

  const where = `${t.location.buildingName} · ${t.location.floorName} · ${t.location.roomName}`
  const text =
    user?.role === 'customer'
      ? `This asset is in ${where}, outside your rented room and your building's common areas. Ask the staff of that building.`
      : user?.role === 'technician'
        ? `This asset is in ${where}. New problems are reported by tenants or the staff; your assigned work is in My work orders.`
        : `This asset is in ${where}, a building not assigned to you.`
  return <Explain title={t.name} text={text} />
}

function Explain({ title, text }: { title: string; text: string }) {
  return (
    <div className="flex max-w-[560px] flex-col gap-5">
      <PageHeader title={title} />
      <Card>
        <p className="text-sm">{text}</p>
        <LinkButton className="mt-4" to={paths.home}>
          Go to my home
        </LinkButton>
      </Card>
    </div>
  )
}
