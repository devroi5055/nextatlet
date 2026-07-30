import { Quote } from 'lucide-react';
import { useTranslations } from 'next-intl';
import { type ReactNode } from 'react';

import { Section } from './section';

/** Highlights the emphasised phrase within the quote in the accent colour. */
const renderQuote = (quote: string, emphasis?: string): ReactNode => {
  if (!emphasis || !quote.includes(emphasis)) return quote;
  const [before, after] = quote.split(emphasis);
  return (
    <>
      {before}
      <span className="text-primary-gold">{emphasis}</span>
      {after}
    </>
  );
};

/** Mentor pull-quote. */
export const TestimonialSection = () => {
  const t = useTranslations('Testimonial');
  return (
    <Section className="bg-background" containerClassName="max-w-3xl text-center">
      <Quote className="mx-auto size-8 text-primary-gold" />
      <blockquote className="mt-6 font-display text-2xl font-bold uppercase leading-snug tracking-tight text-foreground sm:text-3xl">
        {renderQuote(t('quote'), t('emphasis'))}
      </blockquote>
      <p className="mt-6 text-sm text-muted-foreground">— {t('author')}</p>
    </Section>
  );
};
