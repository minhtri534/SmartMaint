import { AnimatePresence, motion, useReducedMotion } from 'motion/react'
import {
  useEffect,
  useRef,
  useState,
  type FormEvent,
  type KeyboardEvent,
  type ReactNode,
} from 'react'
import { useLocation } from 'react-router'
import { useAuth } from '@/features/auth'
import type { Role } from '@/shared/types/role'
import { Icon, ProductMark, Spinner, type IconName } from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { EASE_SNAP } from '@/shared/utils/motion'
import { usePageAction } from '../actions'
import type { AssistantAction, AssistantCard, Turn } from '../types'
import { useAssistantChat } from '../useAssistantChat'
import { AnswerText } from './AnswerText'

/*
 * The assistant (Leader 16/11): a chat in the bottom-right corner of every page, for every role but Admin.
 * It explains and suggests from the user's own data; it never presses a button for them (the server gives the model
 * nothing outside the role's scope, and the rules say so too).
 * Phone: the panel takes the whole screen. Desktop: a 400px panel above the launcher, growing from its corner.
 */

type AssistantRole = Exclude<Role, 'admin'>

const NOUNS: Record<AssistantRole, string> = {
  customer: 'requests',
  staff: 'cases',
  technician: 'work orders',
  manager: 'buildings’ cases',
}

const STARTERS: Record<AssistantRole, string[]> = {
  customer: [
    'Where do my requests stand?',
    'How do I describe a problem so it gets fixed fast?',
    'What does “Completed” mean for my request?',
  ],
  staff: [
    'What should I handle first today?',
    'Which cases are overdue?',
    'Draft a polite reason to reject a duplicate request',
  ],
  technician: [
    'What is on my list today?',
    'How do I troubleshoot an air conditioner that is not cooling?',
    'When should I ask the staff for a third party?',
  ],
  manager: [
    'Summarise the open cases',
    'Which cases are overdue or emergencies?',
    'Do you see a pattern in recent requests?',
  ],
}

/** The case open on the screen, if the path is /cases/:id… */
const caseIdOf = (pathname: string) => {
  const match = /^\/cases\/(\d+)/.exec(pathname)
  return match ? Number(match[1]) : null
}

export function AssistantLauncher() {
  const { user } = useAuth()
  const [open, setOpen] = useState(false)
  const [everOpened, setEverOpened] = useState(false)
  const launcher = useRef<HTMLButtonElement>(null)
  const chat = useAssistantChat(everOpened)
  if (!user || user.role === 'admin') return null

  const toggle = () => {
    setEverOpened(true)
    setOpen((o) => !o)
  }
  const close = () => {
    setOpen(false)
    launcher.current?.focus()
  }

  return (
    <>
      <AnimatePresence>
        {open && (
          <ChatPanel
            role={user.role}
            firstName={user.fullName.split(' ').at(-1) ?? user.fullName}
            chat={chat}
            onClose={close}
          />
        )}
      </AnimatePresence>
      <button
        ref={launcher}
        type="button"
        onClick={toggle}
        aria-expanded={open}
        aria-controls="assistant-panel"
        aria-label={open ? 'Close the assistant' : 'Open the assistant'}
        className={cn(
          'bg-primary hover:bg-primary-hover ease-snap text-on-primary fixed right-4 bottom-4 z-40 flex h-12 items-center justify-center gap-2 rounded-full text-sm font-medium shadow-[0_10px_28px_-8px_rgb(15_27_45/0.5)] transition-[background-color,transform] duration-150 active:scale-[0.97] sm:right-5 sm:bottom-5 print:hidden',
          'w-12 sm:w-auto sm:pr-5 sm:pl-4',
          open && 'max-sm:hidden',
        )}
      >
        <span className="relative grid size-5 place-items-center">
          <AnimatePresence initial={false} mode="popLayout">
            <motion.span
              key={open ? 'close' : 'chat'}
              initial={{ opacity: 0, rotate: -45, scale: 0.6 }}
              animate={{ opacity: 1, rotate: 0, scale: 1 }}
              exit={{ opacity: 0, rotate: 45, scale: 0.6 }}
              transition={{ duration: 0.16, ease: EASE_SNAP }}
              className="grid place-items-center"
            >
              <Icon name={open ? 'close' : 'chat'} className="size-5" />
            </motion.span>
          </AnimatePresence>
        </span>
        <span className="hidden sm:inline">Assistant</span>
      </button>
    </>
  )
}

function ChatPanel({
  role,
  firstName,
  chat,
  onClose,
}: {
  role: AssistantRole
  firstName: string
  chat: ReturnType<typeof useAssistantChat>
  onClose: () => void
}) {
  const { pathname } = useLocation()
  const pageAction = usePageAction()
  const reduce = useReducedMotion()
  const [text, setText] = useState('')
  const input = useRef<HTMLTextAreaElement>(null)
  const log = useRef<HTMLDivElement>(null)
  const context = { caseId: caseIdOf(pathname), page: pathname }

  useEffect(() => input.current?.focus(), [])
  // Keep the newest turn in view (also while it is being answered).
  useEffect(() => {
    log.current?.scrollTo({ top: log.current.scrollHeight, behavior: reduce ? 'auto' : 'smooth' })
  }, [chat.turns, reduce])

  const ask = (question: string) => {
    const q = question.trim()
    if (!q || chat.pending) return
    setText('')
    void chat.send(q, context)
  }
  const onSubmit = (e: FormEvent) => {
    e.preventDefault()
    ask(text)
  }
  const onKeyDown = (e: KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey && !e.nativeEvent.isComposing) {
      e.preventDefault()
      ask(text)
    }
  }
  const runAction = (action: AssistantAction) => {
    if (!chat.pending) void chat.runAction(action)
  }
  // On a phone the panel covers the page: opening a case from an answer closes it.
  const onOpenCase = () => {
    if (window.matchMedia('(max-width: 639px)').matches) onClose()
  }

  return (
    <motion.section
      id="assistant-panel"
      role="dialog"
      aria-label="SmartMaint Assistant"
      onKeyDown={(e) => {
        if (e.key === 'Escape') onClose()
      }}
      initial={reduce ? { opacity: 0 } : { opacity: 0, scale: 0.96, y: 10 }}
      animate={{ opacity: 1, scale: 1, y: 0 }}
      exit={
        reduce ? { opacity: 0 } : { opacity: 0, scale: 0.98, y: 6, transition: { duration: 0.12 } }
      }
      transition={{ duration: 0.2, ease: EASE_SNAP }}
      style={{ transformOrigin: 'bottom right' }}
      className="bg-surface sm:border-border fixed inset-0 z-40 flex flex-col sm:inset-auto sm:right-5 sm:bottom-[84px] sm:h-[min(640px,calc(100vh-112px))] sm:w-[400px] sm:overflow-hidden sm:rounded-[16px] sm:border sm:shadow-[0_24px_64px_-16px_rgb(15_27_45/0.35),0_2px_6px_rgb(15_27_45/0.06)] print:hidden"
    >
      <header className="border-rule flex h-14 flex-none items-center gap-3 border-b pr-2 pl-4">
        <span className="bg-accent-soft grid size-8 flex-none place-items-center rounded-lg">
          <ProductMark className="size-4" />
        </span>
        <div className="min-w-0 flex-1">
          <h2 className="text-sm leading-5 font-semibold">SmartMaint Assistant</h2>
          <p className="text-muted truncate text-xs leading-4">
            Suggests only, never edits your data
          </p>
        </div>
        <HeaderButton
          icon="compose"
          label="New chat"
          onClick={() => {
            chat.reset()
            input.current?.focus()
          }}
          disabled={chat.turns.length === 0 || chat.pending}
        />
        <HeaderButton icon="close" label="Close" onClick={onClose} />
      </header>

      <div ref={log} role="log" aria-live="polite" className="flex-1 overflow-y-auto px-4 py-5">
        {chat.loading ? (
          <div className="grid h-full place-items-center">
            <Spinner />
          </div>
        ) : chat.turns.length === 0 ? (
          <Welcome
            role={role}
            firstName={firstName}
            pageAction={pageAction}
            onAsk={ask}
            onAction={runAction}
          />
        ) : (
          <ol className="flex flex-col gap-5">
            {chat.turns.map((turn) => (
              <TurnView
                key={turn.key}
                turn={turn}
                reduce={!!reduce}
                onRetry={() => chat.retry(turn.key, context)}
                onOpenCase={onOpenCase}
              />
            ))}
          </ol>
        )}
      </div>

      <form onSubmit={onSubmit} className="border-rule flex-none border-t px-3 pt-3 pb-2.5">
        {pageAction && chat.turns.length > 0 && (
          <button
            type="button"
            onClick={() => runAction(pageAction)}
            disabled={chat.pending}
            className="border-border hover:border-control hover:bg-raised text-accent-ink mb-2 inline-flex h-7 max-w-full items-center gap-1.5 rounded-full border px-3 text-xs font-medium transition-colors duration-150 disabled:opacity-50"
          >
            <Icon name="plus" className="size-3.5" />
            <span className="truncate">{pageAction.label}</span>
          </button>
        )}
        <div className="border-border focus-within:border-accent bg-surface flex items-end gap-2 rounded-xl border py-1.5 pr-1.5 pl-3 transition-[border-color,box-shadow] duration-150 focus-within:shadow-[0_0_0_3px_rgb(31_95_174/0.12)]">
          <textarea
            ref={input}
            rows={1}
            value={text}
            maxLength={1000}
            onChange={(e) => setText(e.target.value)}
            onKeyDown={onKeyDown}
            placeholder={`Ask about your ${NOUNS[role]}…`}
            aria-label="Message to the assistant"
            className="placeholder:text-faint field-sizing-content max-h-32 min-h-8 flex-1 resize-none bg-transparent py-1 text-sm leading-6 outline-none"
          />
          <button
            type="submit"
            disabled={!text.trim() || chat.pending}
            aria-label="Send"
            className="bg-primary hover:bg-primary-hover ease-snap text-on-primary grid size-8 flex-none place-items-center rounded-lg transition-[background-color,opacity,transform] duration-150 active:scale-95 disabled:opacity-30"
          >
            <Icon name="send" className="size-4" />
          </button>
        </div>
        <p className="text-faint mt-2 text-center text-[11px] leading-4">
          AI can be wrong. Check the case before you act.
        </p>
      </form>
    </motion.section>
  )
}

function HeaderButton({
  icon,
  label,
  onClick,
  disabled,
}: {
  icon: IconName
  label: string
  onClick: () => void
  disabled?: boolean
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      disabled={disabled}
      aria-label={label}
      title={label}
      className="text-muted hover:text-ink hover:bg-active grid size-9 place-items-center rounded-lg transition-colors duration-150 disabled:pointer-events-none disabled:opacity-35"
    >
      <Icon name={icon} className="size-[18px]" />
    </button>
  )
}

function Welcome({
  role,
  firstName,
  pageAction,
  onAsk,
  onAction,
}: {
  role: AssistantRole
  firstName: string
  pageAction: AssistantAction | null
  onAsk: (q: string) => void
  onAction: (a: AssistantAction) => void
}) {
  return (
    <div className="flex flex-col gap-5">
      <div>
        <p className="text-[17px] leading-6 font-semibold tracking-[-0.01em]">Hi {firstName},</p>
        <p className="text-muted mt-1.5 text-sm leading-[22px]">
          I can explain your {NOUNS[role]}, what a status means and what to do next. I only suggest:
          the buttons stay yours.
        </p>
      </div>
      <div className="flex flex-col gap-2">
        <p className="text-faint text-[11px] font-medium tracking-[0.06em] uppercase">Try asking</p>
        {pageAction && (
          <Starter onClick={() => onAction(pageAction)} accent>
            {pageAction.label}
          </Starter>
        )}
        {STARTERS[role].map((q) => (
          <Starter key={q} onClick={() => onAsk(q)}>
            {q}
          </Starter>
        ))}
      </div>
    </div>
  )
}

function Starter({
  children,
  onClick,
  accent,
}: {
  children: ReactNode
  onClick: () => void
  accent?: boolean
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={cn(
        'group ease-snap flex items-center justify-between gap-3 rounded-xl border px-3.5 py-2.5 text-left text-sm transition-[border-color,background-color,transform] duration-150 active:scale-[0.99]',
        accent
          ? 'border-accent/30 bg-accent-soft text-accent-ink hover:border-accent/60'
          : 'border-border hover:border-control hover:bg-raised',
      )}
    >
      <span>{children}</span>
      <Icon
        name="back"
        className="text-faint size-3.5 rotate-180 transition-transform duration-150 group-hover:translate-x-0.5"
      />
    </button>
  )
}

function TurnView({
  turn,
  reduce,
  onRetry,
  onOpenCase,
}: {
  turn: Turn
  reduce: boolean
  onRetry: () => void
  onOpenCase: () => void
}) {
  // A new turn rises 4px into place (feedback for the message just sent); stored turns appear as they are.
  const enter = turn.key.startsWith('l') && !reduce
  return (
    <motion.li
      initial={enter ? { opacity: 0, y: 4 } : false}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.18, ease: EASE_SNAP }}
      className="flex flex-col gap-2.5"
    >
      <div className="bg-primary text-on-primary ml-auto max-w-[85%] rounded-2xl rounded-br-md px-3.5 py-2 text-sm leading-[22px] whitespace-pre-wrap">
        {turn.question}
      </div>
      <div className="flex items-start gap-2.5">
        <span className="bg-accent-soft mt-0.5 grid size-6 flex-none place-items-center rounded-md">
          <ProductMark className="size-3" />
        </span>
        <div className="min-w-0 flex-1 text-sm leading-[22px]">
          {turn.state === 'pending' ? (
            <TypingDots />
          ) : turn.state === 'failed' ? (
            <div className="text-muted">
              <p>
                {turn.kind === 'card'
                  ? 'I could not read the form just now.'
                  : 'I could not answer just now. The assistant may be busy; try again in a minute.'}
              </p>
              {turn.kind === 'chat' && (
                <button
                  type="button"
                  onClick={onRetry}
                  className="text-accent-ink mt-1 text-sm font-medium hover:underline"
                >
                  Try again
                </button>
              )}
            </div>
          ) : turn.kind === 'card' ? (
            turn.card && <CardView card={turn.card} />
          ) : (
            <AnswerText text={turn.answer ?? ''} onOpenCase={onOpenCase} />
          )}
        </div>
      </div>
    </motion.li>
  )
}

function CardView({ card }: { card: AssistantCard }) {
  const [done, setDone] = useState(false)
  return (
    <div className="flex flex-col gap-2">
      <div className="border-border rounded-xl border px-3.5 py-3">
        <p className="font-medium">{card.title}</p>
        {card.detail && <p className="text-muted text-[13px]">{card.detail}</p>}
        {card.action && (
          <button
            type="button"
            disabled={done}
            onClick={() => {
              card.action?.run()
              setDone(true)
            }}
            className={cn(
              'ease-snap mt-2.5 inline-flex h-8 items-center gap-1.5 rounded-lg px-3 text-[13px] font-medium transition-[background-color,transform] duration-150 active:scale-[0.97]',
              done
                ? 'bg-raised text-positive'
                : 'bg-primary hover:bg-primary-hover text-on-primary',
            )}
          >
            {done ? card.action.doneLabel : card.action.label}
          </button>
        )}
      </div>
      {card.tips.length > 0 && (
        <ul className="text-muted flex list-disc flex-col gap-1 pl-4">
          {card.tips.map((tip) => (
            <li key={tip}>{tip}</li>
          ))}
        </ul>
      )}
      {card.note && <p className="text-faint text-xs">{card.note}</p>}
    </div>
  )
}

function TypingDots() {
  return (
    <span className="bg-raised inline-flex h-8 items-center gap-1 rounded-2xl rounded-tl-md px-3">
      <span className="sr-only">The assistant is writing</span>
      {[0, 160, 320].map((delay) => (
        <span
          key={delay}
          aria-hidden="true"
          className="bg-muted size-1.5 rounded-full"
          style={{ animation: `typing-dot 1.1s ${delay}ms ease-in-out infinite` }}
        />
      ))}
    </span>
  )
}
