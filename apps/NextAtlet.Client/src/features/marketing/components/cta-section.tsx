import { ArrowRight } from 'lucide-react';
import NextLink from 'next/link';

import { Button } from '@/components/ui/button';
import { paths } from '@/config/paths';

import { Section } from './section';

/** Closing call-to-action before the footer. */
export const CtaSection = () => {
  return (
    <Section
      id="om-os"
      className="bg-brand-ink-soft"
      containerClassName="max-w-3xl text-center"
    >
      <p className="text-xs font-semibold uppercase tracking-[0.25em] text-brand-gold">
        Klar til at starte?
      </p>
      <h2 className="mt-5 font-display text-3xl font-extrabold uppercase leading-tight tracking-tight text-brand-cream sm:text-4xl lg:text-5xl">
        Byg din profil.
        <br />
        <span className="text-brand-gold">Skab din fremtid.</span>
      </h2>
      <p className="mx-auto mt-5 max-w-xl text-base leading-relaxed text-brand-muted">
        Opret din gratis profil på under 10 minutter og bliv synlig for
        sponsorer, medier og klubber over hele Danmark.
      </p>
      <div className="mt-8 flex flex-wrap items-center justify-center gap-4">
        <NextLink href={paths.auth.register.getHref()}>
          <Button variant="brand" size="lg">
            Opret gratis profil
          </Button>
        </NextLink>
        <NextLink href="#platform">
          <Button
            variant="brandGhost"
            size="lg"
            icon={<ArrowRight className="size-4" />}
          >
            Sådan virker det
          </Button>
        </NextLink>
      </div>
    </Section>
  );
};
