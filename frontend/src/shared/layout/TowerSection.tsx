import { motion, useReducedMotion } from 'motion/react'
import { EASE_GLIDE, EASE_SNAP } from '@/shared/utils/motion'

/**
 * The sign-in page's picture: the logo ("Tầng lệch") drawn as a whole tower section, 12 floors and a
 * roof with the floor numbers of a lift panel. On load the tower rises floor by floor from the ground,
 * then the floor under service slides out by a third, in the accent. Seen once a day, so it may perform.
 * Decorative: hidden from assistive tech; the page says what the product is in words.
 */
const FLOORS = 12
const SERVICED = 6 // the floor that slides out
const PITCH = 36
const SLAB = 24
const TOP = 28
const LEFT = 64
const WIDTH = 288
const SHIFT = WIDTH / 3

interface Slab {
  label: string
  y: number
  x: number
  width: number
  serviced: boolean
}

// Top to bottom: roof (set back twice, like the mark), then floors 12 … 1.
const SLABS: Slab[] = [
  { label: 'R', width: 156 },
  { label: String(FLOORS), width: 222 },
  ...Array.from({ length: FLOORS - 1 }, (_, i) => ({
    label: String(FLOORS - 1 - i),
    width: WIDTH,
  })),
].map((s, i) => ({
  ...s,
  y: TOP + i * PITCH,
  x: LEFT,
  serviced: s.label === String(SERVICED),
}))

const GROUND = TOP + SLABS.length * PITCH - (PITCH - SLAB) + 14

/** Delay of the i-th slab counted from the ground: gaps shrink as the tower rises (a wave, not a metronome). */
function riseDelay(fromGround: number) {
  return 0.5 * Math.pow(fromGround / (SLABS.length - 1), 0.75)
}

export function TowerSection({ className }: { className?: string }) {
  const reduce = useReducedMotion()

  return (
    <svg
      viewBox={`0 0 ${LEFT + WIDTH + SHIFT + 16} ${GROUND + 8}`}
      className={className}
      aria-hidden="true"
    >
      {SLABS.map((slab, i) => {
        const delay = reduce ? 0 : riseDelay(SLABS.length - 1 - i)
        // Geometry stays in plain SVG attributes; motion only moves the groups (x / y here are transforms).
        return (
          <motion.g
            key={slab.label}
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.45, ease: EASE_SNAP, delay }}
          >
            <text
              x={LEFT - 16}
              y={slab.y + SLAB / 2}
              dominantBaseline="central"
              textAnchor="end"
              className={
                slab.serviced ? 'fill-ink text-[13px] font-semibold' : 'fill-muted text-[12px]'
              }
            >
              {slab.label}
            </text>
            <motion.g
              initial={{ x: 0 }}
              // the lead move: the serviced floor slides out once the roof is up
              animate={{ x: slab.serviced ? SHIFT : 0 }}
              transition={{ duration: 0.6, ease: EASE_GLIDE, delay: reduce ? 0 : 0.62 }}
            >
              <rect
                x={slab.x}
                y={slab.y}
                width={slab.width}
                height={SLAB}
                rx={5}
                className={slab.serviced ? 'fill-accent' : 'fill-slab'}
              />
            </motion.g>
          </motion.g>
        )
      })}
      <line
        x1={LEFT - 28}
        x2={LEFT + WIDTH + SHIFT + 8}
        y1={GROUND}
        y2={GROUND}
        strokeWidth={2}
        strokeLinecap="round"
        className="stroke-slab"
      />
    </svg>
  )
}
