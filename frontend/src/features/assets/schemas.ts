import { z } from 'zod'
import type { AssetRequest } from './types'

/** AST-04 form. Mirrors backend AssetRequestValidator. */
export const assetSchema = z.object({
  name: z.string().trim().min(2, 'At least 2 characters.').max(150),
  buildingId: z.string().min(1, 'Choose a building.'),
  roomId: z.string().min(1, 'Choose the room of the asset.'),
  typeId: z.string().min(1, 'Choose the asset type.'),
  manufacturerId: z.string(),
  hasCopyright: z.boolean(),
  warrantyEndDate: z.string(),
})

export type AssetFormValues = z.infer<typeof assetSchema>

export const toAssetRequest = (v: AssetFormValues): AssetRequest => ({
  name: v.name,
  roomId: Number(v.roomId),
  typeId: Number(v.typeId),
  manufacturerId: v.manufacturerId ? Number(v.manufacturerId) : null,
  hasCopyright: v.hasCopyright,
  warrantyEndDate: v.warrantyEndDate || null,
})

/** F5 way 2: survey of an asset that fails often. */
export const surveySchema = z.object({
  serviceTypeId: z.string().min(1, 'Choose the service type.'),
  note: z.string().trim().max(2000),
})
export type SurveyFormValues = z.infer<typeof surveySchema>

/** A06: the new asset the Administrator registers in place of the old one. */
export const replacementSchema = z.object({
  name: z.string().trim().min(2, 'At least 2 characters.').max(150),
  typeId: z.string().min(1, 'Choose the asset type.'),
  manufacturerId: z.string(),
  hasCopyright: z.boolean(),
  warrantyEndDate: z.string(),
})
export type ReplacementFormValues = z.infer<typeof replacementSchema>

export const dismissReplacementSchema = z.object({
  reason: z.string().trim().min(10, 'At least 10 characters.').max(500),
})
export type DismissReplacementValues = z.infer<typeof dismissReplacementSchema>
