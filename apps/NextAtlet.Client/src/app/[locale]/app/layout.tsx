import { getTranslations } from 'next-intl/server';
import { redirect } from 'next/navigation';
import { ReactNode } from 'react';

import { paths } from '@/config/paths';
import { hasCompletedOnboarding } from '@/features/onboarding/api/check-profile';
import { getMeServer } from '@/features/onboarding/api/check-profile-server';
import { auth0 } from '@/lib/auth0';
import { MeResponse } from '@/types/api';

import { DashboardLayout } from './_components/dashboard-layout';

export async function generateMetadata() {
  const t = await getTranslations('Metadata');
  return {
    title: t('dashboardTitle'),
    description: t('dashboardDescription'),
  };
}

/**
 * Post-login decision gate (plan §3). Enforced on the `/app` boundary so a
 * user without a profile can never land on a broken dashboard:
 *   - no session              → Auth0 login (returns here after)
 *   - session, no profile     → /onboarding
 *   - session, has profile    → dashboard
 *
 * The profile check is best-effort: if it throws (e.g. the Next↔API auth-token
 * integration isn't wired yet) we let the dashboard load rather than trap the
 * user in a redirect. `redirect()` is called OUTSIDE the try — it signals via a
 * thrown control-flow error that must not be swallowed by the catch.
 */
const AppLayout = async ({ children }: { children: ReactNode }) => {
  const session = await auth0.getSession();
  if (!session) {
    redirect(paths.auth.login.getHref(paths.app.editor.getHref()));
  }

  let me: MeResponse | null = null;
  try {
    me = await getMeServer();
  } catch {
    me = null;
  }
  if (me && !hasCompletedOnboarding(me)) {
    redirect(paths.onboarding.root.getHref());
  }

  return <DashboardLayout>{children}</DashboardLayout>;
};

export default AppLayout;
