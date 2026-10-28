import { useId, type ComponentProps } from 'react'
import { controlClass } from './control-class'
import { Field } from './Field'

interface SelectProps extends ComponentProps<'select'> {
  label: string
  error?: string
  hint?: string
  /** Text of the empty first option. Omit to have no empty option. */
  placeholder?: string
}

/** Label + native select + error message. Works with react-hook-form: <Select {...register('x')} />. */
export function Select({
  label,
  error,
  hint,
  placeholder,
  className,
  id,
  children,
  ...rest
}: SelectProps) {
  const generatedId = useId()
  const selectId = id ?? generatedId

  return (
    <Field id={selectId} label={label} error={error} hint={hint} className={className}>
      <select
        id={selectId}
        aria-invalid={error ? true : undefined}
        aria-describedby={error || hint ? `${selectId}-message` : undefined}
        className={controlClass(error) + ' h-[38px] pr-8'}
        {...rest}
      >
        {placeholder !== undefined && <option value="">{placeholder}</option>}
        {children}
      </select>
    </Field>
  )
}
