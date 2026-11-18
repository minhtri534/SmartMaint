import { motion, useReducedMotion } from 'motion/react'
import { setTheme, useTheme, type Theme } from '@/shared/theme/theme'
import { Icon } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { slide } from '@/shared/utils/motion'

const OPTIONS: { theme: Theme; label: string; icon: 'sun' | 'moon' }[] = [
  { theme: 'light', label: 'Light', icon: 'sun' },
  { theme: 'dark', label: 'Dark', icon: 'moon' },
]

/** The sun / moon pill in the top-left corner of the sign-in page (Leader 18/11). Same choice as the switch in the app. */
export function ThemePill() {
  const current = useTheme()
  const reduce = useReducedMotion()
  return (
    <div role="group" aria-label="Theme" className="auth-pill flex rounded-full p-1">
      {OPTIONS.map(({ theme, label, icon }) => {
        const active = theme === current
        return (
          <button
            key={theme}
            type="button"
            aria-pressed={active}
            aria-label={`${label} theme`}
            title={`${label} theme`}
            onClick={() => setTheme(theme)}
            className={cn(
              'relative grid size-8 place-items-center rounded-full transition-colors duration-150',
              active ? 'text-ink' : 'text-muted hover:text-ink',
            )}
          >
            {active && (
              <motion.span
                layoutId="theme-pill"
                transition={reduce ? { duration: 0 } : slide}
                className="auth-pill-thumb absolute inset-0 rounded-full"
              />
            )}
            <Icon name={icon} className="relative size-4" />
          </button>
        )
      })}
    </div>
  )
}
