/** The full URL a QR label opens: this site + /qr/assets/{id} (AST-06 / AST-07). */
export const qrUrl = (path: string) => `${window.location.origin}${path}`
