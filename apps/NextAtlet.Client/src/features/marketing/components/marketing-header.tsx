'use client';

import { Menu, X } from 'lucide-react';
import NextLink from 'next/link';

import { Button } from '@/components/ui/button';
import { useDisclosure } from '@/hooks/use-disclosure';
import { cn } from '@/utils/cn';

import { getStartedCta, primaryNav } from '../data/navigation';

import { BrandWordmark } from './brand-wordmark';
import { Container } from './container';

/** Sticky top navigation for the marketing site. */
export const MarketingHeader = () => {
  const mobileMenu = useDisclosure();

  return (
    <header className="sticky top-0 z-50 border-b border-brand-line/60 bg-brand-ink/80 backdrop-blur">
      <Container className="flex h-16 items-center justify-between">
        <BrandWordmark />

        <nav className="hidden items-center gap-8 md:flex">
          {primaryNav.map((item) => (
            <NextLink
              key={item.label}
              href={item.href}
              className="text-sm font-medium text-brand-muted transition-colors hover:text-brand-cream"
            >
              {item.label}
            </NextLink>
          ))}
        </nav>

        <div className="hidden md:block">
          <NextLink href={getStartedCta.href}>
            <Button variant="brand">{getStartedCta.label}</Button>
          </NextLink>
        </div>

        <button
          type="button"
          aria-label="Åbn menu"
          aria-expanded={mobileMenu.isOpen}
          onClick={mobileMenu.toggle}
          className="text-brand-cream md:hidden"
        >
          {mobileMenu.isOpen ? (
            <X className="size-6" />
          ) : (
            <Menu className="size-6" />
          )}
        </button>
      </Container>

      <div
        className={cn(
          'border-t border-brand-line/60 bg-brand-ink md:hidden',
          mobileMenu.isOpen ? 'block' : 'hidden',
        )}
      >
        <Container className="flex flex-col gap-4 py-6">
          {primaryNav.map((item) => (
            <NextLink
              key={item.label}
              href={item.href}
              onClick={mobileMenu.close}
              className="text-sm font-medium text-brand-muted transition-colors hover:text-brand-cream"
            >
              {item.label}
            </NextLink>
          ))}
          <NextLink href={getStartedCta.href} onClick={mobileMenu.close}>
            <Button variant="brand" className="w-full">
              {getStartedCta.label}
            </Button>
          </NextLink>
        </Container>
      </div>
    </header>
  );
};
