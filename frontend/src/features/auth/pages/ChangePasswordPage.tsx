import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useNavigate } from 'react-router'
import { paths } from '@/app/paths'
import { Alert, Button, Card, TextField } from '@/shared/ui'
import { applyFieldErrors } from '@/shared/utils/apply-field-errors'
import { useAuth, useChangePassword } from '../hooks'
import { changePasswordSchema, type ChangePasswordFormValues } from '../schemas'

/** AUTH-04 (forced on first sign-in) and COM-03 (from Profile). */
export function ChangePasswordPage() {
  const { user, signOut } = useAuth()
  const navigate = useNavigate()
  const changePassword = useChangePassword()

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<ChangePasswordFormValues>({ resolver: zodResolver(changePasswordSchema) })

  const onSubmit = handleSubmit(({ currentPassword, newPassword }) =>
    changePassword.mutate(
      { currentPassword, newPassword },
      {
        onSuccess: () => navigate(paths.home, { replace: true }),
        onError: (error) => {
          if (error.code === 'auth.wrong_current_password') {
            setError('currentPassword', { message: error.message })
          } else {
            applyFieldErrors(error, setError)
          }
        },
      },
    ),
  )

  const forced = user?.mustChangePassword === true
  const showGenericError =
    changePassword.error &&
    changePassword.error.code !== 'auth.wrong_current_password' &&
    Object.keys(changePassword.error.fieldErrors).length === 0

  return (
    <Card className="w-full max-w-sm">
      <h1 className="text-xl font-semibold">
        {forced ? 'Set your own password' : 'Change password'}
      </h1>
      {forced && (
        <p className="text-muted mt-1 text-sm">
          You are using a temporary password. Choose a new one to continue.
        </p>
      )}

      <form onSubmit={onSubmit} noValidate className="mt-6 flex flex-col gap-4">
        {showGenericError && <Alert>{changePassword.error.message}</Alert>}
        <TextField
          label="Current password"
          type="password"
          autoComplete="current-password"
          error={errors.currentPassword?.message}
          {...register('currentPassword')}
        />
        <TextField
          label="New password"
          type="password"
          autoComplete="new-password"
          hint="At least 8 characters, with an uppercase letter, a lowercase letter and a number."
          error={errors.newPassword?.message}
          {...register('newPassword')}
        />
        <TextField
          label="Confirm new password"
          type="password"
          autoComplete="new-password"
          error={errors.confirmPassword?.message}
          {...register('confirmPassword')}
        />
        <Button type="submit" loading={changePassword.isPending} className="mt-2">
          Save password
        </Button>
        {forced && (
          <Button variant="ghost" onClick={() => signOut()}>
            Sign out instead
          </Button>
        )}
      </form>
    </Card>
  )
}
