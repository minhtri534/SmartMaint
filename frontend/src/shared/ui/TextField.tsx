import { useId, type ComponentProps } from 'react'
import { controlClass } from './control-class'
import { Field } from './Field'

interface TextFieldProps extends ComponentProps<'input'> {
  label: string
  error?: string
  hint?: string
}

/** Label + input + error message. Works with react-hook-form: <TextField {...register('email')} />. */
export function TextField({ label, error, hint, className, id, ...rest }: TextFieldProps) {
  const generatedId = useId()
  const inputId = id ?? generatedId

  return (
    <Field id={inputId} label={label} error={error} hint={hint} className={className}>
      <input
        id={inputId}
        aria-invalid={error ? true : undefined}
        aria-describedby={error || hint ? `${inputId}-message` : undefined}
        className={controlClass(error) + ' h-[38px]'}
        {...rest}
      />
    </Field>
  )
}
