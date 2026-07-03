import { useNotifications } from '@/components/ui/notifications';
import { env } from '@/config/env';
import { Api } from '@/types/api'; // the generated file

/**
 * Isomorphic, client-safe API client. In the browser, the session cookie is
 * attached automatically via `credentials: 'include'`. Server components that
 * need to call the API with the forwarded session use the dedicated server
 * helpers (e.g. features/onboarding/api/check-profile-server.ts) so this module
 * never imports `next/headers` and stays usable inside client components.
 */
const apiClient = new Api({
  baseUrl: env.API_URL,
  baseApiParams: {
    credentials: 'include',
  },
  customFetch: async (input, init) => {
    const response = await fetch(input, { ...init, credentials: 'include' });

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
