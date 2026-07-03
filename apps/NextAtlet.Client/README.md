# NextAtlet — Web Client

Next.js (App Router) frontend for NextAtlet — the digital-presence & sponsorship platform for young judoka. It serves two surfaces: the public marketing/athlete pages and the authenticated editor/onboarding flow.

Originally scaffolded from [bulletproof-react](https://github.com/alan2207/bulletproof-react); some template features (`discussions`, `users`, `comments`, `teams`) are leftovers being removed as real features land.

## Stack

- **Next.js 16** (App Router) · React 19
- **Auth:** `@auth0/nextjs-auth0` — the `/auth/*` routes are mounted by `src/proxy.ts`; the post-login profile-existence gate lives in `src/app/app/layout.tsx` (calls `GET /api/Me`).
- **Server state:** TanStack React Query · **client state:** Zustand
- **Styling:** Tailwind CSS v4 — the design tokens live in `src/styles/globals.css` (`@theme`); use the semantic utilities (`bg-background`, `text-foreground`, `text-primary-gold`, `bg-card`, `border-border`, …), not raw colours.
- **Forms:** React Hook Form + Zod · **UI primitives:** Radix + `lucide-react`
- **API types:** generated from the backend's Swagger doc into `src/types/api.ts`, consumed via the typed client in `src/lib/api-client.ts`.

## Getting started

Prerequisites: **Node 20+**, and the backend API running (see `../../BACKEND_QUICK_START.md`).

```bash
cd apps/NextAtlet.Client
cp .env.example .env      # then fill in the values below
yarn install
yarn dev                  # http://localhost:3000
```

### Environment

Public (client) vars, read in `src/config/env.ts`:

- `NEXT_PUBLIC_API_URL` — backend base URL
- `NEXT_PUBLIC_URL` — this app's base URL
- `NEXT_PUBLIC_ENABLE_API_MOCKING`, `NEXT_PUBLIC_MOCK_API_PORT` — MSW mock server (optional)

Auth0 SDK vars (read directly by `@auth0/nextjs-auth0`): `AUTH0_DOMAIN`, `AUTH0_CLIENT_ID`, `AUTH0_CLIENT_SECRET`, `AUTH0_SECRET`, `APP_BASE_URL`.

## Scripts

| Script | What it does |
|--------|--------------|
| `yarn dev` / `build` / `start` | Next dev / production build / serve |
| `yarn check-types` | `tsc --noEmit` |
| `yarn test` | Vitest unit tests |
| `yarn test-e2e` | Playwright (starts the MSW mock server via pm2) |
| `yarn storybook` | Component workshop on :6006 |
| `yarn gen:api` | Regenerate `src/types/api.ts` from the backend Swagger doc (`gen:spec` → `gen:types`) |

> **Note on `gen:api`:** `gen:spec` reads `../NextAtlet.Api/bin/Debug/net10.0/NextAtlet.Api.dll`, so build the backend first. The committed `src/types/api.ts` was produced by **swagger-typescript-api** (it exports an `Api` class used by `api-client.ts`), but `gen:types` currently runs **openapi-typescript** — the two formats are incompatible. Reconcile the generator before running `gen:api`.

## Project structure

```
src/
  app/                 App Router routes (marketing /, onboarding/*, app/* editor, auth via proxy)
  features/            Feature modules (marketing, onboarding, …) — api/ + components/ per feature
  components/ui/       Shared UI primitives (button, form, dialog, …)
  lib/                 api-client, auth0, react-query, server-cookies
  config/              paths, env
  styles/globals.css   Tailwind v4 design tokens (@theme)
```
