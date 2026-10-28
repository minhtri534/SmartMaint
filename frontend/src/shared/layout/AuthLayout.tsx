import { Outlet } from 'react-router'
import { ProductMark } from '@/shared/ui'

/** AUTH-* pages: the product on the tinted frame, one white card (no sidebar). */
export function AuthLayout() {
  return (
    <main className="flex min-h-full flex-col items-center justify-center gap-6 p-4">
      <div className="flex items-center gap-2">
        <ProductMark className="size-6" />
        <span className="text-lg font-semibold tracking-[-0.01em]">SmartMaint</span>
      </div>
      <Outlet />
      <p className="text-muted text-xs">Building maintenance for rented office towers</p>
    </main>
  )
}
