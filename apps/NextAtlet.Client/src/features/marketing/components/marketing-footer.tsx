import { useTranslations } from 'next-intl';
import NextLink from 'next/link';

import { footerColumns } from '../data/navigation';

import { BrandWordmark } from './brand-wordmark';
import { Container } from './container';

/** Site footer with brand blurb and link columns. */
export const MarketingFooter = () => {
  const t = useTranslations('Footer');
  const year = new Date().getFullYear();

  return (
    <footer className="border-t border-border/60 bg-background">
      <Container className="py-16">
        <div className="grid gap-10 lg:grid-cols-[1.5fr_repeat(3,1fr)]">
          <div className="max-w-xs">
            <BrandWordmark />
            <p className="mt-4 text-sm leading-relaxed text-muted-foreground">
              {t('blurb')}
            </p>
          </div>

          {footerColumns.map((column) => (
            <div key={column.key}>
              <h3 className="text-xs font-semibold uppercase tracking-[0.2em] text-foreground">
                {t(`columns.${column.key}.heading`)}
              </h3>
              <ul className="mt-4 space-y-3">
                {column.items.map((item) => (
                  <li key={item.key}>
                    <NextLink
                      href={item.href}
                      className="text-sm text-muted-foreground transition-colors hover:text-foreground"
                    >
                      {t(`columns.${column.key}.${item.key}`)}
                    </NextLink>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        <div className="mt-12 flex flex-col items-center justify-between gap-3 border-t border-border/60 pt-8 text-xs text-muted-foreground sm:flex-row">
          <p>{t('copyright', { year })}</p>
          <p className="uppercase tracking-[0.2em]">{t('tagline')}</p>
        </div>
      </Container>
    </footer>
  );
};
