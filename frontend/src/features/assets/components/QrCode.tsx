import QRCode from 'qrcode'
import { useEffect, useState } from 'react'

/**
 * A QR code image for <paramref name="text"/> (AST-06). Drawn in the browser by the "qrcode" library: black on white,
 * a quiet zone of 1 module, error correction M so a scratched label still scans.
 */
export function QrCode({
  text,
  size = 160,
  label,
}: {
  text: string
  size?: number
  label: string
}) {
  const [src, setSrc] = useState<string | null>(null)

  useEffect(() => {
    let alive = true
    QRCode.toDataURL(text, { margin: 1, width: size * 2, errorCorrectionLevel: 'M' }).then(
      (url) => alive && setSrc(url),
      () => alive && setSrc(null),
    )
    return () => {
      alive = false
    }
  }, [text, size])

  return src ? (
    <img src={src} alt={label} width={size} height={size} className="block" />
  ) : (
    <div className="bg-raised" style={{ width: size, height: size }} aria-label="Loading QR code" />
  )
}
