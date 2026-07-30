import type { NextRequest } from 'next/server';

import createMiddleware from 'next-intl/middleware';

import { auth0 } from '@/lib/auth0';

import { routing } from './i18n/routing';

const intlMiddleware = createMiddleware(routing);

/**
 * Next.js 16 "proxy" (formerly middleware). Next only invokes ONE entry point
 * — the named `proxy` export — so both concerns must be composed here:
 *
 *   1. Auth0 owns its own routes (`/auth/login|logout|callback|profile|
 *      access-token`) and session-cookie refresh.
 *   2. next-intl handles locale negotiation + `[locale]` prefix routing for
 *      everything else. Without it running, `useLocale()` never sees the URL
 *      locale (stuck on the default) and `usePathname()` stops stripping the
 *      prefix (producing `/da/da`).
 *
 * Order (per next-intl + Auth0 guidance): localize first, then let Auth0
 * refresh the session and merge its Set-Cookie headers onto the localized
 * response. `/auth/*` is handed to Auth0 wholesale so it isn't localized.
 *
 * Route protection / the profile-existence decision gate lives in the `/app`
 * server layout (app/app/layout.tsx), not here.
 */
export async function proxy(request: NextRequest) {
    // Auth0 fully owns its endpoints — don't localize them.
    if (request.nextUrl.pathname.startsWith('/auth')) {
        return await auth0.middleware(request);
    }

    // 1. Localize (may redirect `/` -> `/en` or rewrite the locale prefix).
    const response = intlMiddleware(request);

    // 2. Refresh the Auth0 session and merge its cookies onto the intl response.
    const authResponse = await auth0.middleware(request);
    for (const cookie of authResponse.headers.getSetCookie()) {
        response.headers.append('set-cookie', cookie);
    }

    return response;
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
