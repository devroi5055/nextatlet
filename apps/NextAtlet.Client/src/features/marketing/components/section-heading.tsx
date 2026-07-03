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
        <p className="eyebrow">
          {eyebrow}
        </p>
      )}
      <h2 className="h2">
        {title}
      </h2>
      {description && (
        <p className="muted">
          {description}
        </p>
      )}
    </div>
  );
};
