import { ProfileTypeSelector } from '@/features/onboarding';

const OnboardingPage = () => {
  return (
    <div>
      <h1 className="text-xl font-semibold text-gray-900">
        Hvem er profilen til?
      </h1>
      <p className="mt-1 text-sm text-gray-500">
        Vælg, hvem du opretter en profil for.
      </p>
      <div className="mt-6">
        <ProfileTypeSelector />
      </div>
    </div>
  );
};

export default OnboardingPage;
