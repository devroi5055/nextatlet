export const paths = {
  home: {
    getHref: () => '/',
  },

  // These map to the Auth0 SDK routes mounted by middleware.ts. There is no
  // separate `/auth/register` route — signup is the hosted login with
  // `screen_hint=signup`. `returnTo` is the Auth0 post-login redirect param.
  auth: {
    register: {
      getHref: (returnTo?: string | null | undefined) =>
        `/auth/login?screen_hint=signup${returnTo ? `&returnTo=${encodeURIComponent(returnTo)}` : ''}`,
    },
    login: {
      getHref: (returnTo?: string | null | undefined) =>
        `/auth/login${returnTo ? `?returnTo=${encodeURIComponent(returnTo)}` : ''}`,
    },
    logout: {
      getHref: () => '/auth/logout',
    },
  },

  onboarding: {
    root: {
      getHref: () => '/onboarding',
    },
    self: {
      getHref: () => '/onboarding/self',
    },
    guardian: {
      getHref: () => '/onboarding/guardian',
    },
    complete: {
      getHref: () => '/onboarding/complete',
    },
  },

  app: {
    root: {
      getHref: () => '/app',
    },
    dashboard: {
      getHref: () => '/app',
    },
    // The authenticated athlete's own site draft editor — the post-onboarding
    // landing where sections + themes will be edited (content TBD).
    editor: {
      getHref: () => '/app/editor',
    },
    discussions: {
      getHref: () => '/app/discussions',
    },
    discussion: {
      getHref: (id: string) => `/app/discussions/${id}`,
    },
    users: {
      getHref: () => '/app/users',
    },
    profile: {
      getHref: () => '/app/profile',
    },
  },
  public: {
    discussion: {
      getHref: (id: string) => `/public/discussions/${id}`,
    },
  },
} as const;
