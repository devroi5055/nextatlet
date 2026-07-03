import { ArrowLeft } from 'lucide-react';
import NextLink from 'next/link';

import { paths } from '@/config/paths';
import { GuardianRegisterForm } from '@/features/onboarding';

const GuardianOnboardingPage = () => {
  return (
    <div>
      <NextLink
        href={paths.onboarding.root.getHref()}
        className="inline-flex items-center gap-1 text-sm text-gray-500 transition-colors hover:text-gray-900"
      >
        <ArrowLeft className="size-4" />
        Tilbage
      </NextLink>
      <h1 className="mt-4 text-xl font-semibold text-gray-900">
        Opret en profil for dit barn
      </h1>
      <p className="mt-1 text-sm text-gray-500">
        Du bliver værge for profilen og kan udfylde den, til barnet er klar til
        selv at overtage.
      </p>
      <div className="mt-6">
        <GuardianRegisterForm />
      </div>
    </div>
  );
};

export default GuardianOnboardingPage;
