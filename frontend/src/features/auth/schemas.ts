import { z } from 'zod'

/** Client-side checks mirror the backend validators (LoginRequestValidator, PasswordRules). */
export const loginSchema = z.object({
  email: z.email('Enter a valid email address.').max(254),
  password: z.string().min(1, 'Enter your password.'),
})

export type LoginFormValues = z.infer<typeof loginSchema>

// BR-18: >= 8 characters with an uppercase, a lowercase and a number.
const strongPassword = z
  .string()
  .min(8, 'At least 8 characters.')
  .max(128)
  .regex(/[A-Z]/, 'Must contain an uppercase letter.')
  .regex(/[a-z]/, 'Must contain a lowercase letter.')
  .regex(/[0-9]/, 'Must contain a number.')

export const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, 'Enter your current password.'),
    newPassword: strongPassword,
    confirmPassword: z.string(),
  })
  .refine((v) => v.newPassword === v.confirmPassword, {
    path: ['confirmPassword'],
    message: 'Passwords do not match.',
  })
  .refine((v) => v.newPassword !== v.currentPassword, {
    path: ['newPassword'],
    message: 'New password must differ from the current one.',
  })

export type ChangePasswordFormValues = z.infer<typeof changePasswordSchema>
