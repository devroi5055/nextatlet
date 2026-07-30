'use client';

import { useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';
import { useMemo } from 'react';

import { Button } from '@/components/ui/button';
import { Form, Input, Select } from '@/components/ui/form';
import { paths } from '@/config/paths';

import {
  makeGuardianRegisterInputSchema,
  useGuardianRegister,
} from '../api/guardian-register';
import { slugify } from '../utils/slugify';

/** Step 2b — a guardian registers a profile for their child. */
export const GuardianRegisterForm = () => {
  const router = useRouter();
  const t = useTranslations('Onboarding.form');
  const tv = useTranslations('Onboarding.validation');
  const schema = useMemo(() => makeGuardianRegisterInputSchema(tv), [tv]);

  const localeOptions = [
    { label: t('localeDa'), value: 'da' },
    { label: t('localeEn'), value: 'en' },
  ];

  const registering = useGuardianRegister({
    // The guardian-register endpoint has succeeded (child profile created with
    // the caller attached as guardian); land them on the site editor.
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

        return (
          <>
            <Input
              label={t('childName')}
              error={formState.errors['childDisplayName']}
              registration={register('childDisplayName', {
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
                placeholder={t('childSlugPlaceholder')}
                error={formState.errors['slug']}
                registration={register('slug')}
              />
              <p className="mt-1 text-xs text-gray-500">
                nextatlet.dk/{slug || t('childSlugPlaceholder')}
              </p>
            </div>

            <Input
              type="date"
              label={t('childDateOfBirth')}
              error={formState.errors['childDateOfBirth']}
              registration={register('childDateOfBirth')}
            />

            <Select
              label={t('language')}
              options={localeOptions}
              error={formState.errors['defaultLocaleId']}
              registration={register('defaultLocaleId')}
            />

            <Button
              type="submit"
              isLoading={registering.isPending}
              className="w-full"
            >
              {t('submitGuardian')}
            </Button>
          </>
        );
      }}
    </Form>
  );
};
