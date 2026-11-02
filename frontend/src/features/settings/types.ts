/** Mirrors backend Modules/PriorityLevels/Dtos. ERD: PriorityLevels (+ NOT IN ERD v5: code, slaHours). */
export interface PriorityLevel {
  id: number
  code: 'emergency' | 'high' | 'normal' | 'low'
  name: string
  description: string | null
  /** Hours from the start of work to the due time. */
  slaHours: number
}
