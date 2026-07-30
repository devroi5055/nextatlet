import { queryOptions, useQuery } from '@tanstack/react-query';

import { api } from '@/lib/api-client';
import { QueryConfig } from '@/lib/react-query';
import { MeResponse } from '@/types/api';

/**
 * The profile-existence check behind the post-login decision gate (§3).
 * `GET /api/Me` returns whether the authenticated user already has a profile.
 */
export const getMe = async (): Promise<MeResponse> => {
  const response = await api.getMe();
  return response.data;
};

export const getMeQueryOptions = () =>
  queryOptions({
    queryKey: ['me'],
    queryFn: getMe,
  });

type UseMeOptions = {
  queryConfig?: QueryConfig<typeof getMeQueryOptions>;
};

export const useMe = ({ queryConfig }: UseMeOptions = {}) =>
  useQuery({
    ...getMeQueryOptions(),
    ...queryConfig,
  });

/**
 * True once the user has finished onboarding — either by registering their own
 * profile (`profileId`) or by becoming the guardian of at least one child
 * profile (`guardedProfileIds`). The guardian branch matters: a guardian who
 * registers a child has no own `profileId`, so keying only on that would trap
 * them in an onboarding redirect loop.
 *
 * NOTE: `MeResponse` exposes no "partially onboarded" signal, so we can only
 * distinguish has-profile from no-profile — the plan's "resume incomplete
 * onboarding" branch needs a backend `onboardingStatus` that does not exist
 * yet (see open decision #4).
 */
export const hasCompletedOnboarding = (me: MeResponse): boolean =>
  Boolean(me.profileId) || (me.guardedProfileIds?.length ?? 0) > 0;
