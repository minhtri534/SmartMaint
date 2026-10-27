const KEY = 'smartmaint.accessToken'

/** Access token persistence. Only AuthProvider and api-client touch this. */
export const tokenStorage = {
  get: (): string | null => {
    try {
      return localStorage.getItem(KEY)
    } catch {
      return null
    }
  },
  set: (token: string) => {
    try {
      localStorage.setItem(KEY, token)
    } catch {
      // storage unavailable (private mode): the session lasts until the tab closes
    }
  },
  clear: () => {
    try {
      localStorage.removeItem(KEY)
    } catch {
      // ignore
    }
  },
}
