import { z } from 'zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';

import { RegisterIndividualSiteSelfRequest, SiteResponse } from '@/types/api';
import { api } from '@/lib/api-client';
import { MutationConfig } from '@/lib/react-query';

// Input schema for THIS feature — self-registration, not discussions
export const selfRegisterInputSchema = z.object({
    displayName: z.string().min(1, 'Required').max(20, 'Max 20'),
    slug: z.string().min(1, 'Required').max(20, 'Max 20'),
    dateOfBirth: z.string().min(1, 'Required').max(20, 'Max 20'),       // or z.coerce.date() depending on your form
    defaultLocaleId: z.string().max(5, 'Max 5').optional(),
    guardianEmail: z.string().email('Not Valid Email').max(30, 'Max 30').optional(),
    parentalConsentConfirmed: z.boolean().optional(),
});

export type SelfRegisterInput = z.infer<typeof selfRegisterInputSchema>;

export const selfRegisterSite = async (data: RegisterIndividualSiteSelfRequest): Promise<SiteResponse> => {
    const response = await api.individualSitesSelfRegisterCreate(data);
    return response.data;
};

export const useSelfRegisterSite = (config?: MutationConfig<typeof selfRegisterSite>) => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: selfRegisterSite,
        ...config,                              // ← spread config FIRST
        onSuccess: (site, ...args) => {
            queryClient.invalidateQueries({ queryKey: ['user'] });
            config?.onSuccess?.(site, ...args);   // ← then call the caller's handler
        },
    });
};