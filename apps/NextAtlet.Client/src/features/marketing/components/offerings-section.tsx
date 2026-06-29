import { ArrowRight } from 'lucide-react';
import NextLink from 'next/link';

import { offerings } from '../data/offerings';
import { type Offering } from '../types';

import { Section } from './section';
import { SectionHeading } from './section-heading';

const OfferingCard = ({ offering }: { offering: Offering }) => {
  const Icon = offering.icon;

  return (
    <article className="group flex flex-col rounded-2xl border border-brand-line bg-brand-ink-soft p-8 transition-colors hover:border-brand-gold/50">
      <span className="font-display text-3xl font-bold text-brand-line">
        {offering.ordinal}
      </span>
      <span className="mt-6 flex size-11 items-center justify-center rounded-xl bg-brand-gold/15 text-brand-gold">
        <Icon className="size-5" />
      </span>
      <h3 className="mt-6 font-display text-lg font-bold uppercase tracking-wide text-brand-cream">
        {offering.title}
      </h3>
      <p className="mt-3 flex-1 text-sm leading-relaxed text-brand-muted">
        {offering.description}
      </p>
      <NextLink
        href={offering.link.href}
        className="mt-6 inline-flex items-center gap-2 text-sm font-semibold text-brand-gold transition-colors hover:text-brand-gold-soft"
      >
        {offering.link.label}
        <ArrowRight className="size-4" />
      </NextLink>
    </article>
  );
};

/** "What we offer" — the three headline services. */
export const OfferingsSection = () => {
  return (
    <Section id="platform" className="bg-brand-ink">
      <SectionHeading
        eyebrow="Hvad vi tilbyder"
        title="Alt hvad en ung atlet behøver"
        description="Tre sammenhængende ydelser, der tilsammen bygger en troværdig og professionel digital identitet — fra første konkurrence til første sponsor."
      />
      <div className="mt-14 grid gap-6 md:grid-cols-3">
        {offerings.map((offering) => (
          <OfferingCard key={offering.ordinal} offering={offering} />
        ))}
      </div>
    </Section>
  );
};
