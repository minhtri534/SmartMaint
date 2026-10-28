import { isRouteErrorResponse, Link, useRouteError } from 'react-router'
import { paths } from '@/app/paths'

function ErrorScreen({ code, title, message }: { code: string; title: string; message: string }) {
  return (
    <div className="flex min-h-full flex-col items-center justify-center gap-3 p-6 text-center">
      <div className="text-muted text-sm font-medium">{code}</div>
      <h1 className="text-2xl font-semibold">{title}</h1>
      <p className="text-muted max-w-md text-sm">{message}</p>
      <Link
        to={paths.home}
        className="text-ink mt-2 text-sm font-medium underline-offset-4 hover:underline"
      >
        Back to Home
      </Link>
    </div>
  )
}

/** ERR-01 */
export function ForbiddenPage() {
  return (
    <ErrorScreen
      code="403"
      title="Access denied"
      message="You do not have permission to open this page."
    />
  )
}

/** ERR-02 */
export function NotFoundPage() {
  return (
    <ErrorScreen
      code="404"
      title="Page not found"
      message="The page you are looking for does not exist."
    />
  )
}

/** Router-level crash screen (errorElement). */
export function RouteErrorPage() {
  const error = useRouteError()
  if (isRouteErrorResponse(error) && error.status === 404) return <NotFoundPage />
  return (
    <ErrorScreen
      code="500"
      title="Something went wrong"
      message="Please reload the page or try again later."
    />
  )
}
