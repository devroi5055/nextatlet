'use client';

import { useUser } from '@auth0/nextjs-auth0/client';
import { useTranslations } from 'next-intl';

export const DashboardInfo = () => {
  const user = useUser();
  const t = useTranslations('Dashboard');
  const name = `${user.user?.firstName ?? ''} ${user.user?.lastName ?? ''}`.trim();

  return (
    <>
      <h1 className="text-xl">
        {t.rich('welcome', {
          name,
          b: (chunks) => <b>{chunks}</b>,
        })}
      </h1>
    </>
  );
};
