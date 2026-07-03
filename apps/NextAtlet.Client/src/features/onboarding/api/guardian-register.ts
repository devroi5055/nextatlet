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

export const guardianRegisterInputSchema = z
  .object({
    childDisplayName: z.string().min(1, 'Påkrævet').max(80, 'Højst 80 tegn'),
    slug: z
      .string()
      .min(3, 'Mindst 3 tegn')
      .max(60, 'Højst 60 tegn')
      .regex(slugRegex, 'Kun små bogstaver, tal og bindestreger'),
    childDateOfBirth: z.string().min(1, 'Påkrævet'),
    defaultLocaleId: z.enum(['da', 'en']),
  })
  .superRefine((val, ctx) => {
    // Guardian-register is for minors only; an adult must self-register.
    if (isAdult(val.childDateOfBirth)) {
      ctx.addIssue({
        code: 'custom',
        path: ['childDateOfBirth'],
        message:
          'En person på 18 år eller derover skal oprette sin egen profil.',
      });
    }
  });

export type GuardianRegisterInput = z.infer<typeof guardianRegisterInputSchema>;

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
