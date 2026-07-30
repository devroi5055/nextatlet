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

/** Resolves a validation message from the `Onboarding.validation` namespace. */
type Translate = (key: string) => string;

/**
 * Builds the self-registration schema with localized validation messages.
 * The message-agnostic shape is identical across locales, so the inferred
 * `SelfRegisterInput` type is stable regardless of which translator is passed.
 */
export const makeSelfRegisterInputSchema = (t: Translate) =>
  z
    .object({
      displayName: z.string().min(1, t('required')).max(80, t('max80')),
      slug: z
        .string()
        .min(3, t('min3'))
        .max(60, t('max60'))
        .regex(slugRegex, t('slugPattern')),
      // ISO date string (yyyy-mm-dd) from a native date input.
      dateOfBirth: z.string().min(1, t('required')),
      defaultLocaleId: z.enum(['da', 'en']),
      guardianEmail: z
        .string()
        .email(t('invalidEmail'))
        .max(254, t('emailTooLong'))
        .optional()
        .or(z.literal('')),
    })
    .superRefine((val, ctx) => {
      if (isBelowSelfRegisterFloor(val.dateOfBirth)) {
        ctx.addIssue({
          code: 'custom',
          path: ['dateOfBirth'],
          message: t('belowFloor'),
        });
      }
      // Below the self-consent age we need the guardian's email so the backend
      // can send them a consent request (the binding consent happens when they
      // confirm the emailed link — not from any checkbox here).
      if (requiresGuardianConsent(val.dateOfBirth) && !val.guardianEmail) {
        ctx.addIssue({
          code: 'custom',
          path: ['guardianEmail'],
          message: t('guardianRequired'),
        });
      }
    });

export type SelfRegisterInput = z.infer<
  ReturnType<typeof makeSelfRegisterInputSchema>
>;

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
