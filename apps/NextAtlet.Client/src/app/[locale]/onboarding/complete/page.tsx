import { CheckCircle2, MailCheck } from 'lucide-react';
import { getTranslations } from 'next-intl/server';
import NextLink from 'next/link';

import { Button } from '@/components/ui/button';
import { paths } from '@/config/paths';

/**
 * Completion states (plan §7). `consent-pending` is the self-register-as-minor
 * case: the profile exists but stays unpublished until the guardian confirms
 * the emailed consent link. Icon + message key per state; text is resolved from
 * the `Onboarding.complete` message namespace.
 */
const completionStates = {
  ready: { icon: CheckCircle2, key: 'ready' },
  'consent-pending': { icon: MailCheck, key: 'consentPending' },
  guardian: { icon: CheckCircle2, key: 'guardian' },
} as const;

type CompletionState = keyof typeof completionStates;

const OnboardingCompletePage = async ({
  searchParams,
}: {
  searchParams: Promise<{ state?: string }>;
}) => {
  const { state } = await searchParams;
  const key: CompletionState =
    state && state in completionStates
      ? (state as CompletionState)
      : 'ready';
  const content = completionStates[key];
  const Icon = content.icon;
  const t = await getTranslations('Onboarding.complete');

  return (
    <div className="text-center">
      <span className="mx-auto flex size-14 items-center justify-center rounded-full bg-primary-gold/15 text-primary-gold">
        <Icon className="size-7" />
      </span>
      <h1 className="mt-5 text-xl font-semibold text-gray-900">
        {t(`${content.key}.title`)}
      </h1>
      <p className="mt-2 text-sm text-gray-500">{t(`${content.key}.body`)}</p>
      <NextLink href={paths.app.editor.getHref()} className="mt-6 inline-block">
        <Button>{t('cta')}</Button>
      </NextLink>
    </div>
  );
};

export default OnboardingCompletePage;
