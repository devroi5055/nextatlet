import { type ReactNode } from 'react';

import { cn } from '@/utils/cn';

import { Container } from './container';

export type SectionProps = {
  /** Anchor id, lets the header nav scroll to the section. */
  id?: string;
  children: ReactNode;
  className?: string;
  containerClassName?: string;
};

/**
 * Vertical rhythm + container wrapper shared by every landing section.
 * Keeps spacing consistent so new sections drop in without bespoke padding.
 */
export const Section = ({
  id,
  children,
  className,
  containerClassName,
}: SectionProps) => {
  return (
    <section
      id={id}
      className={cn('scroll-mt-20 py-20 lg:py-28', className)}
    >
      <Container className={containerClassName}>{children}</Container>
    </section>
  );
};
