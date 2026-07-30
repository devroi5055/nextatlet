import { useTranslations } from 'next-intl';

import { partners } from '../data/partners';

import { Container } from './container';

/** "Supported by" trust strip with partner logos / names. */
export const PartnersSection = () => {
  const t = useTranslations('Partners');
  return (
    <section className="border-y border-border/60 bg-card py-10">
      <Container>
        <p className="text-center text-xs font-semibold uppercase tracking-[0.25em] text-muted-foreground">
          {t('supportedBy')}
        </p>
        <div className="mt-6 flex flex-wrap items-center justify-center gap-x-10 gap-y-4">
          {partners.map((partner) => (
            <span
              key={partner.name}
              className="font-display text-sm font-bold uppercase tracking-wider text-muted-foreground/70 transition-colors hover:text-foreground"
            >
              {partner.name}
            </span>
          ))}
        </div>
      </Container>
    </section>
  );
};
