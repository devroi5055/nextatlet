import { paths } from '@/config/paths';

import { type PricingTier } from '../types';

/**
 * Athlete self-tiers (B2C): Starter → Pro → Elite. Numbers are illustrative
 * (see CLAUDE.md "Athlete tiers — structure only, numbers TBD"). Add a tier or
 * feature line by editing this array; the pricing grid renders whatever it finds.
 */
export const pricingTiers: PricingTier[] = [
  {
    id: 'starter',
    name: 'Starter',
    price: '0',
    cadence: 'Gratis for evigt',
    cta: { label: 'Opret gratis profil', href: paths.auth.register.getHref() },
    features: [
      { label: 'Automatisk genereret profil', included: true },
      { label: 'Søgbar på nextatlet.dk', included: true },
      { label: 'Resultater & karrierestatus', included: true },
      { label: '1 profilbillede', included: true },
      { label: 'Eget domæne', included: false },
      { label: 'Professionel fotosession', included: false },
      { label: 'Mentornetværk', included: false },
      { label: 'Sponsormatching', included: false },
    ],
  },
  {
    id: 'pro',
    name: 'Pro',
    price: '149',
    cadence: 'pr. måned · ingen binding',
    highlighted: true,
    badge: 'Mest populær',
    cta: { label: 'Kom i gang', href: paths.auth.register.getHref() },
    features: [
      { label: 'Alt i Starter', included: true },
      { label: 'Eget domæne (marcus.nextatlet.dk)', included: true },
      { label: '1 professionel fotosession pr. år', included: true },
      { label: 'Tilpasset design & farver', included: true },
      { label: 'Mediekit til sponsorer (PDF)', included: true },
      { label: 'Sponsorhenvendelsesformular', included: true },
      { label: 'Dedikeret mentor', included: false },
      { label: 'Prioriteret synlighed', included: false },
    ],
  },
  {
    id: 'elite',
    name: 'Elite',
    price: '349',
    cadence: 'pr. måned · ingen binding',
    cta: { label: 'Start Elite', href: paths.auth.register.getHref() },
    features: [
      { label: 'Alt i Pro', included: true },
      { label: 'Eget domæne (marcus.dk)', included: true },
      { label: 'Ubegrænset fotosessioner', included: true },
      { label: 'Dedikeret mentor (1:1 rådgivning)', included: true },
      { label: 'Prioriteret synlighed for sponsorer', included: true },
      { label: 'Video highlights (op til 3 min.)', included: true },
      { label: 'Forhandlingshjælp til aftaler', included: true },
      { label: 'Netværksintroduktioner', included: true },
    ],
  },
];
