import { z } from 'zod'

/** Mirrors backend FacilityRequestValidators. */
export const buildingSchema = z.object({
  name: z.string().trim().min(2, 'At least 2 characters.').max(100),
  address: z.string().trim().max(200),
})
export type BuildingFormValues = z.infer<typeof buildingSchema>

export const floorSchema = z.object({
  floorNumber: z
    .string()
    .trim()
    .min(1, 'Enter the floor, e.g. G, B1, 12.')
    .max(10)
    .regex(/^[A-Za-z0-9]+$/, 'Letters and digits only, e.g. G, B1, 12.'),
})
export type FloorFormValues = z.infer<typeof floorSchema>

export const roomSchema = z.object({
  roomNumber: z.string().trim().min(1, 'Enter the room number or name.').max(50),
  isCommonArea: z.boolean(),
})
export type RoomFormValues = z.infer<typeof roomSchema>
