import { Check, X } from 'lucide-react';
import NextLink from 'next/link';

import { Button } from '@/components/ui/button';
import { cn } from '@/utils/cn';

import { pricingTiers } from '../data/pricing';
import { type PricingTier } from '../types';

import { Section } from './section';
import { SectionHeading } from './section-heading';

const TierCard = ({ tier }: { tier: PricingTier }) => {
  return (
    <article
      className={cn(
        'relative flex flex-col rounded-2xl border p-8',
        tier.highlighted
          ? 'border-primary bg-card shadow-2xl shadow-black/40'
          : 'border-border bg-card',
      )}
    >
      {tier.badge && (
        <span className="absolute -top-3 right-6 rounded-full bg-primary px-3 py-1 text-[0.65rem] font-bold uppercase tracking-wider text-background">
          {tier.badge}
        </span>
      )}

      <h3 className="font-display text-xs font-bold uppercase tracking-[0.25em] text-primary-gold">
        {tier.name}
      </h3>
      <p className="mt-4 flex items-baseline gap-1">
        <span className="text-sm font-medium text-muted-foreground">kr</span>
        <span className="font-display text-4xl font-extrabold text-foreground">
          {tier.price}
        </span>
      </p>
      <p className="mt-1 text-xs text-muted-foreground">{tier.cadence}</p>

      <ul className="mt-6 flex-1 space-y-3">
        {tier.features.map((feature) => (
          <li
            key={feature.label}
            className={cn(
              'flex items-start gap-3 text-sm',
              feature.included ? 'text-foreground' : 'text-muted-foreground/50',
            )}
          >
            {feature.included ? (
              <Check className="mt-0.5 size-4 shrink-0 text-primary-gold" />
            ) : (
              <X className="mt-0.5 size-4 shrink-0 text-border" />
            )}
            {feature.label}
          </li>
        ))}
      </ul>

      <NextLink href={tier.cta.href} className="mt-8 block">
        <Button
          variant={tier.highlighted ? 'primary' : 'outline'}
          className="w-full"
        >
          {tier.cta.label}
        </Button>
      </NextLink>
    </article>
  );
};

/** Athlete subscription tiers. */
export const PricingSection = () => {
  return (
    <Section id="priser" className="bg-card">
      <SectionHeading
        eyebrow="Priser"
        title="Vælg dit niveau"
        description="Start gratis og opgrader, når du er klar til at tage din karriere til næste niveau."
      />
      <div className="mt-14 grid gap-6 lg:grid-cols-3">
        {pricingTiers.map((tier) => (
          <TierCard key={tier.id} tier={tier} />
        ))}
      </div>
    </Section>
  );
};
