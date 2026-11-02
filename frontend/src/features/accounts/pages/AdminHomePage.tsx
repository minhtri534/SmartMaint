import { Link } from 'react-router'
import { paths } from '@/app/paths'
import { ROLE_LABELS } from '@/shared/types/role'
import {
  Card,
  CardHeader,
  EmptyBlock,
  ErrorBlock,
  LinkButton,
  LoadingBlock,
  PageHeader,
} from '@/shared/ui'
import { formatRelative } from '@/shared/utils/format'
import { useAdminOverview } from '../hooks'

const OPERATION_WORDS: Record<string, string> = {
  sign_in: 'signed in',
  create: 'created',
  update: 'updated',
  change_role: 'changed the role of',
  deactivate: 'deactivated',
  reactivate: 'reactivated',
  reset_password: 'reset the password of',
  unlock: 'unlocked',
}
const TARGET_WORDS: Record<string, string> = {
  account: 'an account',
  priority_level: 'an SLA level',
}

/** ADM-01 Administrator home. Question: "Can everyone sign in, and what changed lately?" No case content (BR-11). */
export function AdminHomePage() {
  const overview = useAdminOverview()

  if (overview.isPending) return <LoadingBlock rows={4} />
  if (overview.error)
    return (
      <ErrorBlock
        what="the overview"
        message={overview.error.message}
        onRetry={() => void overview.refetch()}
      />
    )

  const o = overview.data
  const active = o.accounts.reduce((n, r) => n + r.active, 0)
  const finding = [
    `${active} active ${active === 1 ? 'account' : 'accounts'}`,
    o.lockedNow > 0 ? `${o.lockedNow} locked after wrong passwords` : 'none locked',
    o.mustChangePassword > 0 ? `${o.mustChangePassword} still on a temporary password` : null,
  ]
    .filter(Boolean)
    .join(', ')

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title="Administration"
        description={finding}
        actions={
          <LinkButton variant="primary" to={paths.accountNew}>
            New account
          </LinkButton>
        }
      />

      <div className="grid gap-5 lg:grid-cols-[minmax(0,1fr)_minmax(0,1fr)]">
        <Card>
          <CardHeader
            title="Accounts by role"
            aside={
              <Link to={paths.accounts} className="text-sm underline-offset-2 hover:underline">
                All accounts
              </Link>
            }
          />
          <table className="w-full text-sm">
            <thead>
              <tr className="border-border text-muted border-b text-xs">
                <th className="h-8 text-left font-normal">role</th>
                <th className="h-8 text-right font-normal">active</th>
                <th className="h-8 text-right font-normal">deactivated</th>
              </tr>
            </thead>
            <tbody>
              {o.accounts.map((r) => (
                <tr key={r.role} className="border-rule border-b last:border-0">
                  <td className="py-2">{ROLE_LABELS[r.role]}</td>
                  <td className="py-2 text-right tabular-nums">{r.active}</td>
                  <td className="text-muted py-2 text-right tabular-nums">
                    {r.deactivated || '—'}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </Card>

        <Card>
          <CardHeader
            title="Recent activity"
            aside={
              <Link to={paths.auditLog} className="text-sm underline-offset-2 hover:underline">
                Audit log
              </Link>
            }
          />
          {o.recentActivity.length === 0 ? (
            <EmptyBlock>Nothing recorded yet.</EmptyBlock>
          ) : (
            <ul className="flex flex-col text-sm">
              {o.recentActivity.map((a) => (
                <li
                  key={a.id}
                  className="border-rule flex justify-between gap-3 border-b py-2 first:pt-0 last:border-0"
                >
                  <span>
                    {a.adminName} {OPERATION_WORDS[a.operation] ?? a.operation}
                    {a.operation !== 'sign_in' && (
                      <> {TARGET_WORDS[a.targetType] ?? a.targetType}</>
                    )}
                  </span>
                  <span className="text-muted flex-none tabular-nums">
                    {formatRelative(a.occurredAt)}
                  </span>
                </li>
              ))}
            </ul>
          )}
        </Card>
      </div>
    </div>
  )
}
