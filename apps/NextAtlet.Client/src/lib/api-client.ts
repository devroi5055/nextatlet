import { Api } from '@/types/api';   // the generated file
import { getServerCookies } from '@/utils/auth';
import { env } from '@/config/env';
import { useNotifications } from '@/components/ui/notifications';

const apiClient = new Api({
  baseUrl: env.API_URL,
  baseApiParams: {
    credentials: 'include',
  },
  customFetch: async (input, init) => {
    // SSR cookie forwarding — your existing logic
    const headers = new Headers(init?.headers);
    if (typeof window === 'undefined') {
      const cookie = await getServerCookies();
      if (cookie) headers.set('Cookie', cookie);
    }

    const response = await fetch(input, { ...init, headers, credentials: 'include' });

    // notification-on-error — your existing logic
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

export const api = apiClient.api;   // export the typed methods