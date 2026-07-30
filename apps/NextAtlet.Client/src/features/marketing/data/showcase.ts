import { type AthleteShowcase } from '../types';

/**
 * Sample athlete profile rendered in the hero and how-it-works cards. Proper
 * nouns (name, club, brand) stay here; localized labels (stat/badge names,
 * notification title/time) are resolved from the `Showcase` message namespace.
 */
export const featuredAthlete: AthleteShowcase = {
  name: 'Marcus Andersen',
  club: 'Ballerup Judoklub',
  sport: 'Judo',
  weightClass: '−60 kg',
  ageClass: 'U18',
  slug: 'marcus-andersen',
  nationalTeam: true,
  stats: [
    { value: '8×', key: 'gold' },
    { value: '14', key: 'tournaments' },
    { value: '3', key: 'silver' },
  ],
  badges: [
    { key: 'nationalTeam', variant: 'accent', icon: 'medal' },
    { key: 'judo', variant: 'neutral' },
    { key: 'u18Talent', variant: 'neutral' },
  ],
  notification: {
    brand: 'SportGear Danmark',
  },
};
