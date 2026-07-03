'use client';

import { useRouter } from 'next/navigation';

import { Button } from '@/components/ui/button';
import { Form, Input, Select } from '@/components/ui/form';
import { paths } from '@/config/paths';

import {
  guardianRegisterInputSchema,
  useGuardianRegister,
} from '../api/guardian-register';
import { slugify } from '../utils/slugify';

const localeOptions = [
  { label: 'Dansk', value: 'da' },
  { label: 'English', value: 'en' },
];

/** Step 2b — a guardian registers a profile for their child. */
export const GuardianRegisterForm = () => {
  const router = useRouter();
  const registering = useGuardianRegister({
    onSuccess: () => {
      router.push(`${paths.onboarding.complete.getHref()}?state=guardian`);
    },
  });

  return (
    <Form
      schema={guardianRegisterInputSchema}
      onSubmit={(values) => registering.mutate(values)}
      options={{ defaultValues: { defaultLocaleId: 'da' } }}
    >
      {({ register, formState, watch, setValue, getValues }) => {
        const slug = watch('slug');

        return (
          <>
            <Input
              label="Barnets navn"
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
                label="Profil-URL"
                placeholder="barnets-navn"
                error={formState.errors['slug']}
                registration={register('slug')}
              />
              <p className="mt-1 text-xs text-gray-500">
                nextatlet.dk/{slug || 'barnets-navn'}
              </p>
            </div>

            <Input
              type="date"
              label="Barnets fødselsdato"
              error={formState.errors['childDateOfBirth']}
              registration={register('childDateOfBirth')}
            />

            <Select
              label="Sprog"
              options={localeOptions}
              error={formState.errors['defaultLocaleId']}
              registration={register('defaultLocaleId')}
            />

            <Button
              type="submit"
              isLoading={registering.isPending}
              className="w-full"
            >
              Opret barnets profil
            </Button>
          </>
        );
      }}
    </Form>
  );
};
