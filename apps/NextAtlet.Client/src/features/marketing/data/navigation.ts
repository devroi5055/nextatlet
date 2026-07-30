import { paths } from '@/config/paths';

import { type NavColumn, type NavItem } from '../types';

/**
 * Primary header navigation (anchors into the on-page sections). Labels are
 * resolved from the `Header.nav` message namespace by `key`, so the display
 * text is localized while the structure stays here.
 */
export const primaryNav: NavItem[] = [
  { key: 'platform', href: '#platform' },
  { key: 'photography', href: '#fotografi' },
  { key: 'pricing', href: '#priser' },
  { key: 'about', href: '#om-os' },
];

/** The header / hero call-to-action shared across the page. */
export const getStartedCta = {
  // returnTo onboarding: after Auth0 signup the user picks self/guardian, the
  // register endpoint runs, and success routes them to their own site editor.
  href: paths.auth.register.getHref(paths.onboarding.root.getHref()),
};

/**
 * Footer link columns. Each column and item carries a `key` resolved against the
 * `Footer.columns` message namespace; only the anchor targets live here.
 */
export const footerColumns: NavColumn[] = [
  {
    key: 'platform',
    items: [
      { key: 'athletes', href: '#platform' },
      { key: 'photography', href: '#fotografi' },
      { key: 'mentors', href: '#platform' },
      { key: 'pricing', href: '#priser' },
    ],
  },
  {
    key: 'sponsors',
    items: [
      { key: 'findTalent', href: '#om-os' },
      { key: 'howWeWork', href: '#om-os' },
      { key: 'contact', href: '#om-os' },
    ],
  },
  {
    key: 'about',
    items: [
      { key: 'mission', href: '#om-os' },
      { key: 'team', href: '#om-os' },
      { key: 'press', href: '#om-os' },
      { key: 'privacy', href: '#om-os' },
    ],
  },
];
