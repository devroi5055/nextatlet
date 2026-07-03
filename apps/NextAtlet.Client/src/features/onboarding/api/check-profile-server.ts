// Server-only by construction: getServerCookies imports `next/headers`, which
// throws if this module is ever pulled into a client bundle.
import { env } from '@/config/env';
import { getServerCookies } from '@/lib/server-cookies';
import { MeResponse } from '@/types/api';

/**
 * Server-only profile-existence check for the decision gate (§3). Calls
 * `GET /api/Me` with the request's session cookie forwarded, so it can run in
 * the `/app` server layout. Kept out of the client api-client to avoid pulling
 * `next/headers` into the browser bundle.
 */
export const getMeServer = async (): Promise<MeResponse> => {
  const cookie = await getServerCookies();
  const response = await fetch(`${env.API_URL}/api/Me`, {
    headers: cookie ? { Cookie: cookie } : undefined,
    cache: 'no-store',
  });
  if (!response.ok) {
    throw new Error(`GET /api/Me failed with ${response.status}`);
  }
  return (await response.json()) as MeResponse;
};
