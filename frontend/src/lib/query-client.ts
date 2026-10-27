import { QueryClient } from '@tanstack/react-query'
import type { ApiError } from './api-error'

declare module '@tanstack/react-query' {
  interface Register {
    defaultError: ApiError
  }
}

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      retry: (failureCount, error) => error.status >= 500 && failureCount < 2,
      refetchOnWindowFocus: false,
    },
  },
})
