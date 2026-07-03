import { ArrowLeft } from 'lucide-react';
import NextLink from 'next/link';

import { paths } from '@/config/paths';
import { SelfRegisterForm } from '@/features/onboarding';

const SelfOnboardingPage = () => {
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
        Opret din profil
      </h1>
      <p className="mt-1 text-sm text-gray-500">
        Udfyld dine oplysninger — din hjemmeside genereres automatisk.
      </p>
      <div className="mt-6">
        <SelfRegisterForm />
      </div>
    </div>
  );
};

export default SelfOnboardingPage;
