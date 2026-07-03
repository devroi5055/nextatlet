import { featuredAthlete } from '../data/showcase';
import { steps } from '../data/steps';
import { type Step } from '../types';

import { AthleteProfileCard } from './athlete-profile-card';
import { Section } from './section';
import { SectionHeading } from './section-heading';

const StepRow = ({ step }: { step: Step }) => (
  <li className="flex gap-5">
    <span className="font-display text-sm font-bold text-primary-gold">
      {step.ordinal}
    </span>
    <div>
      <h3 className="font-display text-base font-bold text-foreground">
        {step.title}
      </h3>
      <p className="mt-2 text-sm leading-relaxed text-muted-foreground">
        {step.description}
      </p>
    </div>
  </li>
);

/** "How it works" timeline alongside the live profile preview. */
export const HowItWorksSection = () => {
  return (
    <Section className="bg-card">
      <div className="grid gap-12 lg:grid-cols-2 lg:gap-16">
        <div>
          <SectionHeading
            eyebrow="Sådan virker det"
            title="Fra oprettelse til sponsor på få uger"
          />
          <ol className="mt-10 space-y-8">
            {steps.map((step) => (
              <StepRow key={step.ordinal} step={step} />
            ))}
          </ol>
        </div>

        <div className="lg:justify-self-end lg:self-center">
          <AthleteProfileCard
            athlete={featuredAthlete}
            variant="browser"
            className="mx-auto max-w-sm"
          />
        </div>
      </div>
    </Section>
  );
};
