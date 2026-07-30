'use client';

import { Menu, X } from 'lucide-react';
import { useTranslations } from 'next-intl';
import NextLink from 'next/link';

import { Button } from '@/components/ui/button';
import { useDisclosure } from '@/hooks/use-disclosure';
import { cn } from '@/utils/cn';

import { getStartedCta, primaryNav } from '../data/navigation';

import { BrandWordmark } from './brand-wordmark';
import { Container } from './container';
import { LocaleSwitcher } from './locale-switcher';

/** Sticky top navigation for the marketing site. */
export const MarketingHeader = () => {
  const mobileMenu = useDisclosure();
  const t = useTranslations('Header');

  return (
    <header className="sticky top-0 z-50 border border-border bg-secondary backdrop-blur">
      <Container className="flex h-16 items-center justify-between">
        <BrandWordmark />

        <nav className="hidden items-center gap-8 md:flex">
          {primaryNav.map((item) => (
            <NextLink
              key={item.key}
              href={item.href}
              className="text-sm font-medium text-muted-foreground transition-colors hover:text-foreground"
            >
              {t(`nav.${item.key}`)}
            </NextLink>
          ))}
        </nav>

        <div className="hidden md:flex items-center gap-4">

          <LocaleSwitcher />

          {/* Plain anchor: /auth/login is an Auth0 middleware route, not a
              Next page, so it needs a full browser navigation (no RSC fetch). */}
          <a href={getStartedCta.href}>
            <Button variant="primary">
              {t('cta')}
            </Button>
          </a>
        </div>

        <button
          type="button"
          aria-label={t('openMenu')}
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
              key={item.key}
              href={item.href}
              onClick={mobileMenu.close}
              className="text-sm font-medium text-muted-foreground transition-colors hover:text-foreground"
            >
              {t(`nav.${item.key}`)}
            </NextLink>
          ))}
          <a href={getStartedCta.href} onClick={mobileMenu.close}>
            <Button variant="primary" className="w-full">
              {t('cta')}
            </Button>
          </a>
        </Container>
      </div>
    </header>
  );
};
