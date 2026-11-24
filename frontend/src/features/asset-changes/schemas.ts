import { z } from 'zod'

/** Mirrors backend RejectChangeListRequestValidator. (The tree editor checks its small dialogs itself.) */
export const rejectChangeListSchema = z.object({
  reason: z.string().trim().min(10, 'At least 10 characters.').max(500),
})
export type RejectChangeListValues = z.infer<typeof rejectChangeListSchema>
