import { z } from 'zod'

/** Mirrors backend CatalogAdminValidators. */
const name = (max: number) => z.string().trim().min(2, 'At least 2 characters.').max(max)
const code = z
  .string()
  .trim()
  .min(1, 'Enter a code.')
  .max(20)
  .regex(/^[A-Za-z0-9-]+$/, 'Letters, digits and dash only, e.g. AC-SPLIT.')
const description = z.string().trim().max(300)

export const caseCategorySchema = z.object({
  groupName: name(60),
  name: name(100),
  description,
  isEmergency: z.boolean(),
  defaultPriority: z.enum(['emergency', 'high', 'normal', 'low']),
  defaultCostBearer: z.enum(['building', 'customer']),
  photoRequired: z.boolean(),
})
export type CaseCategoryValues = z.infer<typeof caseCategorySchema>

export const assetCategorySchema = z.object({ code, categoryName: name(100), description })
export type AssetCategoryValues = z.infer<typeof assetCategorySchema>

export const assetTypeSchema = z.object({
  categoryId: z.string().min(1, 'Choose a category.'),
  code,
  name: name(100),
  description,
})
export type AssetTypeValues = z.infer<typeof assetTypeSchema>

export const manufacturerSchema = z.object({
  name: name(100),
  phoneNumber: z.string().trim().max(20),
  email: z.union([z.literal(''), z.email('Enter a valid e-mail address.').max(254)]),
})
export type ManufacturerValues = z.infer<typeof manufacturerSchema>

export const specialtySchema = z.object({ name: name(60) })
export type SpecialtyValues = z.infer<typeof specialtySchema>
