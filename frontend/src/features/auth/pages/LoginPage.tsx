import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Navigate, useLocation, useNavigate, type Location } from 'react-router'
import { paths } from '@/app/paths'
import { Alert, Button, Card, TextField } from '@/shared/ui'
import { applyFieldErrors } from '@/shared/utils/apply-field-errors'
import { useAuth, useLogin } from '../hooks'
import { loginSchema, type LoginFormValues } from '../schemas'

/**
 * AUTH-01 — SAMPLE PAGE. Every page follows this shape:
 *   zod schema (schemas.ts) -> react-hook-form -> mutation/query hook (hooks.ts) -> api function (api.ts)
 *   server field errors -> applyFieldErrors; other errors -> <Alert> by error.code
 */
export function LoginPage() {
  const { user, signOutReason } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const from = (location.state as { from?: Location } | null)?.from?.pathname ?? paths.home
  const login = useLogin()

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) })

  if (user) return <Navigate to={from} replace />

  const onSubmit = handleSubmit((values) =>
    login.mutate(values, {
      onSuccess: () => navigate(from, { replace: true }),
      onError: (error) => applyFieldErrors(error, setError),
    }),
  )

  const showGenericError = login.error && Object.keys(login.error.fieldErrors).length === 0

  return (
    <Card padding="roomy" className="w-full max-w-[400px]">
      <h1 className="text-title font-semibold tracking-[-0.01em]">Sign in</h1>
      <p className="text-muted mt-1 text-sm">Use the account your Administrator gave you.</p>

      <form onSubmit={onSubmit} noValidate className="mt-6 flex flex-col gap-4">
        {signOutReason === 'session_expired' && !login.error && (
          <Alert tone="info">Your session has expired. Please sign in again.</Alert>
        )}
        {showGenericError && <Alert>{login.error.message}</Alert>}

        <TextField
          label="Email"
          type="email"
          autoComplete="username"
          autoFocus
          error={errors.email?.message}
          {...register('email')}
        />
        <TextField
          label="Password"
          type="password"
          autoComplete="current-password"
          error={errors.password?.message}
          {...register('password')}
        />
        <Button type="submit" variant="primary" loading={login.isPending} className="mt-2 h-[38px]">
          Sign in
        </Button>
      </form>
    </Card>
  )
}
