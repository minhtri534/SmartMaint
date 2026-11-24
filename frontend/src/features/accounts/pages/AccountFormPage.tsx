import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import {
  useController,
  useForm,
  useWatch,
  type Control,
  type UseFormRegister,
} from 'react-hook-form'
import { useNavigate, useParams } from 'react-router'
import { accountDetailPath, paths } from '@/app/paths'
import { ROLE_LABELS, ROLES } from '@/shared/types/role'
import {
  Alert,
  Button,
  ErrorBlock,
  FormActions,
  FormSection,
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

type Mode = 'create' | 'edit'

/**
 * ADM-03 Create / Edit user.
 * The role decides the extra fields: technician -> rate + specialties, staff -> buildings (+ primary),
 * manager -> buildings.
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

function toFormValues(a: AccountDetail | null): AccountFormValues {
  if (!a) return emptyAccountForm
  return {
    fullName: a.fullName,
    email: a.email ?? '',
    phoneNumber: a.phoneNumber ?? '',
    role: a.role,
    hourlyRate: a.hourlyRate !== null ? String(a.hourlyRate) : '',
    specialtyIds: a.specialties.map((s) => String(s.id)),
    buildingIds: a.buildings.map((b) => String(b.buildingId)),
    primaryBuildingIds: a.buildings.filter((b) => b.isPrimary).map((b) => String(b.buildingId)),
  }
}

const TITLES: Record<Mode, string> = {
  create: 'New account',
  edit: 'Edit account',
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
  const [created, setCreated] = useState<{
    id: number
    name: string
    email: string | null
    password: string | null
  } | null>(null)
  const create = useAccountMutation(accountsApi.create)
  const update = useAccountMutation(accountsApi.update)
  const mutation = mode === 'create' ? create : update

  const {
    register,
    handleSubmit,
    setError,
    control,
    formState: { errors },
  } = useForm<AccountFormValues>({
    resolver: zodResolver(accountSchema),
    defaultValues: toFormValues(account),
  })
  const role = useWatch({ control, name: 'role' })

  const onSubmit = handleSubmit((v) => {
    const details = toRoleDetails(v)
    const onError = (e: Parameters<typeof applyFieldErrors>[0]) => applyFieldErrors(e, setError)
    if (mode === 'create') {
      create.mutate(
        {
          ...details,
          fullName: v.fullName,
          email: blank(v.email),
          phoneNumber: blank(v.phoneNumber),
          role: v.role,
        },
        {
          onSuccess: (r) =>
            setCreated({
              id: r.account.id,
              name: r.account.fullName,
              email: r.account.email,
              password: r.temporaryPassword,
            }),
          onError,
        },
      )
    } else {
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
    }
  })

  const showError = mutation.error && Object.keys(mutation.error.fieldErrors).length === 0

  return (
    <div className="flex max-w-[1040px] flex-col gap-5">
      <PageHeader
        title={TITLES[mode]}
        description={
          mode === 'edit'
            ? `${account!.fullName} · ${ROLE_LABELS[account!.role]}`
            : 'There is no self-registration: the system makes a temporary password the user changes at first sign-in.'
        }
      />

      <form onSubmit={onSubmit} noValidate className="flex flex-col gap-6">
        <FormSection
          title="Account"
          description="Who the person is and how they sign in: with the e-mail or the phone number."
        >
          <div className="grid gap-4 sm:grid-cols-2">
            <TextField
              label="Full name"
              error={errors.fullName?.message}
              {...register('fullName')}
            />
            <TextField
              label="E-mail"
              type="email"
              hint={
                mode === 'create' ? 'Signs in; the temporary password is sent here' : 'Signs in'
              }
              error={errors.email?.message}
              {...register('email')}
            />
            <TextField
              label="Phone"
              type="tel"
              hint="Optional; also signs in"
              error={errors.phoneNumber?.message}
              {...register('phoneNumber')}
            />
            {mode === 'edit' ? (
              // Not registered: the role of an account never changes.
              <TextField label="Role" readOnly value={ROLE_LABELS[account!.role]} />
            ) : (
              <Select label="Role" error={errors.role?.message} {...register('role')}>
                {ROLES.map((r) => (
                  <option key={r} value={r}>
                    {ROLE_LABELS[r]}
                  </option>
                ))}
              </Select>
            )}
          </div>
        </FormSection>

        <RoleDetailsCard
          role={role}
          register={register}
          control={control}
          errors={errors}
          buildings={buildings}
          specialties={[
            ...specialties,
            ...(account?.specialties.filter((s) => !specialties.some((o) => o.id === s.id)) ?? []),
          ]}
        />

        <FormActions error={showError && <Alert>{mutation.error.message}</Alert>}>
          <LinkButton variant="ghost" to={account ? accountDetailPath(account.id) : paths.accounts}>
            Cancel
          </LinkButton>
          <Button type="submit" variant="primary" loading={mutation.isPending}>
            {mode === 'create' ? 'Create account' : 'Save changes'}
          </Button>
        </FormActions>
      </form>

      {created && (
        <TemporaryPasswordDialog
          name={created.name}
          email={created.email}
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
  const chosenBuildings = useWatch({ control, name: 'buildingIds' })

  if (role === 'admin' || role === 'customer') return null

  if (role === 'technician') {
    return (
      <FormSection
        title="Technician"
        description="The rate prices their labour on a case; the specialties tell the staff what they can repair."
      >
        <div className="flex flex-col gap-4">
          <TextField
            label="Hourly rate (VND)"
            inputMode="decimal"
            hint="Optional, used for labour cost"
            className="max-w-60"
            error={errors.hourlyRate?.message}
            {...register('hourlyRate')}
          />
          <SpecialtyPicker control={control} specialties={specialties} />
        </div>
      </FormSection>
    )
  }

  // staff and manager: the buildings they look after
  return (
    <FormSection
      title="Buildings"
      description={
        role === 'staff'
          ? 'Staff handle the cases of these buildings. One primary staff member per building: choosing one here replaces the current one.'
          : 'The manager sees the cases and plans of these buildings.'
      }
    >
      <ul className="-my-2.5 flex flex-col">
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
    </FormSection>
  )
}

/**
 * Meeting note 18/11: the technician's specialties as a list (each with Remove) and a dropdown of the others; choosing
 * one adds it to the list.
 */
function SpecialtyPicker({
  control,
  specialties,
}: {
  control: Control<AccountFormValues>
  specialties: SpecialtyRef[]
}) {
  const { field } = useController({ control, name: 'specialtyIds' })
  const chosen = field.value
  const others = specialties.filter((s) => !chosen.includes(String(s.id)))
  return (
    <div className="flex flex-col gap-3">
      <span className="text-sm font-medium">Specialties</span>
      {chosen.length === 0 ? (
        <p className="text-muted text-sm">None yet.</p>
      ) : (
        <ul className="border-border rounded-card flex flex-col border">
          {chosen.map((id) => (
            <li
              key={id}
              className="border-rule flex items-center justify-between gap-3 border-b px-3 py-2 text-sm last:border-0"
            >
              {specialties.find((s) => String(s.id) === id)?.name ?? `#${id}`}
              <Button
                variant="ghost"
                className="text-muted! hover:text-negative!"
                onClick={() => field.onChange(chosen.filter((c) => c !== id))}
              >
                Remove
              </Button>
            </li>
          ))}
        </ul>
      )}
      {others.length > 0 && (
        <Select
          label="Add a specialty"
          className="max-w-72"
          value=""
          onChange={(e) => e.target.value && field.onChange([...chosen, e.target.value])}
        >
          <option value="">Choose…</option>
          {others.map((s) => (
            <option key={s.id} value={s.id}>
              {s.name}
            </option>
          ))}
        </Select>
      )}
    </div>
  )
}
