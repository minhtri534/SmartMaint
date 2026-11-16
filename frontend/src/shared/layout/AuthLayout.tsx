import { Outlet } from 'react-router'
import { ProductMark } from '@/shared/ui'
import { TowerSection } from './TowerSection'

/**
 * AUTH-* pages, built like the signed-in shell (framed-sheet): the product on the tinted frame at the left,
 * the form on the white sheet at the right. The frame carries the tower section (the logo grown into a
 * building). Below 1024px the tower goes: a brand row on the frame, then the sheet.
 */
export function AuthLayout() {
  return (
    <div className="min-h-full lg:grid lg:grid-cols-[minmax(0,5fr)_minmax(0,6fr)]">
      <section className="flex flex-col px-4 pt-4 pb-3 lg:px-10 lg:py-10">
        <div className="flex items-center gap-2.5">
          <ProductMark className="size-6" />
          <span className="text-lg font-semibold tracking-[-0.02em]">SmartMaint</span>
        </div>

        <div className="hidden flex-1 items-center justify-center py-8 lg:flex">
          <TowerSection className="h-auto max-h-[56vh] w-full max-w-[420px]" />
        </div>

        <div className="hidden lg:block">
          <p className="text-section font-semibold tracking-[-0.01em]">
            Building maintenance for rented office towers
          </p>
          <p className="text-muted mt-1 max-w-[46ch] text-sm">
            Tenant requests, work orders and preventive plans, floor by floor, for the building
            management office and its technicians.
          </p>
        </div>
      </section>

      <main className="min-w-0 lg:py-2 lg:pr-2">
        <div className="bg-surface lg:border-border lg:rounded-sheet flex min-h-[calc(100vh-56px)] items-start justify-center px-4 pt-10 pb-8 sm:items-center sm:pt-8 lg:min-h-[calc(100vh-16px)] lg:border">
          <Outlet />
        </div>
      </main>
    </div>
  )
}
