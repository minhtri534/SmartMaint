import { useId, useState, type KeyboardEvent } from 'react'
import { Button, TextField } from '@/shared/ui'
import { formatAssetCode } from '@/shared/utils/asset-code'
import { cn } from '@/shared/utils/cn'
import type { AssetRef } from '../types'

interface AssetSearchProps {
  /** The assets of the chosen room; undefined while no room is chosen. */
  assets: AssetRef[] | undefined
  /** Selected asset id as a string ("" = none). */
  value: string
  onChange: (assetId: string) => void
  disabled?: boolean
}

/**
 * Meeting 07/11: type a word, the assets of the room whose name or code contain it drop down, click one to choose it.
 * A combobox (input + listbox): arrows move, Enter chooses, Escape closes.
 */
export function AssetSearch({ assets, value, onChange, disabled }: AssetSearchProps) {
  const listId = useId()
  const [query, setQuery] = useState('')
  const [open, setOpen] = useState(false)
  const [active, setActive] = useState(0)

  const selected = assets?.find((a) => String(a.id) === value)
  if (selected) {
    return (
      <div className="flex flex-col gap-2">
        <span className="text-sm font-medium">Asset</span>
        <div className="border-control rounded-control flex min-h-[38px] items-center justify-between gap-3 border px-3 py-1.5 text-sm">
          <span className="min-w-0">
            <span className="font-medium">{selected.name}</span>
            <span className="text-muted font-mono text-xs"> · {formatAssetCode(selected.id)}</span>
          </span>
          <Button variant="ghost" className="-mr-2" onClick={() => onChange('')}>
            Change
          </Button>
        </div>
      </div>
    )
  }

  const words = query.trim().toLowerCase()
  const matches = (assets ?? []).filter(
    (a) =>
      !words ||
      a.name.toLowerCase().includes(words) ||
      formatAssetCode(a.id).toLowerCase().includes(words),
  )
  const expanded = open && !disabled && assets !== undefined

  const choose = (a: AssetRef) => {
    onChange(String(a.id))
    setQuery('')
    setOpen(false)
  }

  const onKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'ArrowDown') {
      e.preventDefault()
      setOpen(true)
      setActive((i) => Math.min(i + 1, matches.length - 1))
    } else if (e.key === 'ArrowUp') {
      e.preventDefault()
      setActive((i) => Math.max(i - 1, 0))
    } else if (e.key === 'Enter' && expanded && matches[active]) {
      e.preventDefault()
      choose(matches[active])
    } else if (e.key === 'Escape') {
      setOpen(false)
    }
  }

  return (
    <div className="relative">
      <TextField
        label="Asset"
        placeholder={disabled ? 'Choose the room first' : 'Type a word, e.g. air conditioner'}
        hint={
          assets && assets.length === 0
            ? 'No registered asset in this room. Leave it empty.'
            : 'Optional. Leave it empty if the problem is not about one asset.'
        }
        disabled={disabled}
        autoComplete="off"
        role="combobox"
        aria-expanded={expanded}
        aria-controls={listId}
        aria-autocomplete="list"
        aria-activedescendant={expanded && matches[active] ? `${listId}-${active}` : undefined}
        value={query}
        onChange={(e) => {
          setQuery(e.target.value)
          setActive(0)
          setOpen(true)
        }}
        onFocus={() => setOpen(true)}
        onBlur={() => setOpen(false)}
        onKeyDown={onKeyDown}
      />
      {expanded && (
        <ul
          id={listId}
          role="listbox"
          className="border-border bg-surface rounded-control absolute top-[66px] right-0 left-0 z-10 max-h-60 overflow-y-auto border py-1 text-sm shadow-sm"
        >
          {matches.length === 0 ? (
            <li className="text-muted px-3 py-2">No asset in this room matches “{query}”.</li>
          ) : (
            matches.map((a, i) => (
              <li
                key={a.id}
                id={`${listId}-${i}`}
                role="option"
                aria-selected={i === active}
                // mousedown, not click: the input's blur would close the list first.
                onMouseDown={(e) => {
                  e.preventDefault()
                  choose(a)
                }}
                onMouseEnter={() => setActive(i)}
                className={cn(
                  'flex cursor-pointer items-baseline justify-between gap-3 px-3 py-2',
                  i === active && 'bg-raised',
                )}
              >
                <span className="min-w-0 truncate">{a.name}</span>
                <span className="text-muted flex-none font-mono text-xs">
                  {formatAssetCode(a.id)}
                </span>
              </li>
            ))
          )}
        </ul>
      )}
    </div>
  )
}
