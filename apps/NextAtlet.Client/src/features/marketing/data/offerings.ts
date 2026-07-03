import { Camera, LayoutTemplate, Users } from 'lucide-react';

import { type Offering } from '../types';

/**
 * The three headline services. Add a fourth by appending here — the grid
 * and ordinals adapt automatically.
 */
export const offerings: Offering[] = [
  {
    ordinal: '01',
    icon: LayoutTemplate,
    title: 'Atlethjemmeside',
    description:
      'En automatisk genereret hjemmeside, der viser din karriere, dine resultater og din fortælling. Vælg det niveau, der passer dig — fra simpel profil til fuldt tilpasset brand.',
    link: { label: 'Se pakker', href: '#priser' },
  },
  {
    ordinal: '02',
    icon: Camera,
    title: 'Professionelt fotografi',
    description:
      'Professionelle fotos til hjemmesiden — og til privatbrug. Vi fotograferer ved stævner eller i lejede studier. Du ejer billederne. De varer hele karrieren.',
    link: { label: 'Se eksempler', href: '#fotografi' },
  },
  {
    ordinal: '03',
    icon: Users,
    title: 'Mentornetværk',
    description:
      'Guides, sparring og et netværk af tidligere atleter, der kender vejen til sponsorer. Vi hjælper dig med at skrive den første henvendelse og stå stærkt i forhandlingen.',
    link: { label: 'Lær mere', href: '#om-os' },
  },
];
