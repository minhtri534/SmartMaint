import { z } from 'zod'
import { isAmount, amountOf } from './amount'
import { EXTEND_REASONS, REJECT_REASONS, URGENCIES } from './types'

/*
 * Form rules of F3. Select boxes keep their value as a string ("" = nothing chosen);
 * pages convert to numbers when they call the API. The backend validators must say the same.
 */

const requiredId = (message: string) => z.string().min(1, message)

const isUrgency = (v: string) => (URGENCIES as readonly string[]).includes(v)

/**
 * EMP-02 New request. Every request is about one asset (Outscope 23); the fault categories offered depend on its type.
 * The customer chooses the urgency; the staff confirms it on approval.
 */
export const newRequestSchema = z.object({
  serviceTypeId: requiredId('Choose the fault category.'),
  urgency: z.string().refine(isUrgency, 'Choose how urgent it is.'),
  buildingId: requiredId('Choose a building.'),
  floorId: requiredId('Choose a floor.'),
  roomId: requiredId('Choose a room.'),
  assetId: requiredId('Choose the asset that has the problem.'),
  title: z.string().trim().min(5, 'At least 5 characters.').max(120, 'At most 120 characters.'),
  description: z
    .string()
    .trim()
    .min(15, 'Describe the problem in at least 15 characters.')
    .max(2000, 'At most 2000 characters.'),
  photos: z.array(z.string()).max(5, 'At most 5 photos.'),
})
export type NewRequestFormValues = z.infer<typeof newRequestSchema>

/** One task line of the work order form (id '' = new). */
export const taskLineSchema = z.object({
  id: z.string(),
  name: z.string().trim().min(2, 'At least 2 characters.').max(200, 'At most 200 characters.'),
  description: z.string().trim().max(500, 'At most 500 characters.'),
})
export type TaskLineValues = z.infer<typeof taskLineSchema>

/**
 * DLG-01 Approve and the staff's Edit = one work order form (meeting 07/11): the customer's request as the staff
 * corrects it, plus the planned window and the tasks. Approve names the technician; Edit adds the status change.
 * Date-times are the values of datetime-local inputs ('' = none).
 */
export const workOrderSchema = z
  .object({
    title: z.string().trim().min(5, 'At least 5 characters.').max(120, 'At most 120 characters.'),
    description: z
      .string()
      .trim()
      .min(15, 'At least 15 characters.')
      .max(2000, 'At most 2000 characters.'),
    serviceTypeId: requiredId('Choose the fault category.'),
    urgency: z.enum(URGENCIES),
    technicianId: z.string(),
    plannedStartAt: z.string(),
    plannedEndAt: z.string(),
    tasks: z.array(taskLineSchema).max(30, 'At most 30 tasks.'),
    statusChange: z.enum(['', 'continue', 'close', 'reopen']),
    reason: z.string().trim().max(500),
  })
  .refine((v) => !v.plannedStartAt || !v.plannedEndAt || v.plannedEndAt > v.plannedStartAt, {
    path: ['plannedEndAt'],
    message: 'The planned end must be after the planned start.',
  })
  .refine((v) => v.statusChange !== 'reopen' || v.reason.length >= 10, {
    path: ['reason'],
    message: 'Say what is not OK (at least 10 characters).',
  })
export type WorkOrderFormValues = z.infer<typeof workOrderSchema>

/** DLG-02 Reject. A duplicate names the case it repeats and what to hand over (meeting 07/11, replaces merge). */
export const rejectSchema = z
  .object({
    reason: z.enum(REJECT_REASONS),
    note: z.string().trim().min(10, 'Tell the customer why (at least 10 characters).'),
    duplicateOf: z.string(),
    addAsTask: z.boolean(),
    copyPhotos: z.boolean(),
    addFollower: z.boolean(),
  })
  .refine((v) => v.reason !== 'duplicate' || v.duplicateOf !== '', {
    path: ['duplicateOf'],
    message: 'Choose the case this request repeats.',
  })
export type RejectFormValues = z.infer<typeof rejectSchema>
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
/** One cost line in a form: amount typed as text, VND. Mirrors backend CostRules. */
export const costLineSchema = z.object({
  /** The line's id when the staff keeps an existing one ('' = new): its receipt stays. */
  id: z.string(),
  name: z.string().trim().min(2, 'At least 2 characters.').max(150),
  /** Meeting 07/11: each cost has a title, a description (e.g. the receipt number) and an amount. */
  description: z.string().trim().max(300, 'At most 300 characters.'),
  // typed with a space every three digits (meeting note 18/11)
  amount: z.string().trim().refine(isAmount, 'A whole number of VND, e.g. 150 000.'),
})
export type CostLineValues = z.infer<typeof costLineSchema>

/** Form rows -> request lines. */
export const toCostLines = (rows: CostLineValues[]) =>
  rows.map((r) => ({
    id: r.id ? Number(r.id) : null,
    name: r.name,
    description: r.description || null,
    amount: amountOf(r.amount),
  }))

/** O09 extend the deadline: a later time, a reason from the list, a note when "other". */
export const extendDueSchema = z
  .object({
    newDueAt: z.string().min(1, 'Choose the new deadline.'),
    reason: z.enum(EXTEND_REASONS),
    note: z.string().trim().max(300),
  })
  .refine((v) => v.reason !== 'other' || v.note.length > 0, { path: ['note'], message: 'Say why.' })
export type ExtendDueFormValues = z.infer<typeof extendDueSchema>

/**
 * TEC-05, meeting 07/11: one task's status (still to do is fine), a note, photos with captions. A failure needs a
 * note; on a work order it says what happens next (fixed on the spot, or a repair case).
 */
export const taskUpdateSchema = z
  .object({
    status: z.enum(['pending', 'pass', 'fail', 'not_applicable']),
    note: z.string().trim().max(1000, 'At most 1000 characters.'),
    photos: z.array(z.string()).max(5, 'At most 5 photos.'),
    captions: z.array(z.string().trim().max(200, 'At most 200 characters.')),
    failAction: z.enum(['fixed_on_site', 'create_case']),
    dangerous: z.boolean(),
  })
  .refine((v) => v.status !== 'fail' || v.note.length >= 5, {
    path: ['note'],
    message: 'Say what is wrong (at least 5 characters).',
  })
export type TaskUpdateFormValues = z.infer<typeof taskUpdateSchema>

/** Meeting 07/11: the technician types the total work hours (e.g. forgot to start the clock). */
export const logHoursSchema = z.object({
  hours: z
    .string()
    .trim()
    .refine((v) => Number(v) > 0 && Number(v) <= 200, 'More than 0 and at most 200 hours.'),
})
export type LogHoursFormValues = z.infer<typeof logHoursSchema>

/** Meeting 07/11: a cost the technician paid, with the receipt number and / or a photo of it. */
export const costSchema = z.object({
  name: z.string().trim().min(2, 'At least 2 characters.').max(150),
  description: z.string().trim().max(300, 'At most 300 characters.'),
  // typed with a space every three digits (meeting note 18/11)
  amount: z.string().trim().refine(isAmount, 'A whole number of VND, e.g. 150 000.'),
  receiptCode: z.string().trim().max(100, 'At most 100 characters.'),
  receiptPhoto: z.array(z.string()).max(1),
})
export type AddCostFormValues = z.infer<typeof costSchema>

/** Meeting 07/11: ask the staff for an outside party — what is wrong, why, photos. */
export const requestDecisionSchema = z.object({
  description: z.string().trim().min(10, 'At least 10 characters.').max(1000),
  reason: z.string().trim().min(5, 'At least 5 characters.').max(1000),
  photos: z.array(z.string()).min(1, 'Add at least one photo.').max(5),
})
export type RequestDecisionFormValues = z.infer<typeof requestDecisionSchema>

/** F4 emergency log line. */
export const emergencyLogSchema = z.object({
  kind: z.enum(['third_party', 'workaround', 'customer_update', 'other']),
  note: z.string().trim().min(5, 'At least 5 characters.').max(500),
})
export type EmergencyLogFormValues = z.infer<typeof emergencyLogSchema>

/** F4 step ⑫ post-incident report. */
export const incidentReportSchema = z.object({
  damage: z.string().trim().min(10, 'At least 10 characters.').max(1000),
  lessons: z.string().trim().min(10, 'At least 10 characters.').max(1000),
  followUp: z.enum(['none', 'preventive_plan', 'replacement_review']),
})
export type IncidentReportFormValues = z.infer<typeof incidentReportSchema>

/** The staff records the cost lines (meeting 07/11). */
export const costsSchema = z.object({ lines: z.array(costLineSchema).max(20) })
export type CostsFormValues = z.infer<typeof costsSchema>
