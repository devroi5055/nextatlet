import { ArrowRight } from 'lucide-react';
import { useTranslations } from 'next-intl';
import NextLink from 'next/link';

import { Button } from '@/components/ui/button';
import { paths } from '@/config/paths';

import { Section } from './section';

/** Closing call-to-action before the footer. */
export const CtaSection = () => {
  const t = useTranslations('Cta');
  return (
    <Section
      id="om-os"
      className="bg-card"
      containerClassName="max-w-3xl text-center"
    >
      <p className="text-xs font-semibold uppercase tracking-[0.25em] text-primary-gold">
        {t('eyebrow')}
      </p>
      <h2 className="mt-5 font-display text-3xl font-extrabold uppercase leading-tight tracking-tight text-foreground sm:text-4xl lg:text-5xl">
        {t('titleLine1')}
        <br />
        <span className="text-primary-gold">{t('titleLine2')}</span>
      </h2>
      <p className="mx-auto mt-5 max-w-xl text-base leading-relaxed text-muted-foreground">
        {t('body')}
      </p>
      <div className="mt-8 flex flex-wrap items-center justify-center gap-4">
        {/* Plain anchor: /auth/login is an Auth0 middleware route, not a Next
            page, so it needs a full browser navigation (no RSC fetch). */}
        <a href={paths.auth.register.getHref(paths.onboarding.root.getHref())}>
          <Button variant="primary" size="lg">
            {t('primary')}
          </Button>
        </a>
        <NextLink href="#platform">
          <Button
            variant="ghost"
            size="lg"
            icon={<ArrowRight className="size-4" />}
          >
            {t('secondary')}
          </Button>
        </NextLink>
      </div>
    </Section>
  );
};
