import { type Step } from '../types';

/**
 * The "from sign-up to sponsor" timeline. Text is resolved from the
 * `HowItWorks.steps.<key>` message namespace.
 */
export const steps: Step[] = [
  { key: 'profile', ordinal: '01' },
  { key: 'photoshoot', ordinal: '02' },
  { key: 'visible', ordinal: '03' },
  { key: 'sponsor', ordinal: '04' },
];
