/**
 * Meeting note 18/11: an amount is typed with a space every three digits ("1 500 000") so it reads at a glance.
 * The form keeps that text; the request gets the number.
 */
export function groupDigits(value: string): string {
  const digits = value
    .replace(/\D/g, '')
    .replace(/^0+(?=\d)/, '')
    .slice(0, 10)
  return digits.replace(/\B(?=(\d{3})+(?!\d))/g, ' ')
}

/** "1 500 000" -> 1500000. */
export const amountOf = (value: string) => Number(value.replace(/\s/g, ''))

/** Whole VND, 1 to 10 digits once the spaces are gone. */
export const isAmount = (value: string) => /^\d{1,10}$/.test(value.replace(/\s/g, ''))
