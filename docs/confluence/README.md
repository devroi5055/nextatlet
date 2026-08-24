# NextAtlet — Engineering Documentation

Welcome. This space is the onboarding home for engineers joining **NextAtlet**. It describes the system **as it actually is today**, not as it was originally planned. Where something is half-built, stubbed, or broken, we say so plainly — that honesty is the point of an onboarding doc.

> These pages were written from a full read of the source on the `main` branch of
> [`devroi5055/nextatlet`](https://github.com/devroi5055/nextatlet). Every file reference links to GitHub.

## What is NextAtlet?

NextAtlet is a platform where **judo athletes in Denmark build a public profile website** to present themselves and attract sponsors. Athletes (and their guardians, for minors) own a profile; clubs get their own organization pages. The backend stores each site as **structured data plus a theme** and never emits HTML — a Next.js frontend renders it. See the [Project Overview](./01-project-overview.md) for the full concept and business model.

## Start here

| Page | What you'll learn |
|------|-------------------|
| [1. Project Overview](./01-project-overview.md) | The concept, the tech stack, the moving parts, and what is built vs. not built |
| [2. Architecture](./02-architecture.md) | How a request flows from browser to database; the four backend layers; how frontend and backend talk |
| [3. Data Model & ER Diagram](./03-data-model-erd.md) | Every table, relationship, and index, with a visual ER diagram and commentary |
| [4. Running the Application](./04-running-the-application.md) | Prerequisites and exact commands to get backend + frontend running locally |

## Backend reference

| Page | What you'll learn |
|------|-------------------|
| [Backend: Commands & Queries](./backend/commands/README.md) | One page per backend command/query — what it does, its endpoint, errors, and gotchas |
| [Backend: Authentication & Tokens](./backend/authentication-and-tokens.md) | Auth0 dual-scheme auth, how claims are read, and the single-use ActionToken flow |
| [Backend: Configuration](./backend/configuration.md) | Every appsettings key, options class, and secret; how to configure the backend |

## Frontend reference

| Page | What you'll learn |
|------|-------------------|
| [Frontend: Overview](./frontend/README.md) | The Next.js app structure, tech stack, and folder map |
| [Frontend: Routing & Layouts](./frontend/routing-and-layouts.md) | Every route, the layout hierarchy, and the auth gate |
| [Frontend: Authentication](./frontend/authentication.md) | The Auth0 v4 proxy/middleware story and how tokens reach the API |
| [Frontend: Onboarding Flow](./frontend/onboarding-flow.md) | The self vs. guardian registration journey end to end |
| [Frontend: Configuration](./frontend/configuration.md) | Environment variables and every config file |

## A note on maturity

NextAtlet is an **early-stage codebase mid-refactor**. The identity, registration, and permission model is real and tested. The public render pipeline, billing/tiers, media pipeline, memberships, and change-request workflow are **designed but not built**. Several endpoints have **known security gaps** (documented on their pages) that must be closed before any production use. Read the "Known issues" and "Gotchas" sections — they are not filler.
