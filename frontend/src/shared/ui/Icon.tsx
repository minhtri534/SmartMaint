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
