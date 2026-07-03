import { CheckCircle2, MailCheck } from 'lucide-react';
import NextLink from 'next/link';

import { Button } from '@/components/ui/button';
import { paths } from '@/config/paths';

/**
 * Completion states (plan §7). `consent-pending` is the self-register-as-minor
 * case: the profile exists but stays unpublished until the guardian confirms
 * the emailed consent link.
 */
const completionStates = {
  ready: {
    icon: CheckCircle2,
    title: 'Din profil er oprettet',
    body: 'Du er klar. Gå til dit dashboard for at tilføje bio, resultater og vælge tema.',
  },
  'consent-pending': {
    icon: MailCheck,
    title: 'Næsten klar — vi mangler samtykke',
    body: 'Vi har sendt en anmodning om samtykke til din forælder. Din profil er oprettet, men kan først offentliggøres, når de har godkendt via linket i e-mailen.',
  },
  guardian: {
    icon: CheckCircle2,
    title: 'Barnets profil er oprettet',
    body: 'Du er nu værge for profilen. Gå til dit dashboard for at udfylde oplysningerne — profilen kan offentliggøres, når du er klar.',
  },
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

  return (
    <div className="text-center">
      <span className="mx-auto flex size-14 items-center justify-center rounded-full bg-primary-gold/15 text-primary-gold">
        <Icon className="size-7" />
      </span>
      <h1 className="mt-5 text-xl font-semibold text-gray-900">
        {content.title}
      </h1>
      <p className="mt-2 text-sm text-gray-500">{content.body}</p>
      <NextLink href={paths.app.dashboard.getHref()} className="mt-6 inline-block">
        <Button>Gå til dashboard</Button>
      </NextLink>
    </div>
  );
};

export default OnboardingCompletePage;
