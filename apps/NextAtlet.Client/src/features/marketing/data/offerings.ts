import { Camera, LayoutTemplate, Users } from 'lucide-react';

import { type Offering } from '../types';

/**
 * The three headline services. Add a fourth by appending here — the grid
 * and ordinals adapt automatically. Text is resolved from the
 * `Offerings.items.<key>` message namespace.
 */
export const offerings: Offering[] = [
  {
    key: 'website',
    ordinal: '01',
    icon: LayoutTemplate,
    href: '#priser',
  },
  {
    key: 'photography',
    ordinal: '02',
    icon: Camera,
    href: '#fotografi',
  },
  {
    key: 'mentoring',
    ordinal: '03',
    icon: Users,
    href: '#om-os',
  },
];
