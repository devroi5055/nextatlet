import { useTranslations } from 'next-intl';

import { ProfileTypeSelector } from '@/features/onboarding';

const OnboardingPage = () => {
  const t = useTranslations('Onboarding.chooseType');
  return (
    <div>
      <h1 className="text-xl font-semibold text-gray-900">{t('title')}</h1>
      <p className="mt-1 text-sm text-gray-500">{t('subtitle')}</p>
      <div className="mt-6">
        <ProfileTypeSelector />
      </div>
    </div>
  );
};

export default OnboardingPage;
