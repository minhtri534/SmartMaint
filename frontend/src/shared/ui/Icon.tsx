import { cn } from '@/shared/utils/cn'

/** Outline icons, 24-unit grid, 1.5px stroke. One grey set: nav, a single affordance (menu, back), never in a tinted box. */
const PATHS = {
  queue:
    'M4 13h4l2 3h4l2-3h4 M4 13V6a2 2 0 0 1 2-2h12a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2z',
  requests: 'M14 3H7a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V8z M14 3v5h5 M9 13h6 M9 17h4',
  plus: 'M12 5v14 M5 12h14',
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

/** The SmartMaint mark: an office tower whose floors are being checked off, in the accent. */
export function ProductMark({ className }: { className?: string }) {
  return (
    <svg viewBox="0 0 20 20" aria-hidden="true" className={cn('size-5 flex-none', className)}>
      <rect x="2" y="1" width="16" height="18" rx="4" className="fill-accent" />
      <path d="M6 6h5 M6 10h5 M6 14h5" stroke="white" strokeWidth="1.6" strokeLinecap="round" />
      <path
        d="M12.8 13.6l1.2 1.2 2-2.4"
        stroke="white"
        strokeWidth="1.4"
        fill="none"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  )
}
