import { type AthleteShowcase, type Testimonial } from '../types';

/** Sample athlete profile rendered in the hero and how-it-works cards. */
export const featuredAthlete: AthleteShowcase = {
  name: 'Marcus Andersen',
  club: 'Ballerup Judoklub',
  sport: 'Judo',
  weightClass: '−60 kg',
  ageClass: 'U18',
  slug: 'marcus-andersen',
  nationalTeam: true,
  stats: [
    { value: '8×', label: 'Guld' },
    { value: '14', label: 'Stævner' },
    { value: '3', label: 'Sølv' },
  ],
  badges: [
    { label: 'Landshold', variant: 'accent', icon: 'medal' },
    { label: 'Judo', variant: 'neutral' },
    { label: 'U18 Talent', variant: 'neutral' },
  ],
  tags: ['Søger sponsor', 'Kamp', 'Coaching', 'Udstyr'],
  notification: {
    title: 'Ny sponsorforespørgsel',
    brand: 'SportGear Danmark',
    time: 'i dag',
  },
};

/** Pull-quote shown between the pricing and final call-to-action. */
export const testimonial: Testimonial = {
  quote:
    'Sponsorer leder ikke efter den bedste atlet. De leder efter den mest synlige atlet.',
  emphasis: 'mest synlige atlet',
  author: 'Thomas Bech, tidligere landsholdsjudoka & mentor hos NextAtlet',
};
