import { Outlet } from 'react-router'
import { ProductMark } from '@/shared/ui'

/*
 * AUTH-* pages (sign in, change password). Layout chosen by the Leader 18/11: the product's line on the left, the
 * form on a pane of frosted glass on the right, over slow ribbons of light. The scene is always the night one, whatever
 * the theme switch says (tokens in .auth-scene, index.css). On a phone the line is dropped and the pane takes the width.
 *
 * The ribbons are drawn here as SVG, so there is no image to license and they stay sharp on any screen.
 */

/** Light falling on the tube: each band is narrower, brighter and a little further towards the top left. */
const HIGHLIGHTS = [
  { size: 0.8, opacity: 0.1, shift: 0.03 },
  { size: 0.62, opacity: 0.12, shift: 0.06 },
  { size: 0.44, opacity: 0.15, shift: 0.09 },
  { size: 0.28, opacity: 0.2, shift: 0.12 },
  { size: 0.13, opacity: 0.32, shift: 0.14 },
]

interface RibbonProps {
  id: string
  /** The curve, in a 1600 x 900 picture. */
  d: string
  width: number
  /** Which slow drift it follows (keyframes auth-drift-* in index.css). */
  drift: 'a' | 'b' | 'c'
  opacity?: number
}

/** One glossy tube. Each ribbon is its own layer, so moving it costs the browser no repaint. */
function Ribbon({ id, d, width, drift, opacity = 1 }: RibbonProps) {
  return (
    <svg
      viewBox="0 0 1600 900"
      preserveAspectRatio="xMidYMid slice"
      className={`auth-ribbon auth-drift-${drift}`}
      style={{ opacity }}
    >
      <defs>
        <linearGradient id={id} gradientUnits="userSpaceOnUse" x1="0" y1="0" x2="1600" y2="900">
          <stop offset="0" stopColor="var(--ribbon-deep)" />
          <stop offset="0.38" stopColor="var(--ribbon-mid)" />
          <stop offset="0.62" stopColor="var(--ribbon-bright)" />
          <stop offset="1" stopColor="var(--ribbon-deep)" />
        </linearGradient>
        <filter id={`${id}-shade`} filterUnits="userSpaceOnUse" x="-500" y="-500" width="2600" height="1900">
          <feGaussianBlur stdDeviation={width * 0.24} />
        </filter>
        <filter id={`${id}-soft`} filterUnits="userSpaceOnUse" x="-500" y="-500" width="2600" height="1900">
          <feGaussianBlur stdDeviation={width * 0.06} />
        </filter>
      </defs>
      <g fill="none" strokeLinecap="round">
        <path
          d={d}
          stroke="var(--ribbon-shade)"
          strokeWidth={width * 1.08}
          opacity="0.6"
          filter={`url(#${id}-shade)`}
          transform={`translate(0 ${width * 0.26})`}
        />
        <path d={d} stroke={`url(#${id})`} strokeWidth={width} />
        <g filter={`url(#${id}-soft)`}>
          {HIGHLIGHTS.map((h) => (
            <path
              key={h.size}
              d={d}
              stroke="var(--ribbon-light)"
              strokeWidth={width * h.size}
              opacity={h.opacity}
              transform={`translate(${-width * h.shift} ${-width * h.shift})`}
            />
          ))}
        </g>
      </g>
    </svg>
  )
}

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
      </div>

      <div className="mx-auto grid min-h-dvh w-full max-w-[1240px] content-center items-center gap-6 px-4 py-8 sm:px-8 lg:grid-cols-[minmax(0,1fr)_420px] lg:gap-16 lg:px-12">
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
