import { partners } from '../data/partners';

import { Container } from './container';

/** "Supported by" trust strip with partner logos / names. */
export const PartnersSection = () => {
  return (
    <section className="border-y border-brand-line/60 bg-brand-ink-soft py-10">
      <Container>
        <p className="text-center text-xs font-semibold uppercase tracking-[0.25em] text-brand-muted">
          Støttet af
        </p>
        <div className="mt-6 flex flex-wrap items-center justify-center gap-x-10 gap-y-4">
          {partners.map((partner) => (
            <span
              key={partner.name}
              className="font-display text-sm font-bold uppercase tracking-wider text-brand-muted/70 transition-colors hover:text-brand-cream"
            >
              {partner.name}
            </span>
          ))}
        </div>
      </Container>
    </section>
  );
};
