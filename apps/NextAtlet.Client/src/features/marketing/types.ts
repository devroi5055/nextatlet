import { type LucideIcon } from 'lucide-react';

/**
 * A single navigation entry in the marketing header / footer. `key` resolves the
 * display label from the message catalog; `href` is the anchor/route target.
 */
export type NavItem = {
  key: string;
  href: string;
};

/** A grouped column of links in the footer. */
export type NavColumn = {
  key: string;
  items: NavItem[];
};

/**
 * One of the headline services shown in the "what we offer" grid. Text (title,
 * description, link label) is resolved from `Offerings.items.<key>`.
 */
export type Offering = {
  /** Message key under `Offerings.items`. */
  key: string;
  /** Display ordinal, e.g. "01". */
  ordinal: string;
  icon: LucideIcon;
  href: string;
};

/** A step in the "how it works" timeline. Text from `HowItWorks.steps.<key>`. */
export type Step = {
  key: string;
  ordinal: string;
};

/** A logo/name shown in the "supported by" strip (proper nouns, not localized). */
export type Partner = {
  name: string;
};

/** A category tile in the photography gallery. Text from `Photography.gallery.<key>`. */
export type GalleryItem = {
  key: string;
  /** Featured tiles span a larger area in the grid. */
  featured?: boolean;
};

/** A single feature line inside a pricing tier. */
export type PricingFeature = {
  included: boolean;
};

/**
 * A subscription tier in the pricing table. Names, cadence, badge, CTA label and
 * feature labels come from `Pricing.tiers.<id>`; only structure lives here.
 */
export type PricingTier = {
  id: string;
  /** Numeric amount rendered next to the currency prefix. */
  price: string;
  /** `included` flags parallel to `Pricing.tiers.<id>.features`. */
  features: PricingFeature[];
  href: string;
  /** Visually emphasises the tier and renders the badge. */
  highlighted?: boolean;
  /** Whether the tier renders a "most popular" badge. */
  hasBadge?: boolean;
};

/** A single statistic shown on the athlete showcase card. */
export type AthleteStat = {
  value: string;
  /** Message key under `Showcase.stats`. */
  key: string;
};

export interface AthleteBadge {
  /** Message key under `Showcase.badges`. */
  key: string;
  variant: 'accent' | 'solid' | 'neutral' | 'status' | 'warn' | 'info';
  icon?: 'flag' | 'medal' | 'shield' | 'star'; // maps to a lucide icon in the card
}

/** The sample athlete profile rendered in the hero / how-it-works cards. */
export interface AthleteShowcase {
  name: string;
  club: string;
  sport: string;
  weightClass: string;
  ageClass: string;
  slug: string;
  nationalTeam?: boolean;
  stats: AthleteStat[];
  badges?: AthleteBadge[];
  /** Notification brand is a proper noun; title/time are localized in the card. */
  notification: { brand: string };
}
