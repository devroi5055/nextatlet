import { auth0 } from '@/lib/auth0'; // your auth0 client instance

// getServerCookies moved to '@/lib/server-cookies' so the API client can
// import it lazily without dragging `next/headers` into client bundles.
export { getServerCookies } from '@/lib/server-cookies';

export const checkLoggedIn = async () => {
  const session = await auth0.getSession();
  return !!session;
};
