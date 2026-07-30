import { Auth0Client } from '@auth0/nextjs-auth0/server';

const audience = process.env.AUTH0_AUDIENCE;

/**
 * Auth0 SDK client.
 *
 * When `AUTH0_AUDIENCE` is set, login additionally requests an API access token
 * (a JWT) for the backend's "bearer" scheme. Leave it UNSET until an Auth0 API
 * with that exact identifier exists in the tenant — otherwise Auth0 rejects the
 * `/authorize` request ("service not found") and login breaks entirely.
 *
 * With no audience, login works normally (ID token only) but backend API calls
 * will 401, since the backend has no valid JWT to validate.
 */
export const auth0 = new Auth0Client(
  audience
    ? {
        authorizationParameters: {
          audience,
          scope:
            process.env.AUTH0_SCOPE ?? 'openid profile email offline_access',
        },
      }
    : undefined,
);
