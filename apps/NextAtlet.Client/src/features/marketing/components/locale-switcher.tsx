'use client';

import { useLocale, useTranslations } from 'next-intl';
import { useOptimistic, useTransition } from 'react';

import { usePathname, useRouter } from '@/i18n/navigation';
import { routing } from '@/i18n/routing';

/** Two-letter labels based on the English language name (not the ISO code). */
const LOCALE_LABELS: Record<string, string> = {
  en: 'EN', // English
  da: 'DA', // Danish
};

/**
 * Language toggle with an animated slider.
 *
 * A locale switch changes the top-most `[locale]` route segment, which remounts
 * the entire localized subtree — so the slider cannot animate by "surviving"
 * the navigation. Instead we move it optimistically on click: `useTransition`
 * keeps the current instance mounted while the next route loads, and
 * `useOptimistic` flips the active locale immediately so the CSS transition on
 * `.seg-slider` plays on the mounted element. When the new route commits, the
 * fresh switcher mounts already in the matching position, so there's no jump.
 */
export const LocaleSwitcher = () => {
  const activeLocale = useLocale();
  const t = useTranslations('Header');
  const pathname = usePathname();
  const router = useRouter();
  const [, startTransition] = useTransition();
  const [optimisticLocale, setOptimisticLocale] = useOptimistic(activeLocale);

  const switchTo = (locale: string) => {
    if (locale === optimisticLocale) return;
    startTransition(() => {
      setOptimisticLocale(locale);
      router.replace(pathname, { locale });
    });
  };

  const activeIndex = routing.locales.indexOf(
    optimisticLocale as (typeof routing.locales)[number],
  );

  return (
    <div className="seg" role="group" aria-label={t('localeSwitcher')}>
      <span
        className="seg-slider"
        aria-hidden
        style={{ transform: `translateX(${activeIndex * 100}%)` }}
      />
      {routing.locales.map((loc) => (
        <button
          key={loc}
          type="button"
          onClick={() => switchTo(loc)}
          aria-pressed={optimisticLocale === loc}
          className={`seg-item ${optimisticLocale === loc ? 'active' : ''}`}
        >
          {LOCALE_LABELS[loc] ?? loc.toUpperCase()}
        </button>
      ))}
    </div>
  );
};
