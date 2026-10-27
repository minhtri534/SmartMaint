import { useId, useState } from 'react'
import { cn } from '@/shared/utils/cn'

interface PhotoPickerProps {
  label: string
  value: string[]
  onChange: (photos: string[]) => void
  max?: number
  error?: string
  hint?: string
}

/**
 * Pick photos from the device and preview them. Photos are shrunk to max 960 px JPEG data URLs.
 * TODO(api): upload to Supabase Storage through the Attachment module and keep the returned ids instead.
 */
export function PhotoPicker({ label, value, onChange, max = 5, error, hint }: PhotoPickerProps) {
  const inputId = useId()
  const [reading, setReading] = useState(false)

  async function onFiles(files: FileList | null) {
    if (!files) return
    setReading(true)
    try {
      const room = max - value.length
      const picked = Array.from(files)
        .filter((f) => f.type.startsWith('image/'))
        .slice(0, room)
      const urls = await Promise.all(picked.map((f) => shrinkToDataUrl(f)))
      onChange([...value, ...urls])
    } finally {
      setReading(false)
    }
  }

  return (
    <div className="flex flex-col gap-1.5">
      <span className="text-sm font-medium">{label}</span>
      <div className="flex flex-wrap gap-2">
        {value.map((url, i) => (
          <div key={url.slice(-40) + i} className="relative">
            <img src={url} alt="" className="border-line size-20 rounded-md border object-cover" />
            <button
              type="button"
              onClick={() => onChange(value.filter((_, j) => j !== i))}
              className="bg-ink absolute -top-2 -right-2 size-5 rounded-full text-xs text-white"
              aria-label="Remove photo"
            >
              ×
            </button>
          </div>
        ))}
        {value.length < max && (
          <label
            htmlFor={inputId}
            className={cn(
              'text-muted hover:bg-canvas flex size-20 cursor-pointer flex-col items-center justify-center',
              'rounded-md border border-dashed text-xs',
              error ? 'border-danger' : 'border-line',
            )}
          >
            <span className="text-xl leading-none">+</span>
            {reading ? 'Reading…' : 'Add photo'}
          </label>
        )}
        <input
          id={inputId}
          type="file"
          accept="image/*"
          multiple
          className="sr-only"
          onChange={(e) => {
            void onFiles(e.target.files)
            e.target.value = ''
          }}
        />
      </div>
      {(error || hint) && (
        <p className={cn('text-xs', error ? 'text-danger' : 'text-muted')}>{error ?? hint}</p>
      )}
    </div>
  )
}

async function shrinkToDataUrl(file: File, maxSide = 960): Promise<string> {
  const bitmap = await createImageBitmap(file)
  const scale = Math.min(1, maxSide / Math.max(bitmap.width, bitmap.height))
  const canvas = document.createElement('canvas')
  canvas.width = Math.round(bitmap.width * scale)
  canvas.height = Math.round(bitmap.height * scale)
  canvas.getContext('2d')?.drawImage(bitmap, 0, 0, canvas.width, canvas.height)
  bitmap.close()
  return canvas.toDataURL('image/jpeg', 0.8)
}
