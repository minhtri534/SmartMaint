import { useId, type ComponentProps } from 'react'
import { controlClass } from './control-class'
import { Field } from './Field'

interface TextAreaProps extends ComponentProps<'textarea'> {
  label: string
  error?: string
  hint?: string
}

/** Label + textarea + error message. Works with react-hook-form: <TextArea {...register('x')} />. */
export function TextArea({ label, error, hint, className, id, rows = 4, ...rest }: TextAreaProps) {
  const generatedId = useId()
  const inputId = id ?? generatedId

  return (
    <Field id={inputId} label={label} error={error} hint={hint} className={className}>
      <textarea
        id={inputId}
        rows={rows}
        aria-invalid={error ? true : undefined}
        aria-describedby={error || hint ? `${inputId}-message` : undefined}
        className={controlClass(error) + ' py-2 leading-5'}
        {...rest}
      />
    </Field>
  )
}
