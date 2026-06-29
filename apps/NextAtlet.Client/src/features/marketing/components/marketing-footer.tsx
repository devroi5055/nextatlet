import NextLink from 'next/link';

import { footerColumns } from '../data/navigation';

import { BrandWordmark } from './brand-wordmark';
import { Container } from './container';

/** Site footer with brand blurb and link columns. */
export const MarketingFooter = () => {
  return (
    <footer className="border-t border-brand-line/60 bg-brand-ink">
      <Container className="py-16">
        <div className="grid gap-10 lg:grid-cols-[1.5fr_repeat(3,1fr)]">
          <div className="max-w-xs">
            <BrandWordmark />
            <p className="mt-4 text-sm leading-relaxed text-brand-muted">
              Digital tilstedeværelse og sponsorplatform for unge judoudøvere i
              Danmark.
            </p>
          </div>

          {footerColumns.map((column) => (
            <div key={column.heading}>
              <h3 className="text-xs font-semibold uppercase tracking-[0.2em] text-brand-cream">
                {column.heading}
              </h3>
              <ul className="mt-4 space-y-3">
                {column.items.map((item) => (
                  <li key={item.label}>
                    <NextLink
                      href={item.href}
                      className="text-sm text-brand-muted transition-colors hover:text-brand-cream"
                    >
                      {item.label}
                    </NextLink>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        <div className="mt-12 flex flex-col items-center justify-between gap-3 border-t border-brand-line/60 pt-8 text-xs text-brand-muted sm:flex-row">
          <p>© 2025 NextAtlet. Alle rettigheder forbeholdes.</p>
          <p className="uppercase tracking-[0.2em]">Judo · Sport · Talent</p>
        </div>
      </Container>
    </footer>
  );
};
