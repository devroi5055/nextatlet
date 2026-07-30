'use client';

import { useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';
import { useMemo } from 'react';

import { Button } from '@/components/ui/button';
import { Form, Input, Select } from '@/components/ui/form';
import { paths } from '@/config/paths';

import { makeSelfRegisterInputSchema, useSelfRegister } from '../api/self-register';
import { requiresGuardianConsent } from '../utils/derive-minor';
import { slugify } from '../utils/slugify';

/** Step 2a — the athlete registers their own profile (age-conditional). */
export const SelfRegisterForm = () => {
  const router = useRouter();
  const t = useTranslations('Onboarding.form');
  const tv = useTranslations('Onboarding.validation');
  const schema = useMemo(() => makeSelfRegisterInputSchema(tv), [tv]);

  const localeOptions = [
    { label: t('localeDa'), value: 'da' },
    { label: t('localeEn'), value: 'en' },
  ];

  const registering = useSelfRegister({
    // The register endpoint has succeeded (profile created); land the athlete
    // straight on their own site editor.
    onSuccess: () => {
      router.push(paths.app.editor.getHref());
    },
  });

  return (
    <Form
      schema={schema}
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
              label={t('displayName')}
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
                label={t('slug')}
                placeholder={t('slugPlaceholder')}
                error={formState.errors['slug']}
                registration={register('slug')}
              />
              <p className="mt-1 text-xs text-gray-500">
                nextatlet.dk/{slug || t('slugPlaceholder')}
              </p>
            </div>

            <Input
              type="date"
              label={t('dateOfBirth')}
              error={formState.errors['dateOfBirth']}
              registration={register('dateOfBirth')}
            />

            <Select
              label={t('language')}
              options={localeOptions}
              error={formState.errors['defaultLocaleId']}
              registration={register('defaultLocaleId')}
            />

            {showGuardian && (
              <div className="space-y-2">
                <Input
                  type="email"
                  label={t('guardianEmail')}
                  error={formState.errors['guardianEmail']}
                  registration={register('guardianEmail')}
                />
                <p className="text-xs text-gray-500">{t('guardianNote')}</p>
              </div>
            )}

            <Button
              type="submit"
              isLoading={registering.isPending}
              className="w-full"
            >
              {t('submitSelf')}
            </Button>
          </>
        );
      }}
    </Form>
  );
};
