import { getTranslations } from 'next-intl/server';
import NextLink from 'next/link';
import { redirect } from 'next/navigation';
import { ReactNode } from 'react';

import { paths } from '@/config/paths';
import { auth0 } from '@/lib/auth0';

export async function generateMetadata() {
  const t = await getTranslations('Metadata');
  return {
    title: t('onboardingTitle'),
    description: t('onboardingDescription'),
  };
}

/**
 * Wizard shell. Both register endpoints require an authenticated caller (the
 * backend reads identity from the token, never the body), so an unauthenticated
 * visitor is sent to Auth0 first and returned here afterwards.
 */
const OnboardingLayout = async ({ children }: { children: ReactNode }) => {
  const session = await auth0.getSession();
  if (!session) {
    redirect(paths.auth.login.getHref(paths.onboarding.root.getHref()));
  }

  return (
    <div className="flex min-h-screen flex-col items-center bg-background px-4 py-12">
      <NextLink
        href={paths.home.getHref()}
        className="font-display text-lg font-extrabold uppercase tracking-[0.2em] text-foreground"
      >
        Next<span className="text-primary-gold">Atlet</span>
      </NextLink>
      <div className="mt-10 w-full max-w-md rounded-2xl bg-white p-8 shadow-2xl shadow-black/40">
        {children}
      </div>
    </div>
  );
};

export default OnboardingLayout;
