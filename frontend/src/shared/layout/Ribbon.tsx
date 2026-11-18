/*
 * The product's ribbon of light, first seen on the sign-in page and echoed faintly across the desk (Leader 18/11). Drawn as
 * SVG, so there is no image to license and it stays sharp on any screen. Colours: --ribbon-* in index.css.
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
  /** Which slow drift it follows (keyframes auth-drift-* in index.css); none = it stays still. */
  drift?: 'a' | 'b' | 'c'
  opacity?: number
}

/** One glossy tube. Each ribbon is its own layer, so moving it costs the browser no repaint. */
export function Ribbon({ id, d, width, drift, opacity = 1 }: RibbonProps) {
  return (
    <svg
      viewBox="0 0 1600 900"
      preserveAspectRatio="xMidYMid slice"
      className={drift ? `auth-ribbon auth-drift-${drift}` : 'auth-ribbon'}
      style={{ opacity }}
    >
      <defs>
        <linearGradient id={id} gradientUnits="userSpaceOnUse" x1="0" y1="0" x2="1600" y2="900">
          <stop offset="0" stopColor="var(--ribbon-deep)" />
          <stop offset="0.38" stopColor="var(--ribbon-mid)" />
          <stop offset="0.62" stopColor="var(--ribbon-bright)" />
          <stop offset="1" stopColor="var(--ribbon-deep)" />
        </linearGradient>
        <filter
          id={`${id}-shade`}
          filterUnits="userSpaceOnUse"
          x="-500"
          y="-500"
          width="2600"
          height="1900"
        >
          <feGaussianBlur stdDeviation={width * 0.24} />
        </filter>
        <filter
          id={`${id}-soft`}
          filterUnits="userSpaceOnUse"
          x="-500"
          y="-500"
          width="2600"
          height="1900"
        >
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
