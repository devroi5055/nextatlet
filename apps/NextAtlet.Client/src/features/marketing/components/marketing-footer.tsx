import NextLink from 'next/link';

import { footerColumns } from '../data/navigation';

import { BrandWordmark } from './brand-wordmark';
import { Container } from './container';

/** Site footer with brand blurb and link columns. */
export const MarketingFooter = () => {
  return (
    <footer className="border-t border-border/60 bg-background">
      <Container className="py-16">
        <div className="grid gap-10 lg:grid-cols-[1.5fr_repeat(3,1fr)]">
          <div className="max-w-xs">
            <BrandWordmark />
            <p className="mt-4 text-sm leading-relaxed text-muted-foreground">
              Digital tilstedeværelse og sponsorplatform for unge judoudøvere i
              Danmark.
            </p>
          </div>

          {footerColumns.map((column) => (
            <div key={column.heading}>
              <h3 className="text-xs font-semibold uppercase tracking-[0.2em] text-foreground">
                {column.heading}
              </h3>
              <ul className="mt-4 space-y-3">
                {column.items.map((item) => (
                  <li key={item.label}>
                    <NextLink
                      href={item.href}
                      className="text-sm text-muted-foreground transition-colors hover:text-foreground"
                    >
                      {item.label}
                    </NextLink>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        <div className="mt-12 flex flex-col items-center justify-between gap-3 border-t border-border/60 pt-8 text-xs text-muted-foreground sm:flex-row">
          <p>© 2025 NextAtlet. Alle rettigheder forbeholdes.</p>
          <p className="uppercase tracking-[0.2em]">Judo · Sport · Talent</p>
        </div>
      </Container>
    </footer>
  );
};
