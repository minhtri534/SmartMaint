import { QueryClientProvider } from '@tanstack/react-query'
import { MotionConfig } from 'motion/react'
import { RouterProvider } from 'react-router'
import { AuthProvider } from '@/features/auth'
import { queryClient } from '@/lib/query-client'
import { router } from './router'

export function App() {
  return (
    // reducedMotion="user": with "reduce motion" on, movement is skipped and only opacity animates.
    <MotionConfig reducedMotion="user">
      <QueryClientProvider client={queryClient}>
        <AuthProvider>
          <RouterProvider router={router} />
        </AuthProvider>
      </QueryClientProvider>
    </MotionConfig>
  )
}
