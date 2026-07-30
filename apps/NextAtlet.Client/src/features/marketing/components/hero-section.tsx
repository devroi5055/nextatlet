import { ArrowRight } from 'lucide-react';
import NextLink from 'next/link';

import { Button } from '@/components/ui/button';
import { paths } from '@/config/paths';

import { featuredAthlete } from '../data/showcase';

import { AthleteProfileCard } from './athlete-profile-card';
import { Container } from './container';

import {useTranslations} from 'next-intl';


/** Above-the-fold hero: value proposition + sample athlete card. */
export const HeroSection = () => {
  const t = useTranslations('HeroSection');
  return (
    <section className="relative overflow-hidden bg-background py-5 lg:pb-20">
      <div className="pointer-events-none absolute -right-40 top-0 size-128 rounded-full bg-primary/10 blur-3xl" />
      <Container className="relative grid gap-16 lg:grid-cols-2 grid-cols-1" >

        {/* LEFT */}
        <div className="grid justify-center justify-items-center lg:justify-items-start gap-6 lg:max-w-lg ">
          <p className="eyebrow">
            {t('eyebrow')}
          </p>

          <h1 className="display text-center lg:text-left text-balance-xs max-w-xs">
            {t("title.lead")}
            <span className="text-primary">
              {t("title.highlight")}
            </span>
          </h1>

          <p className="mt-6 max-w-xl text-base leading-relaxed text-muted-foreground sm:text-lg ">
            {t("subtitle")}
          </p>

          <div className="mt-8 grid grid-cols-1 sm:grid-cols-2 gap-4">
            {/* Plain anchor: /auth/login is an Auth0 middleware route, not a
                Next page, so it needs a full browser navigation (no RSC fetch). */}
            <a href={paths.auth.register.getHref(paths.onboarding.root.getHref())}>
              <Button variant="primary" size="lg">
                {t('ctaPrimary')}
              </Button>
            </a>

            <NextLink href="#priser">
              <Button
                variant="outline"
                size="lg"
                icon={<ArrowRight className="size-4" />}
              >
                {t('ctaSecondary')}
              </Button>
            </NextLink>
          </div>
        </div>

        {/* RIGHT */}
        <div className="flex justify-center">
          <AthleteProfileCard
            athlete={featuredAthlete}
            variant="browser"
            className="w-full sm:max-w-md md:max-w-md lg:max-w-xl h-fit pb-10"
          />
        </div>

      </Container>
    </section>
  );
};
