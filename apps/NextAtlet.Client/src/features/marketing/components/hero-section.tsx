import { ArrowRight } from 'lucide-react';
import NextLink from 'next/link';

import { Button } from '@/components/ui/button';
import { paths } from '@/config/paths';

import { featuredAthlete } from '../data/showcase';

import { AthleteProfileCard } from './athlete-profile-card';
import { Container } from './container';

/** Above-the-fold hero: value proposition + sample athlete card. */
export const HeroSection = () => {
  return (
    <section className="relative overflow-hidden bg-brand-ink py-20 lg:py-28">
      <div className="pointer-events-none absolute -right-40 top-0 size-[32rem] rounded-full bg-brand-gold/10 blur-3xl" />
      <Container className="relative grid items-center gap-12 lg:grid-cols-2 lg:gap-16">
        <div>
          <p className="mb-5 flex items-center gap-3 text-xs font-semibold uppercase tracking-[0.25em] text-brand-gold">
            <span className="h-px w-8 bg-brand-gold" />
            Digital tilstedeværelse for unge atleter
          </p>
          <h1 className="font-display text-4xl font-extrabold uppercase leading-[1.05] tracking-tight text-brand-cream sm:text-5xl lg:text-6xl">
            Din profil.
            <br />
            Din <span className="text-brand-gold">karriere.</span>
          </h1>
          <p className="mt-6 max-w-xl text-base leading-relaxed text-brand-muted sm:text-lg">
            NextAtlet hjælper unge judoudøvere med at bygge en stærk digital
            identitet — med automatisk genererede hjemmesider, professionelle
            fotos og et netværk af mentorer, der åbner døre til sponsorater.
          </p>
          <div className="mt-8 flex flex-wrap items-center gap-4">
            <NextLink href={paths.auth.register.getHref()}>
              <Button variant="brand" size="lg">
                Opret din profil
              </Button>
            </NextLink>
            <NextLink href="#platform">
              <Button
                variant="brandGhost"
                size="lg"
                icon={<ArrowRight className="size-4" />}
              >
                Se platformen
              </Button>
            </NextLink>
          </div>
        </div>

        <div className="lg:justify-self-end">
          <AthleteProfileCard
            athlete={featuredAthlete}
            variant="hero"
            className="mx-auto max-w-sm"
          />
        </div>
      </Container>
    </section>
  );
};
