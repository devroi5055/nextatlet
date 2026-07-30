// Server-only by construction: imports `@/lib/auth0`, which pulls in
// `next/headers` and must never reach a client bundle.
import { env } from '@/config/env';
import { auth0 } from '@/lib/auth0';
import { MeResponse } from '@/types/api';

/**
 * Server-only profile-existence check for the decision gate (§3). Calls
 * `GET /api/Me` with the Auth0 access token as a Bearer header — the API's
 * "smart" scheme validates it via JWT. Kept out of the client api-client to
 * avoid pulling `next/headers` into the browser bundle.
 */
export const getMeServer = async (): Promise<MeResponse> => {
  const { token } = await auth0.getAccessToken();

  const response = await fetch(`${env.API_URL}/api/Me`, {
    headers: { Authorization: `Bearer ${token}` },
    cache: 'no-store',
  });
  if (!response.ok) {
    throw new Error(`GET /api/Me failed with ${response.status}`);
  }
  return (await response.json()) as MeResponse;
};
