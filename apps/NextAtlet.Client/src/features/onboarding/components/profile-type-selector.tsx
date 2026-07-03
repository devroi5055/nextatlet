import { User, Users } from 'lucide-react';
import NextLink from 'next/link';

import { paths } from '@/config/paths';

type ProfileTypeOption = {
  href: string;
  icon: typeof User;
  title: string;
  description: string;
};

// Open for extension: a future profile type (e.g. a coach) is one more entry.
const options: ProfileTypeOption[] = [
  {
    href: paths.onboarding.self.getHref(),
    icon: User,
    title: 'Mig selv',
    description: 'Jeg er atleten og opretter min egen profil.',
  },
  {
    href: paths.onboarding.guardian.getHref(),
    icon: Users,
    title: 'Mit barn',
    description: 'Jeg opretter en profil for mit barn som forælder/værge.',
  },
];

/** Step 1 — who is the profile for? The choice is carried by the route split. */
export const ProfileTypeSelector = () => {
  return (
    <div className="grid gap-4 sm:grid-cols-2">
      {options.map((option) => {
        const Icon = option.icon;
        return (
          <NextLink
            key={option.href}
            href={option.href}
            className="group flex flex-col items-start rounded-xl border border-gray-200 p-6 text-left transition-colors hover:border-primary-gold hover:bg-primary-gold/5"
          >
            <span className="flex size-11 items-center justify-center rounded-lg bg-primary-gold/15 text-primary-gold">
              <Icon className="size-5" />
            </span>
            <h3 className="mt-4 text-base font-semibold text-gray-900">
              {option.title}
            </h3>
            <p className="mt-1 text-sm text-gray-500">{option.description}</p>
          </NextLink>
        );
      })}
    </div>
  );
};
