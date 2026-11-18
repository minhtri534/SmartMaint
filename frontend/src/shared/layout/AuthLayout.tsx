import { Outlet } from 'react-router'
import { ProductMark } from '@/shared/ui'
import { Ribbon } from './Ribbon'
import { ThemePill } from './ThemePill'

/*
 * AUTH-* pages (sign in, change password). Layout chosen by the Leader 18/11: the product's line on the left, the
 * form on a pane of frosted glass on the right, over slow ribbons of light. The scene has a light and a night version
 * (tokens in .auth-scene, index.css), chosen with the pill in the corner. On a phone the line is dropped and the pane
 * takes the width.
 */

export function AuthLayout() {
  return (
    <div className="auth-scene relative isolate min-h-full overflow-hidden">
      <div aria-hidden="true" className="absolute inset-0 -z-10 overflow-hidden">
        <Ribbon
          id="ribbon-far"
          d="M 1840 500 C 1580 420, 1520 880, 1240 1120"
          width={96}
          drift="c"
          opacity={0.7}
        />
        <Ribbon
          id="ribbon-thin"
          d="M -220 130 C 240 -40, 460 250, 820 110 C 1100 10, 1320 -50, 1820 170"
          width={30}
          drift="b"
          opacity={0.75}
        />
        <Ribbon
          id="ribbon-low"
          d="M -260 640 C 120 450, 420 1010, 720 780 C 900 640, 1040 880, 1180 1140"
          width={112}
          drift="b"
        />
        <Ribbon
          id="ribbon-main"
          d="M 1480 -260 C 1620 160, 1340 230, 1130 370 C 900 520, 1000 730, 1240 800 C 1420 860, 1470 1000, 1400 1180"
          width={156}
          drift="a"
        />
        {/* keeps the line of text readable where a ribbon passes behind it */}
        <div className="auth-veil absolute inset-0" />
        {/* a soft glow in the top-left corner (Leader 18/11): colour without a shape next to the logo */}
        <div className="auth-glow auth-drift-b" />
      </div>

      <div className="absolute top-4 left-4 z-10 sm:top-6 sm:left-8">
        <ThemePill />
      </div>

      <div className="mx-auto grid min-h-dvh w-full max-w-[1240px] content-center items-center gap-6 px-4 pt-20 pb-8 sm:px-8 lg:grid-cols-[minmax(0,1fr)_420px] lg:gap-16 lg:px-12 lg:py-8">
        <div className="mx-auto w-full max-w-[420px] lg:mx-0 lg:max-w-none">
          <div className="flex items-center gap-2.5">
            <ProductMark className="size-7" />
            <span className="text-xl font-bold tracking-[-0.02em]">SmartMaint</span>
          </div>
          <div className="mt-8 hidden lg:block">
            <p className="max-w-[12ch] text-[clamp(2.75rem,4.6vw,4rem)] leading-[1.05] font-bold tracking-[-0.03em]">
              Keep every building running.
            </p>
            <p className="text-muted mt-5 max-w-[42ch] text-base leading-7">
              Repair requests, preventive maintenance and emergencies for office towers and
              apartments.
            </p>
          </div>
        </div>

        <main className="auth-glass mx-auto w-full max-w-[420px] rounded-[24px] p-7 sm:p-9 lg:mx-0">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
