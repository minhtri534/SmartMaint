/**
 * Asset display code, built from the id like the case code CS-000123 (no column, Leader 09/11): AS-000012.
 * Printed on the QR label so a customer can type it into the report form.
 */
export function formatAssetCode(id: number): string {
  return `AS-${String(id).padStart(6, '0')}`
}

/** "AS-000012", "as-12", "AS12" or "12" -> 12. Anything else -> null. */
export function parseAssetCode(text: string): number | null {
  const match = /^\s*(?:AS\s*-?\s*)?0*(\d{1,12})\s*$/i.exec(text)
  if (!match) return null
  const id = Number(match[1])
  return id > 0 ? id : null
}
