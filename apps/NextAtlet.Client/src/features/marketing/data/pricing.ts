import { paths } from '@/config/paths';

import { type PricingTier } from '../types';

/**
 * Athlete self-tiers (B2C): Starter → Pro → Elite. Numbers are illustrative
 * (see CLAUDE.md "Athlete tiers — structure only, numbers TBD"). Names, cadence,
 * badge, CTA labels and feature text are resolved from `Pricing.tiers.<id>`; the
 * `included` flags below run parallel to that tier's `features` array.
 */
export const pricingTiers: PricingTier[] = [
  {
    id: 'starter',
    price: '0',
    href: paths.auth.register.getHref(paths.onboarding.root.getHref()),
    features: [
      { included: true },
      { included: true },
      { included: true },
      { included: true },
      { included: false },
      { included: false },
      { included: false },
      { included: false },
    ],
  },
  {
    id: 'pro',
    price: '149',
    highlighted: true,
    hasBadge: true,
    href: paths.auth.register.getHref(paths.onboarding.root.getHref()),
    features: [
      { included: true },
      { included: true },
      { included: true },
      { included: true },
      { included: true },
      { included: true },
      { included: false },
      { included: false },
    ],
  },
  {
    id: 'elite',
    price: '349',
    href: paths.auth.register.getHref(paths.onboarding.root.getHref()),
    features: [
      { included: true },
      { included: true },
      { included: true },
      { included: true },
      { included: true },
      { included: true },
      { included: true },
      { included: true },
    ],
  },
];
