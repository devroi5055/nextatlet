import type { NextRequest } from 'next/server';

import { auth0 } from '@/lib/auth0';

/**
 * Next.js 16 "proxy" (formerly middleware). Mounts the Auth0 SDK's auth routes
 * (`/auth/login`, `/auth/logout`, `/auth/callback`, `/auth/profile`,
 * `/auth/access-token`) and refreshes the session cookie on every matched
 * request. Without this, none of the `/auth/*` endpoints the registration flow
 * links to would exist.
 *
 * Route protection / the profile-existence decision gate lives in the `/app`
 * server layout (app/app/layout.tsx), not here — keeping the proxy to session
 * plumbing avoids a per-request API round-trip.
 */
export async function proxy(request: NextRequest) {
    return await auth0.middleware(request);
}

export const config = {
    matcher: [
        /*
         * Match all request paths except static assets and image optimisation
         * files, so the Auth0 proxy can intercept `/auth/*`.
         */
        '/((?!_next/static|_next/image|favicon.ico|.*\\.(?:svg|png|jpg|jpeg|gif|webp)$).*)',
    ],
};
