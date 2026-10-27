import { Outlet } from 'react-router'

/** Centered card layout for AUTH-* pages (no sidebar). */
export function AuthLayout() {
  return (
    <main className="flex min-h-full flex-col items-center justify-center gap-6 p-6">
      <div className="text-lg font-semibold tracking-tight">SmartMaint</div>
      <Outlet />
    </main>
  )
}
