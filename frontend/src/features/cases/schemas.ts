import { z } from 'zod'
import { COST_BEARERS, EXTEND_REASONS, HOLD_REASONS, REJECT_REASONS, URGENCIES } from './types'

/*
 * Form rules of F3. Select boxes keep their value as a string ("" = nothing chosen);
 * pages convert to numbers when they call the API. The backend validators must say the same.
 */

const requiredId = (message: string) => z.string().min(1, message)

/** EMP-02 New request. */
export const newRequestSchema = z.object({
  serviceGroup: requiredId('Choose a group.'),
  serviceTypeId: requiredId('Choose what is wrong.'),
  buildingId: requiredId('Choose a building.'),
  floorId: requiredId('Choose a floor.'),
  roomId: requiredId('Choose a room or area.'),
  assetId: z.string(),
  title: z.string().trim().min(5, 'At least 5 characters.').max(120, 'At most 120 characters.'),
  description: z
    .string()
    .trim()
    .min(15, 'Describe the problem in at least 15 characters.')
    .max(2000, 'At most 2000 characters.'),
  photos: z.array(z.string()).max(5, 'At most 5 photos.'),
})
export type NewRequestFormValues = z.infer<typeof newRequestSchema>

/** DLG-01 Approve: the staff confirms urgency and who pays. */
export const approveSchema = z.object({
  urgency: z.enum(URGENCIES),
  costBearer: z.enum(COST_BEARERS),
})
export type ApproveFormValues = z.infer<typeof approveSchema>

/** DLG-02 Reject. */
export const rejectSchema = z.object({
  reason: z.enum(REJECT_REASONS),
  note: z.string().trim().min(10, 'Tell the customer what is missing (at least 10 characters).'),
})
export type RejectFormValues = z.infer<typeof rejectSchema>

/** DLG-04 Assign technician. */
export const assignSchema = z.object({
  technicianId: requiredId('Choose a technician.'),
})
export type AssignFormValues = z.infer<typeof assignSchema>

/** Cancel (DLG-12), decline (DLG-09), reopen (DLG-07): a reason of at least 10 characters. */
export const reasonSchema = z.object({
  reason: z.string().trim().min(10, 'At least 10 characters.').max(500),
})
export type ReasonFormValues = z.infer<typeof reasonSchema>

/** DLG-14 Rate the service. */
export const rateSchema = z.object({
  stars: z.number().int().min(1, 'Choose 1 to 5 stars.').max(5),
  comment: z.string().trim().max(500),
})
export type RateFormValues = z.infer<typeof rateSchema>
/** DLG-10 Put on hold: a fixed reason and the day it should continue (today or later). */
export const holdSchema = z.object({
  reason: z.enum(HOLD_REASONS),
  expectedResumeOn: z
    .string()
    .min(1, 'Choose the day you expect to continue.')
    .refine((d) => d >= new Date().toISOString().slice(0, 10), 'Choose today or a later day.'),
})
export type HoldFormValues = z.infer<typeof holdSchema>

/** One cost line in a form: amount typed as text, VND. Mirrors backend CostRules. */
export const costLineSchema = z.object({
  name: z.string().trim().min(2, 'At least 2 characters.').max(150),
  amount: z
    .string()
    .trim()
    .regex(/^\d{1,10}$/, 'A whole number of VND, e.g. 150000.'),
})
export type CostLineValues = z.infer<typeof costLineSchema>

/** Form rows -> request lines. */
export const toCostLines = (rows: CostLineValues[]) =>
  rows.map((r) => ({ name: r.name, description: null, amount: Number(r.amount) }))

/** DLG-11 Complete work + repair record. Hours left empty = the system measures start to finish. */
export const completeWorkSchema = z.object({
  parts: z.array(costLineSchema).max(20),
  rootCause: z.string().trim().min(3, 'Enter the cause.'),
  partsUsed: z.string().trim(),
  laborHours: z
    .string()
    .trim()
    .refine(
      (v) => v === '' || (Number(v) >= 0.25 && Number(v) <= 24),
      'Between 0.25 and 24 hours.',
    ),
  beforePhotos: z.array(z.string()).max(5),
  afterPhotos: z.array(z.string()).min(1, 'Add at least one photo after the repair.').max(5),
})
export type CompleteWorkFormValues = z.infer<typeof completeWorkSchema>

/** Reassign: another technician and why. */
export const reassignSchema = z.object({
  technicianId: requiredId('Choose a technician.'),
  reason: z.string().trim().min(10, 'At least 10 characters.').max(500),
})
export type ReassignFormValues = z.infer<typeof reassignSchema>

/** O08 change urgency. */
export const urgencySchema = z.object({
  urgency: z.enum(URGENCIES),
  reason: z.string().trim().min(10, 'At least 10 characters.').max(500),
})
export type UrgencyFormValues = z.infer<typeof urgencySchema>

/** O09 extend the deadline: a later time, a reason from the list, a note when "other". */
export const extendDueSchema = z
  .object({
    newDueAt: z.string().min(1, 'Choose the new deadline.'),
    reason: z.enum(EXTEND_REASONS),
    note: z.string().trim().max(300),
  })
  .refine((v) => v.reason !== 'other' || v.note.length > 0, { path: ['note'], message: 'Say why.' })
export type ExtendDueFormValues = z.infer<typeof extendDueSchema>

/** Staff corrects the cost lines before closing. */
export const costsSchema = z.object({ lines: z.array(costLineSchema).max(20) })
export type CostsFormValues = z.infer<typeof costsSchema>
