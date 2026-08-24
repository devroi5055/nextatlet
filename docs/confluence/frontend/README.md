# Frontend Overview

The NextAtlet frontend ([`apps/NextAtlet.Client`](https://github.com/devroi5055/nextatlet/blob/main/apps/NextAtlet.Client)) is a **Next.js 16 App Router** application that renders the marketing site, the onboarding wizard, and (eventually) the athlete dashboard/editor and public profile pages.

## Tech stack

| Concern | Choice | Notes |
|---------|--------|-------|
| Framework | **Next.js 16** + **React 19** | App Router; middleware entry is `src/proxy.ts` (renamed from `middleware.ts` in Next 16) |
| Auth | **@auth0/nextjs-auth0 v4** | Client instance in `src/lib/auth0.ts` |
| Server state | **TanStack React Query** | One `QueryClient`, config in `src/lib/react-query.ts` |
| Client state | **Zustand** | Exactly one store: notifications |
| Styling | **Tailwind CSS v4** | Design tokens in `src/styles/globals.css`; `tailwind.config.cjs` is a dead v3 leftover |
| i18n | **next-intl v4** | `en` (default) + `da`, messages in `messages/*.json` |
| UI primitives | **Radix UI** + a small custom kit | Under `src/components/ui/**` |
| Forms | **react-hook-form + Zod** | Via a `Form` render-prop wrapper |
| API types | generated `Api` class in `src/types/api.ts` | From the backend OpenAPI spec |

## Bulletproof-react heritage

The app was scaffolded from the [**bulletproof-react**](https://github.com/alan2207/bulletproof-react) template, which is why:

- Code is **feature-sliced**: `src/features/<feature>/{api,components,utils}`.
- There's a `src/testing/` harness with MSW, and Storybook + Playwright config.
- **Template leftovers remain and are inert:** MSW handlers and e2e tests for discussions/teams/users/comments (concepts NextAtlet doesn't have), a commented-out `login-form`, an orphaned duplicate `individual-sites` feature, and dead `/login` and `/register` pages. Don't be confused by them — they're not part of the real app.

## `src/` folder map

| Folder | What lives there |
|--------|------------------|
| `src/app/[locale]/` | All routes. There is **no root `app/layout.tsx`** — the locale layout is the de-facto root. |
| `src/features/` | Feature slices: `marketing`, `onboarding`, `individual-sites` (orphaned dup), `auth` (commented out) |
| `src/components/ui/` | The UI primitive kit (button, form, dialog, drawer, dropdown, notifications, spinner, …) |
| `src/components/{errors,layouts}/` | Error fallback + shared layouts |
| `src/lib/` | `auth0.ts`, `api-client.ts`, `react-query.ts`, and (dead) `authorization.ts` |
| `src/config/` | `env.ts` (Zod-validated env), `paths.ts` (route registry) |
| `src/i18n/` | next-intl `routing`, `request`, `navigation` |
| `src/hooks/` | `use-disclosure` (the only hook) |
| `src/styles/` | `globals.css` — the ~1165-line design-token system |
| `src/types/` | `api.ts` — the generated API client + DTOs |
| `src/testing/` | MSW mocks, test utils (mostly template leftovers) |
| `messages/` | `en.json`, `da.json` — 190 keys each, identical key sets |

## What actually works

- **Marketing landing page** — finished, section-registry driven (`landingSections`).
- **Onboarding wizard** — the only fully-working authenticated flow (self + guardian registration). See [Onboarding Flow](./onboarding-flow.md).
- **Auth** — Auth0 v4 login/logout/token flow and the `/app` decision gate. See [Authentication](./authentication.md).
- **Dashboard/editor** — a **stub** (placeholder cards, "coming soon").

## Read next

| Page | Topic |
|------|-------|
| [Routing & Layouts](./routing-and-layouts.md) | Every route, the layout tree, the auth gate |
| [Authentication](./authentication.md) | The Auth0 proxy + token story |
| [Onboarding Flow](./onboarding-flow.md) | Self vs. guardian registration, end to end |
| [Configuration](./configuration.md) | Env vars and config files |

## Run it

```bash
cd apps/NextAtlet.Client
corepack enable   # provides pnpm from the packageManager field (one-time)
pnpm install
pnpm dev   # http://localhost:3000  (/ redirects to /en)
```

See [Running the Application](../04-running-the-application.md) for the full setup including the `.env` template.
