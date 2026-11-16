import { Outlet } from 'react-router'
import { ProductMark } from '@/shared/ui'

/*
 * AUTH-* pages (sign in, change password), layout chosen by Leader 16/11: the photograph covers the screen under
 * a navy veil, the form sits on a white card in the middle. Works the same on a phone (the card takes the width).
 *
 * Photo: "Free Saigon River image" (Ho Chi Minh City by the river at dusk), CC0 via rawpixel,
 * https://www.rawpixel.com/image/5911021 — public domain, no credit required. It shows the city, not a client's
 * building. Only 1024px wide, so the veil and a slight scale keep its softness from reading as a flaw.
 */
export function AuthLayout() {
  return (
    <div className="relative grid min-h-full place-items-center overflow-hidden bg-[#0f1b2d] px-4 py-10">
      <img
        src="/images/signin-saigon.webp"
        alt=""
        aria-hidden="true"
        className="absolute inset-0 size-full scale-[1.03] object-cover blur-[1px]"
      />
      <div aria-hidden="true" className="absolute inset-0 bg-[rgb(10_18_32/0.42)]" />

      <main className="bg-surface relative w-full max-w-[420px] rounded-[14px] p-7 shadow-[0_24px_64px_-16px_rgb(0_0_0/0.45)] sm:p-8">
        <div className="mb-6 flex items-center gap-2.5">
          <ProductMark className="size-6" />
          <span className="text-lg font-bold tracking-[-0.02em]">SmartMaint</span>
        </div>
        <Outlet />
      </main>

      <p className="absolute bottom-5 left-6 text-xs text-white/80">
        Building maintenance for rented office towers
      </p>
    </div>
  )
}
