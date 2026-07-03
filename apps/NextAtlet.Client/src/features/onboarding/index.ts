export { ProfileTypeSelector } from './components/profile-type-selector';
export { SelfRegisterForm } from './components/self-register-form';
export { GuardianRegisterForm } from './components/guardian-register-form';

export { getMe, getMeQueryOptions, useMe, hasCompletedOnboarding } from './api/check-profile';
export { useSelfRegister, selfRegisterInputSchema } from './api/self-register';
export {
  useGuardianRegister,
  guardianRegisterInputSchema,
} from './api/guardian-register';

export * from './utils/derive-minor';
export { slugify } from './utils/slugify';
