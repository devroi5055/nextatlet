/**
 * Client-side age helpers — for UI gating only. The server is the single source
 * of truth (NextAtlet.Application/Common/Options/AgeThresholdOptions.cs); these
 * mirror the Denmark launch values so the form can reveal/validate the right
 * fields without a round-trip. Never trust these for authorization.
 */
export const AGE_THRESHOLDS = {
  /** Cannot self-register below this age — must be guardian-registered. */
  absoluteMinimum: 13,
  /** Below this age a guardian must consent (GDPR Art. 8). */
  selfConsent: 16,
  /** Guardian-register is rejected at/above this age (the control boundary). */
  guardianBoundary: 18,
} as const;

/** Whole years between `dateOfBirth` and today, or null if unparseable. */
export const getAge = (dateOfBirth: string | Date): number | null => {
  if (!dateOfBirth) return null;
  const dob = new Date(dateOfBirth);
  if (Number.isNaN(dob.getTime())) return null;

  const today = new Date();
  let age = today.getFullYear() - dob.getFullYear();
  const monthDelta = today.getMonth() - dob.getMonth();
  if (monthDelta < 0 || (monthDelta === 0 && today.getDate() < dob.getDate())) {
    age -= 1;
  }
  return age;
};

/** A self-registering athlete this young needs a guardian's emailed consent. */
export const requiresGuardianConsent = (dateOfBirth: string | Date): boolean => {
  const age = getAge(dateOfBirth);
  return age !== null && age < AGE_THRESHOLDS.selfConsent;
};

/** Too young to self-register at all — a guardian must create the profile. */
export const isBelowSelfRegisterFloor = (
  dateOfBirth: string | Date,
): boolean => {
  const age = getAge(dateOfBirth);
  return age !== null && age < AGE_THRESHOLDS.absoluteMinimum;
};

/** An adult must self-register; a guardian cannot register them. */
export const isAdult = (dateOfBirth: string | Date): boolean => {
  const age = getAge(dateOfBirth);
  return age !== null && age >= AGE_THRESHOLDS.guardianBoundary;
};
