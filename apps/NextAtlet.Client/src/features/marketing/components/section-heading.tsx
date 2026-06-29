import { type ReactNode } from 'react';

import { cn } from '@/utils/cn';

export type SectionHeadingProps = {
  /** Small uppercase kicker above the title. */
  eyebrow?: string;
  title: ReactNode;
  description?: ReactNode;
  align?: 'left' | 'center';
  className?: string;
};

/** Consistent eyebrow + title + description block used by every section. */
export const SectionHeading = ({
  eyebrow,
  title,
  description,
  align = 'left',
  className,
}: SectionHeadingProps) => {
  const centered = align === 'center';

  return (
    <div
      className={cn(
        'max-w-2xl',
        centered && 'mx-auto text-center',
        className,
      )}
    >
      {eyebrow && (
        <p className="mb-4 flex items-center gap-3 text-xs font-semibold uppercase tracking-[0.25em] text-brand-gold">
          {!centered && <span className="h-px w-8 bg-brand-gold" />}
          {eyebrow}
        </p>
      )}
      <h2 className="font-display text-3xl font-extrabold uppercase leading-tight tracking-tight text-brand-cream sm:text-4xl lg:text-5xl">
        {title}
      </h2>
      {description && (
        <p className="mt-5 text-base leading-relaxed text-brand-muted sm:text-lg">
          {description}
        </p>
      )}
    </div>
  );
};
