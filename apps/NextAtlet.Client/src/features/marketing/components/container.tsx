import { type ReactNode } from 'react';

import { cn } from '@/utils/cn';

export type ContainerProps = {
  children: ReactNode;
  className?: string;
};

/** Centres content and applies the shared marketing page gutters. */
export const Container = ({ children, className }: ContainerProps) => {
  return (
    <div className={cn('mx-auto w-full max-w-6xl px-6 lg:px-8', className)}>
      {children}
    </div>
  );
};
