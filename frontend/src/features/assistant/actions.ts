import { useEffect, useRef, useSyncExternalStore } from 'react'
import type { AssistantAction } from './types'

/*
 * The page that is open can offer the chat one shortcut (New request: "Suggest a category from my description").
 * A tiny store instead of a context: the chat lives in the app shell, the page deep inside the router.
 */
let current: AssistantAction | null = null
const listeners = new Set<() => void>()

function set(action: AssistantAction | null) {
  current = action
  listeners.forEach((l) => l())
}

/** The shortcut of the open page, if any (read by the chat). */
export function usePageAction(): AssistantAction | null {
  return useSyncExternalStore(
    (listener) => {
      listeners.add(listener)
      return () => listeners.delete(listener)
    },
    () => current,
  )
}

/**
 * Offer a shortcut in the chat while this page is open. `run` may change between renders (it reads the form):
 * the latest one is always used.
 */
export function useAssistantAction(label: string, run: AssistantAction['run'] | null) {
  const latest = useRef(run)
  useEffect(() => {
    latest.current = run
  })
  const enabled = run !== null
  useEffect(() => {
    if (!enabled) return
    const action: AssistantAction = {
      label,
      run: () => (latest.current ? latest.current() : Promise.reject(new Error('gone'))),
    }
    set(action)
    return () => {
      if (current === action) set(null)
    }
  }, [label, enabled])
}
