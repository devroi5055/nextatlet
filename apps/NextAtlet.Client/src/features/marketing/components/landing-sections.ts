import { type ComponentType } from 'react';

import { CtaSection } from './cta-section';
import { HeroSection } from './hero-section';
import { HowItWorksSection } from './how-it-works-section';
import { OfferingsSection } from './offerings-section';
import { PartnersSection } from './partners-section';
import { PhotographySection } from './photography-section';
import { PricingSection } from './pricing-section';
import { TestimonialSection } from './testimonial-section';

/**
 * Ordered registry of landing-page sections (mirrors the backend's
 * config-as-data section model). Each section is self-contained and pulls its
 * own content from `../data`, so reordering or adding a section is a one-line
 * change here — no edits to the page composition. This is the page's primary
 * extension point.
 */
export const landingSections: ComponentType[] = [
  HeroSection,
  PartnersSection,
  OfferingsSection,
  HowItWorksSection,
  PhotographySection,
  PricingSection,
  TestimonialSection,
  CtaSection,
];
