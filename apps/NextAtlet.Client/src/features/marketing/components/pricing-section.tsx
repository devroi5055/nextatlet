import { Check, X } from 'lucide-react';
import { useTranslations } from 'next-intl';

import { Button } from '@/components/ui/button';
import { cn } from '@/utils/cn';

import { pricingTiers } from '../data/pricing';
import { type PricingTier } from '../types';

import { Section } from './section';
import { SectionHeading } from './section-heading';

const TierCard = ({ tier }: { tier: PricingTier }) => {
  const t = useTranslations('Pricing');
  const tt = useTranslations(`Pricing.tiers.${tier.id}`);
  // Feature labels are localized; `included` flags run parallel in the tier data.
  const featureLabels = tt.raw('features') as string[];

  return (
    <article
      className={cn(
        'relative flex flex-col rounded-2xl border p-8',
        tier.highlighted
          ? 'border-primary bg-card shadow-2xl shadow-black/40'
          : 'border-border bg-card',
      )}
    >
      {tier.hasBadge && (
        <span className="absolute -top-3 right-6 rounded-full bg-primary px-3 py-1 text-[0.65rem] font-bold uppercase tracking-wider text-background">
          {tt('badge')}
        </span>
      )}

      <h3 className="font-display text-xs font-bold uppercase tracking-[0.25em] text-primary-gold">
        {tt('name')}
      </h3>
      <p className="mt-4 flex items-baseline gap-1">
        <span className="text-sm font-medium text-muted-foreground">
          {t('currency')}
        </span>
        <span className="font-display text-4xl font-extrabold text-foreground">
          {tier.price}
        </span>
      </p>
      <p className="mt-1 text-xs text-muted-foreground">{tt('cadence')}</p>

      <ul className="mt-6 flex-1 space-y-3">
        {tier.features.map((feature, index) => (
          <li
            key={index}
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
            {featureLabels[index]}
          </li>
        ))}
      </ul>

      {/* Plain anchor: /auth/login is an Auth0 middleware route, not a Next
          page, so it needs a full browser navigation (no RSC fetch). */}
      <a href={tier.href} className="mt-8 block">
        <Button
          variant={tier.highlighted ? 'primary' : 'outline'}
          className="w-full"
        >
          {tt('ctaLabel')}
        </Button>
      </a>
    </article>
  );
};

/** Athlete subscription tiers. */
export const PricingSection = () => {
  const t = useTranslations('Pricing');
  return (
    <Section id="priser" className="bg-card">
      <SectionHeading
        eyebrow={t('eyebrow')}
        title={t('title')}
        description={t('description')}
      />
      <div className="mt-14 grid gap-6 lg:grid-cols-3">
        {pricingTiers.map((tier) => (
          <TierCard key={tier.id} tier={tier} />
        ))}
      </div>
    </Section>
  );
};
