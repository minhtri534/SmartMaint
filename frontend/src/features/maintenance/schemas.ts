import { z } from 'zod'

/** Mirrors backend MaintenanceValidators. */

export const checklistSchema = z.object({
  name: z.string().trim().min(3, 'At least 3 characters.').max(120),
  description: z.string().trim().max(500),
  categoryId: z.string().min(1, 'Choose the service type.'),
  tasks: z
    .array(z.object({ name: z.string().trim().min(3, 'At least 3 characters.').max(200) }))
    .min(1, 'Add at least one item.')
    .max(50),
})
export type ChecklistFormValues = z.infer<typeof checklistSchema>

export const planSchema = z.object({
  frequency: z.string().regex(/^\d{1,3}$/, 'A whole number, 1 to 365.'),
  frequencyType: z.enum(['day', 'week', 'month', 'year']),
  leadDays: z.string().regex(/^\d{1,2}$/, '0 to 30 days.'),
  checklistId: z.string().min(1, 'Choose a checklist.'),
  estimatedHours: z.string().regex(/^(\d{1,2}(\.\d{1,2})?)?$/, 'Hours, e.g. 1.5.'),
  priority: z.enum(['emergency', 'high', 'normal', 'low']),
  startDate: z.string().min(1, 'Choose the first due day.'),
  endDate: z.string(),
  technicianId: z.string(),
  assetIds: z.array(z.string()).min(1, 'Choose at least one asset.'),
})
export type PlanFormValues = z.infer<typeof planSchema>

export const rejectPlanSchema = z.object({
  reason: z.string().trim().min(10, 'At least 10 characters.').max(500),
})
export type RejectPlanValues = z.infer<typeof rejectPlanSchema>
