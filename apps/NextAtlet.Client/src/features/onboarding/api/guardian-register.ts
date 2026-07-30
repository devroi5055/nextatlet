import { useMutation, useQueryClient } from '@tanstack/react-query';
import { z } from 'zod';

import { api } from '@/lib/api-client';
import { MutationConfig } from '@/lib/react-query';
import {
  RegisterIndividualSiteGuardianRequest,
  SiteResponse,
} from '@/types/api';

import { isAdult } from '../utils/derive-minor';

import { getMeQueryOptions } from './check-profile';

const slugRegex = /^[a-z0-9]+(?:-[a-z0-9]+)*$/;

/** Resolves a validation message from the `Onboarding.validation` namespace. */
type Translate = (key: string) => string;

/**
 * Builds the guardian-registration schema with localized validation messages.
 * The shape is identical across locales, so `GuardianRegisterInput` is stable.
 */
export const makeGuardianRegisterInputSchema = (t: Translate) =>
  z
    .object({
      childDisplayName: z.string().min(1, t('required')).max(80, t('max80')),
      slug: z
        .string()
        .min(3, t('min3'))
        .max(60, t('max60'))
        .regex(slugRegex, t('slugPattern')),
      childDateOfBirth: z.string().min(1, t('required')),
      defaultLocaleId: z.enum(['da', 'en']),
    })
    .superRefine((val, ctx) => {
      // Guardian-register is for minors only; an adult must self-register.
      if (isAdult(val.childDateOfBirth)) {
        ctx.addIssue({
          code: 'custom',
          path: ['childDateOfBirth'],
          message: t('adultMustSelfRegister'),
        });
      }
    });

export type GuardianRegisterInput = z.infer<
  ReturnType<typeof makeGuardianRegisterInputSchema>
>;

const toRequest = (
  input: GuardianRegisterInput,
): RegisterIndividualSiteGuardianRequest => ({
  childDisplayName: input.childDisplayName,
  slug: input.slug,
  childDateOfBirth: new Date(input.childDateOfBirth).toISOString(),
  defaultLocaleId: input.defaultLocaleId,
});

export const guardianRegister = async (
  input: GuardianRegisterInput,
): Promise<SiteResponse> => {
  const response = await api.individualSitesGuardianRegisterCreate(
    toRequest(input),
  );
  return response.data;
};

export const useGuardianRegister = (
  config?: MutationConfig<typeof guardianRegister>,
) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: guardianRegister,
    ...config,
    onSuccess: (...args) => {
      queryClient.invalidateQueries({
        queryKey: getMeQueryOptions().queryKey,
      });
      config?.onSuccess?.(...args);
    },
  });
};
