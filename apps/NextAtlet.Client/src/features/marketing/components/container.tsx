import { type ReactNode } from 'react';

import { cn } from '@/utils/cn';

export type ContainerProps = {
  children: ReactNode;
  className?: string;
};

/** Centres content and applies the shared marketing page gutters. */
export const Container = ({ children, className }: ContainerProps) => {
  return (
    <div className={cn('mx-auto w-full max-w-screen-7xl px-10 lg:px-20', className)}>
      {children}
    </div>
  );
};
