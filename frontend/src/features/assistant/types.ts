// Types of the assistant chat (AiController). The AI only suggests: nothing here changes data.

/** One turn as the server keeps it: the question and the answer (null when the AI failed). */
export interface ChatMessage {
  id: number
  question: string
  answer: string | null
  sentAt: string
}

/** GET /api/ai/chat: the caller's latest conversation. */
export interface ChatThread {
  conversationId: number | null
  messages: ChatMessage[]
}

/** POST /api/ai/chat. conversationId null = start a new conversation. */
export interface ChatRequest {
  message: string
  conversationId: number | null
  /** The case open on the screen, if any; the server only reads it when the caller may see it. */
  caseId: number | null
  page: string
}

/** available = false: no answer this time (AI off, busy or failed). */
export interface ChatReply {
  available: boolean
  conversationId: number | null
  message: ChatMessage
}

/**
 * A suggestion a page hands to the chat (e.g. New request: the category from the description). The user applies it
 * with the button; nothing is sent or saved by the assistant itself.
 */
export interface AssistantCard {
  title: string
  detail?: string
  tips: string[]
  /** Small grey line under the card, e.g. "matched by keywords". */
  note?: string
  action?: { label: string; doneLabel: string; run: () => void }
}

/** A shortcut a page offers in the chat while it is open (registered with useAssistantAction). */
export interface AssistantAction {
  label: string
  run: () => Promise<AssistantCard>
}

/** What the chat shows: server turns plus local ones (pending questions, page cards). */
export type Turn =
  | {
      kind: 'chat'
      key: string
      question: string
      answer: string | null
      state: 'pending' | 'answered' | 'failed'
    }
  | {
      kind: 'card'
      key: string
      question: string
      card: AssistantCard | null
      state: 'pending' | 'answered' | 'failed'
    }
