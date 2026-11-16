/**
 * Motion tokens for the `motion` library — the same curves as --ease-* in index.css (emil-design-eng values).
 * Rule of thumb: UI under 300ms, ease-out to enter / exit, no entrance animations on app screens.
 */
export const EASE_SNAP = [0.23, 1, 0.32, 1] as const
export const EASE_GLIDE = [0.77, 0, 0.175, 1] as const

/** Tab underline, selected filter: an element moving to a new place. */
export const slide = { duration: 0.22, ease: EASE_SNAP } as const

/** A page swap inside the sheet: opacity only, nothing travels (navigation is used many times a day). */
export const pageFade = { duration: 0.15, ease: 'easeOut' } as const
