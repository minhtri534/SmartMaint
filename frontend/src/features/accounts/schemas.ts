import { z } from 'zod'
import { ROLES } from '@/shared/types/role'
import type { RoleDetailsRequest } from './types'

/**
 * ADM-03 form (create / edit / change role). Mirrors backend AccountRequestValidators.
 * Inputs stay strings; toRoleDetails() turns them into the request shape.
 */
export const accountSchema = z
  .object({
    username: z
      .string()
      .trim()
      .min(3, '3 to 50 characters.')
      .max(50, '3 to 50 characters.')
      .regex(/^[A-Za-z0-9._-]+$/, 'Letters, digits, dot, dash and underscore only.'),
    fullName: z.string().trim().min(2, 'At least 2 characters.').max(60, 'At most 60 characters.'),
    email: z.union([z.literal(''), z.email('Enter a valid e-mail address.').max(254)]),
    phoneNumber: z
      .string()
      .trim()
      .regex(/^(\+?[0-9 ]{8,20})?$/, '8 to 20 digits, may start with +.'),
    role: z.enum(ROLES),
    /** Customer: building first (UI only), then the rented room. */
    buildingId: z.string(),
    roomId: z.string(),
    hourlyRate: z.string().regex(/^(\d{1,8}(\.\d{1,2})?)?$/, 'A number, e.g. 80000.'),
    specialtyIds: z.array(z.string()),
    buildingIds: z.array(z.string()),
    primaryBuildingIds: z.array(z.string()),
  })
  .refine((v) => v.role !== 'customer' || v.roomId !== '', {
    path: ['roomId'],
    message: "Choose the customer's room.",
  })

export type AccountFormValues = z.infer<typeof accountSchema>

export const emptyAccountForm: AccountFormValues = {
  username: '',
  fullName: '',
  email: '',
  phoneNumber: '',
  role: 'customer',
  buildingId: '',
  roomId: '',
  hourlyRate: '',
  specialtyIds: [],
  buildingIds: [],
  primaryBuildingIds: [],
}

export function toRoleDetails(v: AccountFormValues): RoleDetailsRequest {
  return {
    roomId: v.roomId ? Number(v.roomId) : null,
    hourlyRate: v.hourlyRate ? Number(v.hourlyRate) : null,
    specialtyIds: v.specialtyIds.map(Number),
    buildings: v.buildingIds.map((id) => ({
      buildingId: Number(id),
      isPrimary: v.role === 'staff' && v.primaryBuildingIds.includes(id),
    })),
  }
}

export const blank = (s: string) => (s.trim() === '' ? null : s.trim())
