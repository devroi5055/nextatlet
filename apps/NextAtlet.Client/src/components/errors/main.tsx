'use client';

import { useTranslations } from 'next-intl';

import { Button } from '../ui/button';

export const MainErrorFallback = () => {
  const t = useTranslations('Errors');
  return (
    <div
      className="flex h-screen w-screen flex-col items-center justify-center text-red-500"
      role="alert"
    >
      <h2 className="text-lg font-semibold">{t('generic')}</h2>
      <Button
        className="mt-4"
        onClick={() => window.location.assign(window.location.origin)}
      >
        {t('refresh')}
      </Button>
    </div>
  );
};
