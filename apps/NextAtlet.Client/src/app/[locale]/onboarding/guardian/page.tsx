import { ArrowLeft } from 'lucide-react';
import { useTranslations } from 'next-intl';
import NextLink from 'next/link';

import { paths } from '@/config/paths';
import { GuardianRegisterForm } from '@/features/onboarding';

const GuardianOnboardingPage = () => {
  const t = useTranslations('Onboarding');
  const tc = useTranslations('Common');
  return (
    <div>
      <NextLink
        href={paths.onboarding.root.getHref()}
        className="inline-flex items-center gap-1 text-sm text-gray-500 transition-colors hover:text-gray-900"
      >
        <ArrowLeft className="size-4" />
        {tc('back')}
      </NextLink>
      <h1 className="mt-4 text-xl font-semibold text-gray-900">
        {t('guardian.title')}
      </h1>
      <p className="mt-1 text-sm text-gray-500">{t('guardian.subtitle')}</p>
      <div className="mt-6">
        <GuardianRegisterForm />
      </div>
    </div>
  );
};

export default GuardianOnboardingPage;
