import { useState, type ReactNode } from 'react'
import { useParams } from 'react-router'
import { accountChangeRolePath, accountEditPath, paths } from '@/app/paths'
import { ROLE_LABELS } from '@/shared/types/role'
import {
  Button,
  Card,
  CardHeader,
  ConfirmDialog,
  ErrorBlock,
  LinkButton,
  LoadingBlock,
  PageHeader,
} from '@/shared/ui'
import { formatDateTime } from '@/shared/utils/format'
import { accountsApi } from '../api'
import { AccountStatusBadge } from '../components/AccountBadges'
import { TemporaryPasswordDialog } from '../components/TemporaryPasswordDialog'
import { useAccount, useAccountMutation } from '../hooks'
import type { AccountAction, AccountDetail } from '../types'

/** ADM-04 User detail + DLG-16 account actions. The buttons are the server's allowedActions. */
export function AccountDetailPage() {
  const { id } = useParams()
  const account = useAccount(Number(id))

  if (account.isPending) return <LoadingBlock rows={5} />
  if (account.error)
    return (
      <ErrorBlock
        what="the account"
        message={account.error.message}
        onRetry={() => void account.refetch()}
      />
    )
  return <AccountView a={account.data} />
}

type Confirm = Extract<AccountAction, 'deactivate' | 'reactivate' | 'reset_password' | 'unlock'>

const CONFIRMS: Record<Confirm, { title: string; text: string; label: string }> = {
  deactivate: {
    title: 'Deactivate account',
    text: 'The user can no longer sign in. History stays. You can reactivate it later.',
    label: 'Deactivate',
  },
  reactivate: {
    title: 'Reactivate account',
    text: 'The user can sign in again.',
    label: 'Reactivate',
  },
  reset_password: {
    title: 'Reset password',
    text: 'A new temporary password replaces the current one; the user must change it at the next sign-in.',
    label: 'Reset password',
  },
  unlock: {
    title: 'Unlock account',
    text: 'Ends the lock after too many wrong passwords, so the user can try again now.',
    label: 'Unlock',
  },
}

function AccountView({ a }: { a: AccountDetail }) {
  const [confirm, setConfirm] = useState<Confirm | null>(null)
  const [password, setPassword] = useState<string | null>(null)
  const deactivate = useAccountMutation(accountsApi.deactivate)
  const reactivate = useAccountMutation(accountsApi.reactivate)
  const unlock = useAccountMutation(accountsApi.unlock)
  const reset = useAccountMutation(accountsApi.resetPassword)
  const mutations = { deactivate, reactivate, unlock, reset_password: reset }
  const can = (action: AccountAction) => a.allowedActions.includes(action)

  const run = () => {
    if (!confirm) return
    if (confirm === 'reset_password') {
      reset.mutate(
        { id: a.id },
        { onSuccess: (r) => (setConfirm(null), setPassword(r.temporaryPassword)) },
      )
    } else {
      mutations[confirm].mutate({ id: a.id }, { onSuccess: () => setConfirm(null) })
    }
  }
  const open = (action: Confirm) => {
    mutations[action].reset()
    setConfirm(action)
  }

  const facts = [
    a.isLocked && a.lockedUntil ? `locked until ${formatDateTime(a.lockedUntil)}` : null,
    a.mustChangePassword ? 'has not changed the temporary password yet' : null,
    a.lastLoginAt ? `last signed in ${formatDateTime(a.lastLoginAt)}` : 'never signed in',
  ].filter(Boolean)

  return (
    <div className="flex max-w-[860px] flex-col gap-5">
      <PageHeader
        above={
          <LinkButton variant="ghost" to={paths.accounts} className="-ml-3">
            Accounts
          </LinkButton>
        }
        title={a.fullName}
        description={`${ROLE_LABELS[a.role]}, ${facts.join(', ')}.`}
        actions={
          <>
            {can('unlock') && (
              <Button variant="secondary" onClick={() => open('unlock')}>
                Unlock
              </Button>
            )}
            {can('reset_password') && (
              <Button variant="secondary" onClick={() => open('reset_password')}>
                Reset password
              </Button>
            )}
            {can('reactivate') && (
              <Button variant="secondary" onClick={() => open('reactivate')}>
                Reactivate
              </Button>
            )}
            {can('deactivate') && (
              <Button variant="danger" onClick={() => open('deactivate')}>
                Deactivate
              </Button>
            )}
            {can('edit') && (
              <LinkButton variant="primary" to={accountEditPath(a.id)}>
                Edit
              </LinkButton>
            )}
          </>
        }
      />

      <div className="grid gap-5 md:grid-cols-2">
        <Card>
          <CardHeader title="Account" aside={<AccountStatusBadge account={a} />} />
          <dl className="flex flex-col gap-3 text-sm">
            <Row label="Username">
              <span className="font-mono text-xs">{a.username}</span>
            </Row>
            <Row label="E-mail">{a.email ?? <Dash />}</Row>
            <Row label="Phone">{a.phoneNumber ?? <Dash />}</Row>
            <Row label="Created">{formatDateTime(a.createdAt)}</Row>
          </dl>
        </Card>

        <Card>
          <CardHeader
            title={ROLE_LABELS[a.role]}
            aside={
              can('change_role') ? (
                <LinkButton variant="ghost" to={accountChangeRolePath(a.id)}>
                  Change role
                </LinkButton>
              ) : undefined
            }
          />
          <RoleFacts a={a} />
          {a.hasHistory && (
            <p className="text-muted mt-3 text-xs">
              This account already has history, so its role is fixed. To move the person to another
              role, deactivate it and create a new account.
            </p>
          )}
        </Card>
      </div>

      {confirm && (
        <ConfirmDialog
          open
          onClose={() => setConfirm(null)}
          title={CONFIRMS[confirm].title}
          description={CONFIRMS[confirm].text}
          confirmLabel={CONFIRMS[confirm].label}
          onConfirm={run}
          loading={mutations[confirm].isPending}
          error={mutations[confirm].error?.message}
        />
      )}
      {password && (
        <TemporaryPasswordDialog
          open
          username={a.username}
          password={password}
          onClose={() => setPassword(null)}
        />
      )}
    </div>
  )
}

function RoleFacts({ a }: { a: AccountDetail }) {
  if (a.role === 'admin')
    return (
      <p className="text-muted text-sm">Manages accounts and settings. Never sees case content.</p>
    )
  if (a.role === 'customer') {
    return (
      <dl className="flex flex-col gap-3 text-sm">
        <Row label="Building">{a.room?.buildingName ?? <Dash />}</Row>
        <Row label="Room">{a.room ? `${a.room.floorName} · ${a.room.roomName}` : <Dash />}</Row>
      </dl>
    )
  }
  if (a.role === 'technician') {
    return (
      <dl className="flex flex-col gap-3 text-sm">
        <Row label="Hourly rate">
          {a.hourlyRate !== null ? (
            <span className="tabular-nums">{a.hourlyRate.toLocaleString('en-US')} VND</span>
          ) : (
            <Dash />
          )}
        </Row>
        <Row label="Specialties">
          {a.specialties.length > 0 ? a.specialties.map((s) => s.name).join(', ') : <Dash />}
        </Row>
      </dl>
    )
  }
  if (a.buildings.length === 0)
    return <p className="text-muted text-sm">No building assigned yet.</p>
  return (
    <ul className="flex flex-col text-sm">
      {a.buildings.map((b) => (
        <li
          key={b.buildingId}
          className="border-rule flex justify-between border-b py-2 first:pt-0 last:border-0"
        >
          {b.buildingName}
          {b.isPrimary && <span className="text-muted">Primary</span>}
        </li>
      ))}
    </ul>
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
