import { ArrowRight } from 'lucide-react';
import NextLink from 'next/link';

import { offerings } from '../data/offerings';
import { type Offering } from '../types';

import { Section } from './section';
import { SectionHeading } from './section-heading';

const OfferingCard = ({ offering }: { offering: Offering }) => {
  const Icon = offering.icon;

  return (
    <article className="group flex flex-col rounded-2xl border border-border bg-card p-8 transition-colors hover:border-primary-gold/50">
      <span className="font-display text-3xl font-bold text-border">
        {offering.ordinal}
      </span>
      <span className="mt-6 flex size-11 items-center justify-center rounded-xl bg-primary-gold/15 text-primary-gold">
        <Icon className="size-5" />
      </span>
      <h3 className="mt-6 font-display text-lg font-bold uppercase tracking-wide text-foreground">
        {offering.title}
      </h3>
      <p className="mt-3 flex-1 text-sm leading-relaxed text-muted-foreground">
        {offering.description}
      </p>
      <NextLink
        href={offering.link.href}
        className="mt-6 inline-flex items-center gap-2 text-sm font-semibold text-primary-gold transition-colors hover:text-gold-400"
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
    <Section id="platform" className="bg-background">
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
