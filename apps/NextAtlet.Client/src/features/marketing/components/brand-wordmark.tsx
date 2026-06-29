import NextLink from 'next/link';

import { paths } from '@/config/paths';
import { cn } from '@/utils/cn';

export type BrandWordmarkProps = {
  className?: string;
};

/** The "NEXTATLET" wordmark, linking home. */
export const BrandWordmark = ({ className }: BrandWordmarkProps) => {
  return (
    <NextLink
      href={paths.home.getHref()}
      className={cn(
        'font-display text-lg font-extrabold uppercase tracking-[0.2em] text-brand-cream',
        className,
      )}
    >
      Next<span className="text-brand-gold">Atlet</span>
    </NextLink>
  );
};
