import { ArrowRight, Camera } from 'lucide-react';
import NextLink from 'next/link';

import { cn } from '@/utils/cn';

import { galleryItems } from '../data/gallery';
import { type GalleryItem } from '../types';

import { Section } from './section';
import { SectionHeading } from './section-heading';

const GalleryTile = ({ item }: { item: GalleryItem }) => (
  <div
    className={cn(
      'group relative flex aspect-[4/3] flex-col justify-end overflow-hidden rounded-2xl border border-border bg-gradient-to-br from-muted to-background p-5',
      item.featured && 'sm:col-span-2 sm:row-span-2 sm:aspect-auto',
    )}
  >
    <Camera className="absolute right-5 top-5 size-6 text-primary-gold/30" />
    <div className="relative">
      <p className="font-display text-sm font-bold uppercase tracking-wide text-foreground">
        {item.title}
      </p>
      <p className="mt-1 text-xs uppercase tracking-widest text-muted-foreground">
        {item.caption}
      </p>
    </div>
  </div>
);

/** Photography gallery with a featured tile and supporting categories. */
export const PhotographySection = () => {
  return (
    <Section id="fotografi" className="bg-background">
      <SectionHeading
        eyebrow="Fotografi"
        title="Billeder der taler for dig"
        description="Professionelle fotos fra stævner og studioer. Du ejer dem — brug dem overalt."
      />
      <div className="mt-12 grid auto-rows-[minmax(0,1fr)] grid-cols-2 gap-4 lg:grid-cols-4">
        {galleryItems.map((item) => (
          <GalleryTile key={item.title} item={item} />
        ))}
      </div>
      <NextLink
        href="#priser"
        className="mt-8 inline-flex items-center gap-2 text-sm font-semibold text-primary-gold transition-colors hover:text-gold-400"
      >
        Se alle eksempler
        <ArrowRight className="size-4" />
      </NextLink>
    </Section>
  );
};
