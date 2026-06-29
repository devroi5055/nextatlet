import { type LucideIcon } from 'lucide-react';

/** A single navigation entry in the marketing header / footer. */
export type NavItem = {
  label: string;
  href: string;
};

/** A grouped column of links in the footer. */
export type NavColumn = {
  heading: string;
  items: NavItem[];
};

/** One of the headline services shown in the "what we offer" grid. */
export type Offering = {
  /** Display ordinal, e.g. "01". */
  ordinal: string;
  icon: LucideIcon;
  title: string;
  description: string;
  link: NavItem;
};

/** A step in the "how it works" timeline. */
export type Step = {
  ordinal: string;
  title: string;
  description: string;
};

/** A logo/name shown in the "supported by" strip. */
export type Partner = {
  name: string;
};

/** A category tile in the photography gallery. */
export type GalleryItem = {
  title: string;
  caption: string;
  /** Featured tiles span a larger area in the grid. */
  featured?: boolean;
};

/** A single feature line inside a pricing tier. */
export type PricingFeature = {
  label: string;
  included: boolean;
};

/** A subscription tier in the pricing table. */
export type PricingTier = {
  id: string;
  name: string;
  /** Numeric amount rendered next to the currency prefix. */
  price: string;
  /** Sub-label under the price, e.g. "pr. måned · ingen binding". */
  cadence: string;
  features: PricingFeature[];
  cta: NavItem;
  /** Visually emphasises the tier and renders the badge. */
  highlighted?: boolean;
  badge?: string;
};

/** A pull-quote / testimonial. */
export type Testimonial = {
  quote: string;
  /** Portion of the quote rendered in the accent colour. */
  emphasis?: string;
  author: string;
};

/** A single statistic shown on the athlete showcase card. */
export type AthleteStat = {
  value: string;
  label: string;
};

/** The sample athlete profile rendered in the hero / how-it-works cards. */
export type AthleteShowcase = {
  name: string;
  club: string;
  weightClass: string;
  ageClass: string;
  slug: string;
  stats: AthleteStat[];
  tags: string[];
  notification?: {
    title: string;
    brand: string;
    time: string;
  };
};
