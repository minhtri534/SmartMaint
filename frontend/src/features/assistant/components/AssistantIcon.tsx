import { cn } from '@/shared/utils/cn'

/*
 * The assistant's icon (Leader 18/11): a speech bubble, because it is someone to ask. Drawing: "chat-teardrop-dots"
 * (duotone) from Phosphor Icons, MIT licence, Copyright (c) 2023 Phosphor Icons, https://phosphoricons.com
 * The three dots are drawn separately here so they can move like someone typing (keyframes in index.css).
 */
const DOTS = [
  { cx: 88, delay: '0ms' },
  { cx: 132, delay: '120ms' },
  { cx: 176, delay: '240ms' },
]

/** The assistant inside the chat (Leader 18/11): a small robot. "robot" (duotone), Phosphor Icons, same licence. */
export function AssistantBot({ className }: { className?: string }) {
  return (
    <svg
      viewBox="0 0 256 256"
      fill="currentColor"
      aria-hidden="true"
      className={cn('size-5 flex-none', className)}
    >
      <path
        opacity="0.2"
        d="M200,56H56A24,24,0,0,0,32,80V192a24,24,0,0,0,24,24H200a24,24,0,0,0,24-24V80A24,24,0,0,0,200,56ZM164,184H92a20,20,0,0,1,0-40h72a20,20,0,0,1,0,40Z"
      />
      <path d="M200,48H136V16a8,8,0,0,0-16,0V48H56A32,32,0,0,0,24,80V192a32,32,0,0,0,32,32H200a32,32,0,0,0,32-32V80A32,32,0,0,0,200,48Zm16,144a16,16,0,0,1-16,16H56a16,16,0,0,1-16-16V80A16,16,0,0,1,56,64H200a16,16,0,0,1,16,16ZM72,108a12,12,0,1,1,12,12A12,12,0,0,1,72,108Zm88,0a12,12,0,1,1,12,12A12,12,0,0,1,160,108Zm4,28H92a28,28,0,0,0,0,56h72a28,28,0,0,0,0-56Zm-24,16v24H116V152ZM80,164a12,12,0,0,1,12-12h8v24H92A12,12,0,0,1,80,164Zm84,12h-8V152h8a12,12,0,0,1,0,24Z" />
    </svg>
  )
}

export function AssistantIcon({
  className,
  animated = false,
}: {
  className?: string
  animated?: boolean
}) {
  return (
    <svg
      viewBox="0 0 256 256"
      fill="currentColor"
      aria-hidden="true"
      className={cn('size-5 flex-none', className)}
    >
      <path
        opacity="0.2"
        d="M224,124h0a92,92,0,0,1-92,92H48a8,8,0,0,1-8-8V124a92,92,0,0,1,92-92h0A92,92,0,0,1,224,124Z"
      />
      <path d="M132,24A100.11,100.11,0,0,0,32,124v84a16,16,0,0,0,16,16h84a100,100,0,0,0,0-200Zm0,184H48V124a84,84,0,1,1,84,84Z" />
      {DOTS.map((dot) => (
        <circle
          key={dot.cx}
          cx={dot.cx}
          cy="128"
          r="12"
          className={animated ? 'assistant-dot' : undefined}
          style={animated ? { animationDelay: dot.delay } : undefined}
        />
      ))}
    </svg>
  )
}
