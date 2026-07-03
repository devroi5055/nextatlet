import { cookies } from 'next/headers';

/**
 * Server-only: serialises the incoming request cookies so the API client can
 * forward the session during SSR. Lives in its own module (not utils/auth) so
 * it can be dynamically imported from api-client without pulling `next/headers`
 * into the client bundle.
 */
export async function getServerCookies(): Promise<string> {
  const cookieStore = await cookies();
  return cookieStore
    .getAll()
    .map((c) => `${c.name}=${c.value}`)
    .join('; ');
}
