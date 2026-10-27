/** The only place that reads import.meta.env. Add new VITE_* variables here and in .env.example. */
export const env = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL ?? '',
} as const
