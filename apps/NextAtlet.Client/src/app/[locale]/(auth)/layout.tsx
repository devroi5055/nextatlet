import { getTranslations } from 'next-intl/server';
import { ReactNode, Suspense } from 'react';
import { ErrorBoundary } from 'react-error-boundary';

import { Spinner } from '@/components/ui/spinner';

import { AuthLayout as AuthLayoutComponent } from './_components/auth-layout';

export async function generateMetadata() {
  const t = await getTranslations('Metadata');
  return {
    title: t('authTitle'),
    description: t('authDescription'),
  };
}

const AuthLayout = async ({ children }: { children: ReactNode }) => {
  const t = await getTranslations('Errors');
  return (
    <Suspense
      fallback={
        <div className="flex size-full items-center justify-center">
          <Spinner size="xl" />
        </div>
      }
    >
      <ErrorBoundary fallback={<div>{t('somethingWrong')}</div>}>
        <AuthLayoutComponent>{children}</AuthLayoutComponent>
      </ErrorBoundary>
    </Suspense>
  );
};

export default AuthLayout;
