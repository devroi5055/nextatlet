import { useNotifications } from '@/components/ui/notifications';
import { env } from '@/config/env';
import { Api } from '@/types/api'; // the generated file

/**
 * Fetches the current Auth0 access token from the SDK's mounted
 * `/auth/access-token` route (same-origin on the Next app, so the session
 * cookie is sent). Returns null when unauthenticated. Browser-only — server
 * callers use `auth0.getAccessToken()` directly.
 */
const getBrowserAccessToken = async (): Promise<string | null> => {
  if (typeof window === 'undefined') return null;
  try {
    const res = await fetch('/auth/access-token', { credentials: 'include' });
    if (!res.ok) return null;
    const data = (await res.json()) as { token?: string };
    return data.token ?? null;
  } catch {
    return null;
  }
};

/**
 * Isomorphic, client-safe API client. Browser calls attach the Auth0 access
 * token as a Bearer header — the .NET API's "smart" scheme routes any request
 * with a Bearer token to its JWT validator. Server components that need to call
 * the API use the dedicated server helpers (e.g.
 * features/onboarding/api/check-profile-server.ts) so this module never imports
 * `next/headers` and stays usable inside client components.
 */
const apiClient = new Api({
  baseUrl: env.API_URL,
  baseApiParams: {
    credentials: 'include',
  },
  customFetch: async (input, init) => {
    const headers = new Headers(init?.headers);
    const token = await getBrowserAccessToken();
    if (token) {
      headers.set('Authorization', `Bearer ${token}`);
    }

    const response = await fetch(input, {
      ...init,
      headers,
      credentials: 'include',
    });

    // notification-on-error — client only
    if (!response.ok && typeof window !== 'undefined') {
      const body = await response.clone().json().catch(() => null);
      useNotifications.getState().addNotification({
        type: 'error',
        title: 'Error',
        message: body?.errorCode ?? response.statusText,
      });
    }

    return response;
  },
});

export const api = apiClient.api; // export the typed methods
