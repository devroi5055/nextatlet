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
    <header className="sticky top-0 z-50 border border-border bg-secondary backdrop-blur">
      <Container className="flex h-16 items-center justify-between">
        <BrandWordmark />

        <nav className="hidden items-center gap-8 md:flex">
          {primaryNav.map((item) => (
            <NextLink
              key={item.label}
              href={item.href}
              className="text-sm font-medium text-muted-foreground transition-colors hover:text-foreground"
            >
              {item.label}
            </NextLink>
          ))}
        </nav>

        <div className="hidden md:flex items-center gap-4">
          <div className="seg" role="group" aria-label="Vælg sprog">
            <button className="active">DA</button>
            <button>EN</button>
          </div>

          <NextLink href={getStartedCta.href} onClick={mobileMenu.close}>
            <Button variant="ghost" className="w-full">
              Login
            </Button>
          </NextLink>

          <NextLink href={getStartedCta.href}>
            <Button variant="primary">
              {getStartedCta.label}
            </Button>
          </NextLink>
        </div>

        <button
          type="button"
          aria-label="Åbn menu"
          aria-expanded={mobileMenu.isOpen}
          onClick={mobileMenu.toggle}
          className="text-foreground md:hidden"
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
          'border-t border-border/60 bg-background md:hidden',
          mobileMenu.isOpen ? 'block' : 'hidden',
        )}
      >
        <Container className="flex flex-col gap-4 py-6">
          {primaryNav.map((item) => (
            <NextLink
              key={item.label}
              href={item.href}
              onClick={mobileMenu.close}
              className="text-sm font-medium text-muted-foreground transition-colors hover:text-foreground"
            >
              {item.label}
            </NextLink>
          ))}
          <NextLink href={getStartedCta.href} onClick={mobileMenu.close}>
            <Button variant="primary" className="w-full">
              {getStartedCta.label}
            </Button>
          </NextLink>
        </Container>
      </div>
    </header>
  );
};
