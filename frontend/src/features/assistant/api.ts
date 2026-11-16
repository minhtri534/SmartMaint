import { apiClient } from '@/lib/api-client'
import type { ChatReply, ChatRequest, ChatThread } from './types'

/** One function per endpoint of AiController (chat part). */
export const assistantApi = {
  thread: () => apiClient.get<ChatThread>('/ai/chat').then((r) => r.data),
  /** The model can take a few seconds (and one retry when Replicate throttles): allow 40 s. */
  send: (body: ChatRequest) =>
    apiClient.post<ChatReply>('/ai/chat', body, { timeout: 40_000 }).then((r) => r.data),
}

export const assistantKeys = {
  thread: ['assistant', 'thread'] as const,
}
