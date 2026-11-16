import { Fragment, type ReactNode } from 'react'
import { Link } from 'react-router'
import { caseDetailPath } from '@/app/paths'

/*
 * The model answers in plain text with short "- " lines (system prompt rule 6). This draws that text: paragraphs,
 * bullet lists, **bold**, and every case code (CS-000123) as a link to the case. No markdown library needed.
 */

const INLINE = /(CS-\d{6}|\*\*[^*\n]+\*\*)/g
const BULLET = /^\s*(?:[-*•]|\d+[.)])\s+/

function inline(text: string, onOpenCase?: () => void): ReactNode[] {
  return text.split(INLINE).map((part, i) => {
    if (/^CS-\d{6}$/.test(part)) {
      return (
        <Link
          key={i}
          to={caseDetailPath(Number(part.slice(3)))}
          onClick={onOpenCase}
          className="text-accent-ink decoration-accent/40 hover:decoration-accent font-medium whitespace-nowrap tabular-nums underline underline-offset-2"
        >
          {part}
        </Link>
      )
    }
    if (part.startsWith('**') && part.endsWith('**') && part.length > 4) {
      return <strong key={i}>{part.slice(2, -2)}</strong>
    }
    return <Fragment key={i}>{part}</Fragment>
  })
}

export function AnswerText({ text, onOpenCase }: { text: string; onOpenCase?: () => void }) {
  const blocks: ReactNode[] = []
  let list: string[] = []
  const flush = () => {
    if (list.length === 0) return
    const items = list
    blocks.push(
      <ul key={blocks.length} className="marker:text-faint flex list-disc flex-col gap-1 pl-4">
        {items.map((item, i) => (
          <li key={i}>{inline(item, onOpenCase)}</li>
        ))}
      </ul>,
    )
    list = []
  }

  for (const line of text.split('\n')) {
    if (BULLET.test(line)) {
      list.push(line.replace(BULLET, ''))
    } else if (line.trim() === '') {
      flush()
    } else {
      flush()
      blocks.push(<p key={blocks.length}>{inline(line, onOpenCase)}</p>)
    }
  }
  flush()

  return <div className="flex flex-col gap-2">{blocks}</div>
}
