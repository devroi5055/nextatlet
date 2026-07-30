import {
  dehydrate,
  HydrationBoundary,
  QueryClient,
} from '@tanstack/react-query';
import { ReactNode } from 'react';

import { AppProvider } from '@/app/[locale]/provider';
import {NextIntlClientProvider, hasLocale} from 'next-intl';
import {getTranslations, setRequestLocale} from 'next-intl/server';
import {notFound} from 'next/navigation';
import {routing} from '@/i18n/routing';

// import { getUserQueryOptions } from '@/lib/auth';

import '@/styles/globals.css';

export async function generateMetadata() {
  const t = await getTranslations('Metadata');
  return {
    title: t('rootTitle'),
    description: t('rootDescription'),
  };
}

type Props = {
  children: React.ReactNode;
  params: Promise<{locale: string}>;
};

const RootLayout = async ({ children, params }: Props) => {
  const queryClient = new QueryClient();
  const {locale} = await params;
  if (!hasLocale(routing.locales, locale)) {
    notFound();
  }
  setRequestLocale(locale);

  // await queryClient.prefetchQuery(getUserQueryOptions());

  const dehydratedState = dehydrate(queryClient);

  return (
    <html lang={locale}>
      {/* Base defaults live on the element (not @layer base) so the old
          globals.css base rules can be removed without changing the UI. */}
      <body className="bg-background font-body text-foreground antialiased">
        {/* Intl provider wraps AppProvider so the app-level ErrorBoundary
            fallback can also resolve translations. */}
        <NextIntlClientProvider>
          <AppProvider>
            <HydrationBoundary state={dehydratedState}>
              {children}
            </HydrationBoundary>
          </AppProvider>
        </NextIntlClientProvider>
      </body>
    </html>
  );
};

export default RootLayout;

// We are not prerendering anything because the app is highly dynamic
// and the data depends on the user so we need to send cookies with each request
export const dynamic = 'force-dynamic';
