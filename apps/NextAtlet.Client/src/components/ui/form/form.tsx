'use client';

import { zodResolver } from '@hookform/resolvers/zod';
import * as React from 'react';
import {
  FormProvider,
  Resolver,
  SubmitHandler,
  UseFormProps,
  UseFormReturn,
  useForm,
} from 'react-hook-form';
import { ZodType, z } from 'zod';

import { cn } from '@/utils/cn';

// Infer the form value type solely from the schema. Deriving it from `options`
// (e.g. partial defaultValues) would collapse the type to just those fields.
type FormProps<Schema extends ZodType<any, any, any>> = {
  onSubmit: SubmitHandler<z.infer<Schema>>;
  schema: Schema;
  className?: string;
  children: (methods: UseFormReturn<z.infer<Schema>>) => React.ReactNode;
  options?: UseFormProps<z.infer<Schema>>;
  id?: string;
};

export const Form = <Schema extends ZodType<any, any, any>>({
  onSubmit,
  children,
  className,
  options,
  id,
  schema,
}: FormProps<Schema>) => {
  const form = useForm<z.infer<Schema>>({
    ...options,
    // zod v4's resolver is typed over input/output; our schemas don't transform,
    // so input === output and this cast is safe.
    resolver: zodResolver(schema) as Resolver<z.infer<Schema>>,
  });
  return (
    <FormProvider {...form}>
      <form
        className={cn('space-y-6', className)}
        onSubmit={form.handleSubmit(onSubmit)}
        id={id}
      >
        {children(form)}
      </form>
    </FormProvider>
  );
};
