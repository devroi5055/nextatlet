import { User, Users } from 'lucide-react';
import { useTranslations } from 'next-intl';
import NextLink from 'next/link';

import { paths } from '@/config/paths';

type ProfileTypeOption = {
  key: string;
  href: string;
  icon: typeof User;
};

// Open for extension: a future profile type (e.g. a coach) is one more entry.
// Text is resolved from the `Onboarding.selector.<key>` message namespace.
const options: ProfileTypeOption[] = [
  {
    key: 'self',
    href: paths.onboarding.self.getHref(),
    icon: User,
  },
  {
    key: 'guardian',
    href: paths.onboarding.guardian.getHref(),
    icon: Users,
  },
];

/** Step 1 — who is the profile for? The choice is carried by the route split. */
export const ProfileTypeSelector = () => {
  const t = useTranslations('Onboarding.selector');
  return (
    <div className="grid gap-4 sm:grid-cols-2">
      {options.map((option) => {
        const Icon = option.icon;
        return (
          <NextLink
            key={option.key}
            href={option.href}
            className="group flex flex-col items-start rounded-xl border border-gray-200 p-6 text-left transition-colors hover:border-primary-gold hover:bg-primary-gold/5"
          >
            <span className="flex size-11 items-center justify-center rounded-lg bg-primary-gold/15 text-primary-gold">
              <Icon className="size-5" />
            </span>
            <h3 className="mt-4 text-base font-semibold text-gray-900">
              {t(`${option.key}.title`)}
            </h3>
            <p className="mt-1 text-sm text-gray-500">
              {t(`${option.key}.description`)}
            </p>
          </NextLink>
        );
      })}
    </div>
  );
};
