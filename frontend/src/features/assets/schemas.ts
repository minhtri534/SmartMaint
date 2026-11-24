import { z } from 'zod'

/** F5 way 2: survey of an asset that fails often. */
export const surveySchema = z.object({
  serviceTypeId: z.string().min(1, 'Choose the service type.'),
  note: z.string().trim().max(2000),
})
export type SurveyFormValues = z.infer<typeof surveySchema>
