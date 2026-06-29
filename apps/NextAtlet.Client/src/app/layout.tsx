import {
  dehydrate,
  HydrationBoundary,
  QueryClient,
} from '@tanstack/react-query';
import { ReactNode } from 'react';

import { AppProvider } from '@/app/provider';
// import { getUserQueryOptions } from '@/lib/auth';

import '@/styles/globals.css';

export const metadata = {
  title: 'NextAtlet — Din profil. Din karriere.',
  description:
    'NextAtlet hjælper unge judoudøvere med at bygge en stærk digital identitet — automatisk genererede hjemmesider, professionelle fotos og et mentornetværk, der åbner døre til sponsorater.',
};

const RootLayout = async ({ children }: { children: ReactNode }) => {
  const queryClient = new QueryClient();

  // await queryClient.prefetchQuery(getUserQueryOptions());

  const dehydratedState = dehydrate(queryClient);

  return (
    <html lang="da">
      <body>
        <AppProvider>
          <HydrationBoundary state={dehydratedState}>
            {children}
          </HydrationBoundary>
        </AppProvider>
      </body>
    </html>
  );
};

export default RootLayout;

// We are not prerendering anything because the app is highly dynamic
// and the data depends on the user so we need to send cookies with each request
export const dynamic = 'force-dynamic';
