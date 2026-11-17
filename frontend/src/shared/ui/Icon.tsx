import { cn } from '@/shared/utils/cn'

/** Outline icons, 24-unit grid, 1.5px stroke. One grey set: nav, a single affordance (menu, back), never in a tinted box. */
const PATHS = {
  queue:
    'M4 13h4l2 3h4l2-3h4 M4 13V6a2 2 0 0 1 2-2h12a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2z',
  requests: 'M14 3H7a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V8z M14 3v5h5 M9 13h6 M9 17h4',
  plus: 'M12 5v14 M5 12h14',
  search: 'M11 18a7 7 0 1 0 0-14 7 7 0 0 0 0 14z M20 20l-4-4',
  wrench:
    'M14.5 6.5a4 4 0 0 0-5.2 5.2L4.5 16.5a1.8 1.8 0 0 0 2.5 2.5l4.8-4.8a4 4 0 0 0 5.2-5.2l-2.3 2.3-2.2-.3-.3-2.2z',
  home: 'M4 11l8-7 8 7 M6 9.5V20h12V9.5',
  logout: 'M15 4h3a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-3 M10 8l-4 4 4 4 M6 12h10',
  menu: 'M4 7h16 M4 12h16 M4 17h16',
  close: 'M6 6l12 12 M18 6L6 18',
  back: 'M14 6l-6 6 6 6',
  users:
    'M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8z M3 20a6 6 0 0 1 12 0 M16 4.5a3.5 3.5 0 0 1 0 6.5 M18 20a5 5 0 0 0-2.5-4.3',
  log: 'M8 6h12 M8 12h12 M8 18h12 M4 6h.01 M4 12h.01 M4 18h.01',
  bell: 'M6 16V11a6 6 0 0 1 12 0v5l1.5 2h-15z M10 20a2 2 0 0 0 4 0',
  clock: 'M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18z M12 7v5l3 2',
  shield: 'M12 3l7 3v6c0 4.5-3 7.5-7 9-4-1.5-7-4.5-7-9V6z',
  building:
    'M5 21V5a2 2 0 0 1 2-2h7a2 2 0 0 1 2 2v16 M16 9h3a1 1 0 0 1 1 1v11 M3 21h18 M9 7h2 M9 11h2 M9 15h2',
  box: 'M3 8l9-5 9 5v8l-9 5-9-5z M3 8l9 5 9-5 M12 13v8',
  list: 'M4 6h16 M4 12h16 M4 18h10',
  chart: 'M4 20h16 M7 16v-5 M12 16V7 M17 16v-8',
  truck:
    'M3 6h11v10H3z M14 10h4l3 3v3h-7z M7 19a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3z M17 19a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3z',
  swap: 'M4 8h14l-3-3 M20 16H6l3 3',
  siren: 'M12 3v2 M5 7l1.5 1.5 M19 7l-1.5 1.5 M7 17v-4a5 5 0 0 1 10 0v4 M5 17h14v3H5z',
  calendar: 'M5 5h14a1 1 0 0 1 1 1v14H4V6a1 1 0 0 1 1-1z M4 10h16 M8 3v4 M16 3v4',
  checklist: 'M10 6h10 M10 12h10 M10 18h10 M4 6l1.5 1.5L8 5 M4 12l1.5 1.5L8 11 M4 18l1.5 1.5L8 17',
  qr: 'M4 4h6v6H4z M14 4h6v6h-6z M4 14h6v6H4z M14 14h2v2h-2z M18 18h2v2h-2z M14 18h2 M18 14h2',
  chat: 'M5 4h14a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2h-7l-4.5 3.5V17H5a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2z M8 9h8 M8 12.5h5',
  send: 'M12 19V5 M6 11l6-6 6 6',
  compose: 'M12 20h8 M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4z',
  sun: 'M12 16a4 4 0 1 0 0-8 4 4 0 0 0 0 8z M12 2v2 M12 20v2 M4.9 4.9l1.4 1.4 M17.7 17.7l1.4 1.4 M2 12h2 M20 12h2 M4.9 19.1l1.4-1.4 M17.7 6.3l1.4-1.4',
  moon: 'M20 14.5A8 8 0 0 1 9.5 4a8 8 0 1 0 10.5 10.5z',
  drop: 'M12 3.5c3 3.6 6 7.1 6 10.5a6 6 0 0 1-12 0c0-3.4 3-6.9 6-10.5z',
  bolt: 'M13 3 5 13.5h6L10 21l8-10.5h-6z',
  snow: 'M12 3v18 M4.2 7.5l15.6 9 M4.2 16.5l15.6-9 M9.5 4.5 12 7l2.5-2.5 M9.5 19.5 12 17l2.5 2.5',
  door: 'M6 21V4.5A1.5 1.5 0 0 1 7.5 3h9A1.5 1.5 0 0 1 18 4.5V21 M4 21h16 M14.5 12.5v.01',
  lift: 'M5 3h14v18H5z M12 3v18 M8.5 9.5 7 8 M8.5 9.5 10 8 M15.5 14.5 14 16 M15.5 14.5 17 16',
  flame:
    'M12 21a6 6 0 0 0 6-6c0-4-3-5.5-4-10-2 2-2.5 4-2.5 5.5C10 9.5 9 8.5 9 7c-2 2-3 4.5-3 8a6 6 0 0 0 6 6z',
  wall: 'M3 5h18v14H3z M3 9.7h18 M3 14.3h18 M9 5v4.7 M15 5v4.7 M12 9.7v4.6 M6 9.7v4.6 M18 9.7v4.6 M9 14.3V19 M15 14.3V19',
  pin: 'M12 21s6-5.4 6-10.5a6 6 0 0 0-12 0C6 15.6 12 21 12 21z M12 12.5a2 2 0 1 0 0-4 2 2 0 0 0 0 4z',
  arrowRight: 'M5 12h14 M13 6l6 6-6 6',
  trendUp: 'M4 16l6-6 4 4 6-6 M14 8h6v6',
  money: 'M4 7h16v10H4z M12 14.5a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5z M7 10v.01 M17 14v.01',
} as const

export type IconName = keyof typeof PATHS

export function Icon({ name, className }: { name: IconName; className?: string }) {
  return (
    <svg
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth={1.5}
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      className={cn('size-4 flex-none', className)}
    >
      <path d={PATHS[name]} />
    </svg>
  )
}

/**
 * The SmartMaint mark: four floors of a building section (roof set back); the floor under service
 * slides out by a third, in the accent. Drawn on a 20-pixel grid so it is crisp at size-5.
 * Master files and app icons: docs/design/brand/.
 */
export function ProductMark({ className }: { className?: string }) {
  return (
    <svg viewBox="0 0 20 20" aria-hidden="true" className={cn('size-5 flex-none', className)}>
      <rect x="2" y="1" width="7" height="3" rx="0.9" className="fill-ink" />
      <rect x="2" y="6" width="12" height="3" rx="0.9" className="fill-ink" />
      <rect x="6" y="11" width="12" height="3" rx="0.9" className="fill-accent" />
      <rect x="2" y="16" width="12" height="3" rx="0.9" className="fill-ink" />
    </svg>
  )
}
