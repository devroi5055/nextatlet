'use client';

import { useRouter } from 'next/navigation';

import { Button } from '@/components/ui/button';
import { Form, Input, Select } from '@/components/ui/form';
import { paths } from '@/config/paths';

import { selfRegisterInputSchema, useSelfRegister } from '../api/self-register';
import { requiresGuardianConsent } from '../utils/derive-minor';
import { slugify } from '../utils/slugify';

const localeOptions = [
  { label: 'Dansk', value: 'da' },
  { label: 'English', value: 'en' },
];

/** Step 2a — the athlete registers their own profile (age-conditional). */
export const SelfRegisterForm = () => {
  const router = useRouter();
  const registering = useSelfRegister({
    onSuccess: (_site, variables) => {
      const state = requiresGuardianConsent(variables.dateOfBirth)
        ? 'consent-pending'
        : 'ready';
      router.push(`${paths.onboarding.complete.getHref()}?state=${state}`);
    },
  });

  return (
    <Form
      schema={selfRegisterInputSchema}
      onSubmit={(values) => registering.mutate(values)}
      options={{ defaultValues: { defaultLocaleId: 'da' } }}
    >
      {({ register, formState, watch, setValue, getValues }) => {
        const slug = watch('slug');
        const dateOfBirth = watch('dateOfBirth');
        const showGuardian = dateOfBirth
          ? requiresGuardianConsent(dateOfBirth)
          : false;

        return (
          <>
            <Input
              label="Visningsnavn"
              error={formState.errors['displayName']}
              registration={register('displayName', {
                // Suggest a slug from the name, but only if the user hasn't typed one.
                onBlur: (e) => {
                  if (!getValues('slug')) {
                    setValue('slug', slugify(e.target.value), {
                      shouldValidate: true,
                    });
                  }
                },
              })}
            />

            <div>
              <Input
                label="Profil-URL"
                placeholder="dit-navn"
                error={formState.errors['slug']}
                registration={register('slug')}
              />
              <p className="mt-1 text-xs text-gray-500">
                nextatlet.dk/{slug || 'dit-navn'}
              </p>
            </div>

            <Input
              type="date"
              label="Fødselsdato"
              error={formState.errors['dateOfBirth']}
              registration={register('dateOfBirth')}
            />

            <Select
              label="Sprog"
              options={localeOptions}
              error={formState.errors['defaultLocaleId']}
              registration={register('defaultLocaleId')}
            />

            {showGuardian && (
              <div className="space-y-2">
                <Input
                  type="email"
                  label="Forælders e-mail"
                  error={formState.errors['guardianEmail']}
                  registration={register('guardianEmail')}
                />
                <p className="text-xs text-gray-500">
                  Da du er under 16, sender vi en anmodning om samtykke til din
                  forælder. Din profil kan først offentliggøres, når de har
                  godkendt.
                </p>
              </div>
            )}

            <Button
              type="submit"
              isLoading={registering.isPending}
              className="w-full"
            >
              Opret profil
            </Button>
          </>
        );
      }}
    </Form>
  );
};
