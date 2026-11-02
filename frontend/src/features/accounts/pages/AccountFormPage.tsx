import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm, useWatch, type UseFormRegister } from 'react-hook-form'
import { useNavigate, useParams } from 'react-router'
import { accountDetailPath, paths } from '@/app/paths'
import { ROLE_LABELS, ROLES } from '@/shared/types/role'
import {
  Alert,
  Button,
  Card,
  CardHeader,
  ErrorBlock,
  LinkButton,
  LoadingBlock,
  PageHeader,
  Select,
  TextField,
} from '@/shared/ui'
import { applyFieldErrors } from '@/shared/utils/apply-field-errors'
import { accountsApi } from '../api'
import { TemporaryPasswordDialog } from '../components/TemporaryPasswordDialog'
import { useAccount, useAccountMutation, useBuildingOptions, useSpecialties } from '../hooks'
import {
  accountSchema,
  blank,
  emptyAccountForm,
  toRoleDetails,
  type AccountFormValues,
} from '../schemas'
import type { AccountDetail, BuildingOption, SpecialtyRef } from '../types'

type Mode = 'create' | 'edit' | 'change-role'

/**
 * ADM-03 Create / Edit user, and DLG-16 "Change role" (same form, only the role part editable).
 * The role decides the extra fields: customer -> rented room, technician -> rate + specialties,
 * staff -> buildings (+ primary), manager -> buildings.
 */
export function AccountFormPage({ mode }: { mode: Mode }) {
  const { id } = useParams()
  const needsAccount = mode !== 'create'
  const account = useAccount(Number(id), needsAccount)
  const buildings = useBuildingOptions()
  const specialties = useSpecialties()

  if ((needsAccount && account.isPending) || buildings.isPending || specialties.isPending)
    return <LoadingBlock rows={6} />
  const error = (needsAccount ? account.error : null) ?? buildings.error ?? specialties.error
  if (error) {
    return (
      <ErrorBlock
        what="the form"
        message={error.message}
        onRetry={() =>
          void Promise.all([account.refetch(), buildings.refetch(), specialties.refetch()])
        }
      />
    )
  }

  return (
    <AccountForm
      mode={mode}
      account={needsAccount ? account.data! : null}
      buildings={buildings.data!}
      specialties={specialties.data!}
    />
  )
}

function toFormValues(
  mode: Mode,
  a: AccountDetail | null,
  buildings: BuildingOption[],
): AccountFormValues {
  if (!a) return emptyAccountForm
  const roomBuilding = a.room
    ? buildings.find((b) => b.floors.some((f) => f.rooms.some((r) => r.id === a.room!.id)))
    : undefined
  return {
    username: a.username,
    fullName: a.fullName,
    email: a.email ?? '',
    phoneNumber: a.phoneNumber ?? '',
    // Change role starts on another role than the current one.
    role: mode === 'change-role' ? ROLES.find((r) => r !== a.role)! : a.role,
    buildingId: roomBuilding ? String(roomBuilding.id) : '',
    roomId: a.room ? String(a.room.id) : '',
    hourlyRate: a.hourlyRate !== null ? String(a.hourlyRate) : '',
    specialtyIds: a.specialties.map((s) => String(s.id)),
    buildingIds: a.buildings.map((b) => String(b.buildingId)),
    primaryBuildingIds: a.buildings.filter((b) => b.isPrimary).map((b) => String(b.buildingId)),
  }
}

const TITLES: Record<Mode, string> = {
  create: 'New account',
  edit: 'Edit account',
  'change-role': 'Change role',
}

function AccountForm({
  mode,
  account,
  buildings,
  specialties,
}: {
  mode: Mode
  account: AccountDetail | null
  buildings: BuildingOption[]
  specialties: SpecialtyRef[]
}) {
  const navigate = useNavigate()
  const [created, setCreated] = useState<{ id: number; username: string; password: string } | null>(
    null,
  )
  const create = useAccountMutation(accountsApi.create)
  const update = useAccountMutation(accountsApi.update)
  const changeRole = useAccountMutation(accountsApi.changeRole)
  const mutation = mode === 'create' ? create : mode === 'edit' ? update : changeRole

  const {
    register,
    handleSubmit,
    setError,
    control,
    formState: { errors },
  } = useForm<AccountFormValues>({
    resolver: zodResolver(accountSchema),
    defaultValues: toFormValues(mode, account, buildings),
  })
  const role = useWatch({ control, name: 'role' })

  const onSubmit = handleSubmit((v) => {
    const details = toRoleDetails(v)
    const onError = (e: Parameters<typeof applyFieldErrors>[0]) => applyFieldErrors(e, setError)
    if (mode === 'create') {
      create.mutate(
        {
          ...details,
          username: v.username,
          fullName: v.fullName,
          email: blank(v.email),
          phoneNumber: blank(v.phoneNumber),
          role: v.role,
        },
        {
          onSuccess: (r) =>
            setCreated({
              id: r.account.id,
              username: r.account.username,
              password: r.temporaryPassword,
            }),
          onError,
        },
      )
    } else if (mode === 'edit') {
      update.mutate(
        {
          ...details,
          id: account!.id,
          fullName: v.fullName,
          email: blank(v.email),
          phoneNumber: blank(v.phoneNumber),
        },
        { onSuccess: (r) => navigate(accountDetailPath(r.id)), onError },
      )
    } else {
      changeRole.mutate(
        { ...details, id: account!.id, role: v.role },
        { onSuccess: (r) => navigate(accountDetailPath(r.id), { replace: true }), onError },
      )
    }
  })

  const contactLocked = mode === 'change-role'
  const showError = mutation.error && Object.keys(mutation.error.fieldErrors).length === 0

  return (
    <div className="flex max-w-[720px] flex-col gap-5">
      <PageHeader
        title={TITLES[mode]}
        description={
          mode === 'change-role'
            ? `${account!.fullName} has no history yet, so the account can move to another role. Its id changes; the username and password stay.`
            : mode === 'edit'
              ? `${account!.fullName} · ${ROLE_LABELS[account!.role]}`
              : 'There is no self-registration: the system makes a temporary password the user changes at first sign-in.'
        }
      />

      <form onSubmit={onSubmit} noValidate className="flex flex-col gap-5">
        <Card>
          <CardHeader title="Account" />
          <div className="grid gap-4 sm:grid-cols-2">
            <TextField
              label="Username"
              autoComplete="off"
              readOnly={mode !== 'create'}
              hint={mode === 'create' ? 'Used to sign in. Cannot change later.' : undefined}
              error={errors.username?.message}
              {...register('username')}
            />
            <TextField
              label="Full name"
              readOnly={contactLocked}
              error={errors.fullName?.message}
              {...register('fullName')}
            />
            <TextField
              label="E-mail"
              type="email"
              hint="Optional, for contact"
              readOnly={contactLocked}
              error={errors.email?.message}
              {...register('email')}
            />
            <TextField
              label="Phone"
              type="tel"
              hint="Optional"
              readOnly={contactLocked}
              error={errors.phoneNumber?.message}
              {...register('phoneNumber')}
            />
            {mode === 'edit' ? (
              // Not registered: the role keeps its default value; it changes only through "Change role".
              <TextField label="Role" readOnly value={ROLE_LABELS[account!.role]} />
            ) : (
              <Select label="Role" error={errors.role?.message} {...register('role')}>
                {ROLES.filter((r) => mode !== 'change-role' || r !== account!.role).map((r) => (
                  <option key={r} value={r}>
                    {ROLE_LABELS[r]}
                  </option>
                ))}
              </Select>
            )}
          </div>
        </Card>

        <RoleDetailsCard
          role={role}
          register={register}
          control={control}
          errors={errors}
          buildings={buildings}
          specialties={specialties}
        />

        {showError && <Alert>{mutation.error.message}</Alert>}
        <div className="flex justify-end gap-2">
          <LinkButton variant="ghost" to={account ? accountDetailPath(account.id) : paths.accounts}>
            Cancel
          </LinkButton>
          <Button type="submit" variant="primary" loading={mutation.isPending}>
            {mode === 'create'
              ? 'Create account'
              : mode === 'edit'
                ? 'Save changes'
                : 'Change role'}
          </Button>
        </div>
      </form>

      {created && (
        <TemporaryPasswordDialog
          open
          username={created.username}
          password={created.password}
          onClose={() => navigate(accountDetailPath(created.id), { replace: true })}
        />
      )}
    </div>
  )
}

interface RoleDetailsProps {
  role: AccountFormValues['role']
  register: UseFormRegister<AccountFormValues>
  control: ReturnType<typeof useForm<AccountFormValues>>['control']
  errors: ReturnType<typeof useForm<AccountFormValues>>['formState']['errors']
  buildings: BuildingOption[]
  specialties: SpecialtyRef[]
}

/** The fields that only one role has. Admin has none. */
function RoleDetailsCard({
  role,
  register,
  control,
  errors,
  buildings,
  specialties,
}: RoleDetailsProps) {
  const buildingId = useWatch({ control, name: 'buildingId' })
  const chosenBuildings = useWatch({ control, name: 'buildingIds' })

  if (role === 'admin') return null

  if (role === 'customer') {
    const rooms =
      buildings
        .find((b) => String(b.id) === buildingId)
        ?.floors.flatMap((f) =>
          f.rooms.filter((r) => !r.isCommonArea).map((r) => ({ ...r, floor: f.name })),
        ) ?? []
    return (
      <Card>
        <CardHeader title="Rented room" />
        <p className="text-muted -mt-2 mb-4 text-sm">
          The customer reports problems in this room and in the common areas of the same building.
        </p>
        <div className="grid gap-4 sm:grid-cols-2">
          <Select label="Building" placeholder="Choose a building" {...register('buildingId')}>
            {buildings.map((b) => (
              <option key={b.id} value={b.id}>
                {b.name}
              </option>
            ))}
          </Select>
          <Select
            label="Room"
            placeholder={buildingId ? 'Choose a room' : 'Choose a building first'}
            error={errors.roomId?.message}
            {...register('roomId')}
          >
            {rooms.map((r) => (
              <option key={r.id} value={r.id}>
                {r.floor} · {r.name}
              </option>
            ))}
          </Select>
        </div>
      </Card>
    )
  }

  if (role === 'technician') {
    return (
      <Card>
        <CardHeader title="Technician" />
        <div className="flex flex-col gap-4">
          <TextField
            label="Hourly rate (VND)"
            inputMode="decimal"
            hint="Optional, used for labour cost"
            className="max-w-60"
            error={errors.hourlyRate?.message}
            {...register('hourlyRate')}
          />
          <fieldset>
            <legend className="mb-2 text-sm font-medium">Specialties</legend>
            <div className="grid gap-2 sm:grid-cols-3">
              {specialties.map((s) => (
                <label key={s.id} className="flex items-center gap-2 text-sm">
                  <input
                    type="checkbox"
                    value={s.id}
                    className="accent-primary size-4"
                    {...register('specialtyIds')}
                  />
                  {s.name}
                </label>
              ))}
            </div>
          </fieldset>
        </div>
      </Card>
    )
  }

  // staff and manager: the buildings they look after
  return (
    <Card>
      <CardHeader title="Buildings" />
      <p className="text-muted -mt-2 mb-4 text-sm">
        {role === 'staff'
          ? 'Staff handle the cases of these buildings. One primary staff member per building: choosing one here replaces the current one.'
          : 'The manager sees the cases and plans of these buildings.'}
      </p>
      <ul className="flex flex-col">
        {buildings.map((b) => {
          const id = String(b.id)
          const chosen = chosenBuildings.includes(id)
          return (
            <li
              key={b.id}
              className="border-rule flex items-center justify-between gap-3 border-b py-2.5 last:border-0"
            >
              <label className="flex items-center gap-2 text-sm">
                <input
                  type="checkbox"
                  value={id}
                  className="accent-primary size-4"
                  {...register('buildingIds')}
                />
                {b.name}
              </label>
              {role === 'staff' && (
                <label className={`flex items-center gap-2 text-sm ${chosen ? '' : 'text-muted'}`}>
                  <input
                    type="checkbox"
                    value={id}
                    className="accent-primary size-4"
                    {...register('primaryBuildingIds')}
                  />
                  Primary
                </label>
              )}
            </li>
          )
        })}
      </ul>
    </Card>
  )
}
