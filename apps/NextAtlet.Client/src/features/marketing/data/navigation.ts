import { paths } from '@/config/paths';

import { type NavColumn, type NavItem } from '../types';

/** Primary header navigation (anchors into the on-page sections). */
export const primaryNav: NavItem[] = [
  { label: 'Platform', href: '#platform' },
  { label: 'Fotografi', href: '#fotografi' },
  { label: 'Priser', href: '#priser' },
  { label: 'Om os', href: '#om-os' },
];

/** The header / hero call-to-action shared across the page. */
export const getStartedCta: NavItem = {
  label: 'Kom i gang',
  // returnTo /app so the post-auth decision gate routes new users into onboarding.
  href: paths.auth.register.getHref(paths.app.dashboard.getHref()),
};

/** Footer link columns. */
export const footerColumns: NavColumn[] = [
  {
    heading: 'Platform',
    items: [
      { label: 'Atleter', href: '#platform' },
      { label: 'Fotografi', href: '#fotografi' },
      { label: 'Mentorer', href: '#platform' },
      { label: 'Priser', href: '#priser' },
    ],
  },
  {
    heading: 'For sponsorer',
    items: [
      { label: 'Find talenter', href: '#om-os' },
      { label: 'Sådan samarbejder vi', href: '#om-os' },
      { label: 'Kontakt', href: '#om-os' },
    ],
  },
  {
    heading: 'Om NextAtlet',
    items: [
      { label: 'Vores mission', href: '#om-os' },
      { label: 'Team', href: '#om-os' },
      { label: 'Presse', href: '#om-os' },
      { label: 'Privatlivspolitik', href: '#om-os' },
    ],
  },
];
