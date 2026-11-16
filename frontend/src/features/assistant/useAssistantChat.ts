import { useQuery } from '@tanstack/react-query'
import { useRef, useState } from 'react'
import { assistantApi, assistantKeys } from './api'
import type { AssistantAction, ChatMessage, Turn } from './types'

const toTurn = (m: ChatMessage): Turn => ({
  kind: 'chat',
  key: `m${m.id}`,
  question: m.question,
  answer: m.answer,
  state: m.answer ? 'answered' : 'failed',
})

interface ChatState {
  turns: Turn[]
  conversationId: number | null
}

/**
 * The chat's state, kept in the app shell so closing the panel loses nothing. It starts from the latest conversation
 * on the server (loaded the first time the panel opens); new turns are added locally and answered by the API.
 */
export function useAssistantChat(enabled: boolean) {
  const thread = useQuery({
    queryKey: assistantKeys.thread,
    queryFn: assistantApi.thread,
    enabled,
    staleTime: Infinity,
  })
  const [local, setLocal] = useState<ChatState | null>(null)
  const nextKey = useRef(0)

  const state: ChatState = local ?? {
    turns: thread.data?.messages.map(toTurn) ?? [],
    conversationId: thread.data?.conversationId ?? null,
  }
  const pending = state.turns.some((t) => t.state === 'pending')

  const update = (change: (s: ChatState) => ChatState) => setLocal((prev) => change(prev ?? state))
  const patchTurn = (key: string, patch: Partial<Turn>) =>
    update((s) => ({
      ...s,
      turns: s.turns.map((t) => (t.key === key ? ({ ...t, ...patch } as Turn) : t)),
    }))

  async function send(question: string, context: { caseId: number | null; page: string }) {
    const key = `l${nextKey.current++}`
    const conversationId = state.conversationId
    update((s) => ({
      ...s,
      turns: [...s.turns, { kind: 'chat', key, question, answer: null, state: 'pending' }],
    }))
    try {
      const reply = await assistantApi.send({ message: question, conversationId, ...context })
      update((s) => ({
        conversationId: reply.conversationId ?? s.conversationId,
        turns: s.turns.map((t) =>
          t.key === key
            ? {
                ...t,
                answer: reply.message.answer,
                state: reply.available && reply.message.answer ? 'answered' : 'failed',
              }
            : t,
        ),
      }))
    } catch {
      // Network trouble, or the conversation is gone: the next question starts a fresh one.
      update((s) => ({
        conversationId: null,
        turns: s.turns.map((t) => (t.key === key ? { ...t, state: 'failed' } : t)),
      }))
    }
  }

  /** Ask again after a failed answer: the failed turn is replaced by the new attempt. */
  function retry(key: string, context: { caseId: number | null; page: string }) {
    const turn = state.turns.find((t) => t.key === key)
    if (!turn || turn.kind !== 'chat') return
    update((s) => ({ ...s, turns: s.turns.filter((t) => t.key !== key) }))
    void send(turn.question, context)
  }

  /** A page shortcut (e.g. New request's category suggestion): answered by the page, not by the chat API. */
  async function runAction(action: AssistantAction) {
    const key = `l${nextKey.current++}`
    update((s) => ({
      ...s,
      turns: [
        ...s.turns,
        { kind: 'card', key, question: action.label, card: null, state: 'pending' },
      ],
    }))
    try {
      const card = await action.run()
      patchTurn(key, { card, state: 'answered' })
    } catch {
      patchTurn(key, { state: 'failed' })
    }
  }

  /** "New chat": the next question starts a new conversation (the old one stays on the server for the audit). */
  const reset = () => setLocal({ turns: [], conversationId: null })

  return {
    turns: state.turns,
    loading: enabled && thread.isPending,
    pending,
    send,
    retry,
    runAction,
    reset,
  }
}
