import { useSyncExternalStore } from 'react'

/*
 * Light / dark theme (ui/mercato-style, 17/11). The choice is per browser (localStorage); with no choice the
 * theme follows the system setting. It is written as data-theme on <html>, which index.css reads.
 */
export type Theme = 'light' | 'dark'

const KEY = 'smartmaint.theme'
const listeners = new Set<() => void>()
const media = () => window.matchMedia('(prefers-color-scheme: dark)')

function stored(): Theme | null {
  try {
    const value = localStorage.getItem(KEY)
    return value === 'light' || value === 'dark' ? value : null
  } catch {
    return null
  }
}

function current(): Theme {
  return stored() ?? (media().matches ? 'dark' : 'light')
}

function apply() {
  document.documentElement.dataset.theme = current()
  listeners.forEach((l) => l())
}

/** Call once before the first render, so the page never flashes the wrong theme. */
export function initTheme() {
  apply()
  media().addEventListener('change', () => {
    if (!stored()) apply()
  })
}

export function setTheme(theme: Theme) {
  try {
    localStorage.setItem(KEY, theme)
  } catch {
    // private window: the theme still changes for this visit
  }
  apply()
}

export function useTheme(): Theme {
  return useSyncExternalStore((listener) => {
    listeners.add(listener)
    return () => listeners.delete(listener)
  }, current)
}
