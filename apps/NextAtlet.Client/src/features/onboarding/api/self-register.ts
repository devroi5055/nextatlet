import { useMutation, useQueryClient } from '@tanstack/react-query';
import { z } from 'zod';

import { api } from '@/lib/api-client';
import { MutationConfig } from '@/lib/react-query';
import { RegisterIndividualSiteSelfRequest, SiteResponse } from '@/types/api';

import {
  isBelowSelfRegisterFloor,
  requiresGuardianConsent,
} from '../utils/derive-minor';

import { getMeQueryOptions } from './check-profile';

const slugRegex = /^[a-z0-9]+(?:-[a-z0-9]+)*$/;

export const selfRegisterInputSchema = z
  .object({
    displayName: z.string().min(1, 'Påkrævet').max(80, 'Højst 80 tegn'),
    slug: z
      .string()
      .min(3, 'Mindst 3 tegn')
      .max(60, 'Højst 60 tegn')
      .regex(slugRegex, 'Kun små bogstaver, tal og bindestreger'),
    // ISO date string (yyyy-mm-dd) from a native date input.
    dateOfBirth: z.string().min(1, 'Påkrævet'),
    defaultLocaleId: z.enum(['da', 'en']),
    guardianEmail: z
      .string()
      .email('Ugyldig e-mail')
      .max(254, 'For lang')
      .optional()
      .or(z.literal('')),
  })
  .superRefine((val, ctx) => {
    if (isBelowSelfRegisterFloor(val.dateOfBirth)) {
      ctx.addIssue({
        code: 'custom',
        path: ['dateOfBirth'],
        message:
          'Du skal være mindst 13 år for at oprette din egen profil. Bed en forælder om at oprette den for dig.',
      });
    }
    // Below the self-consent age we need the guardian's email so the backend
    // can send them a consent request (the binding consent happens when they
    // confirm the emailed link — not from any checkbox here).
    if (requiresGuardianConsent(val.dateOfBirth) && !val.guardianEmail) {
      ctx.addIssue({
        code: 'custom',
        path: ['guardianEmail'],
        message:
          'En forælders e-mail er påkrævet for atleter under 16 — vi sender dem en anmodning om samtykke.',
      });
    }
  });

export type SelfRegisterInput = z.infer<typeof selfRegisterInputSchema>;

const toRequest = (
  input: SelfRegisterInput,
): RegisterIndividualSiteSelfRequest => ({
  displayName: input.displayName,
  slug: input.slug,
  dateOfBirth: new Date(input.dateOfBirth).toISOString(),
  defaultLocaleId: input.defaultLocaleId,
  guardianEmail: input.guardianEmail ? input.guardianEmail : null,
  // `parentalConsentConfirmed` is intentionally omitted: the backend command
  // ignores it, and compliant consent (email + terms version + timestamp) is
  // recorded server-side via the guardian consent-token flow. See plan §8.
});

export const selfRegister = async (
  input: SelfRegisterInput,
): Promise<SiteResponse> => {
  const response = await api.individualSitesSelfRegisterCreate(
    toRequest(input),
  );
  return response.data;
};

export const useSelfRegister = (
  config?: MutationConfig<typeof selfRegister>,
) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: selfRegister,
    ...config,
    onSuccess: (...args) => {
      queryClient.invalidateQueries({
        queryKey: getMeQueryOptions().queryKey,
      });
      config?.onSuccess?.(...args);
    },
  });
};
