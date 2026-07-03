import { BellRing, Flag, Medal, Shield, Star } from 'lucide-react';

import { cn } from '@/utils/cn';

import { type AthleteShowcase } from '../types';

const badgeIcon = { flag: Flag, medal: Medal, shield: Shield, star: Star };

export type AthleteProfileCardProps = {
  athlete: AthleteShowcase;
  /**
   * `hero` floats a sponsor notification over the portrait;
   * `browser` frames the card in a faux browser chrome with the public URL.
   */
  variant?: 'hero' | 'browser';
  className?: string;
};

const Portrait = () => (
  <div className="relative flex justify-center aspect-3/1 ph">
    <p className="ph-label">
      marcus-andersen.jpg
      <br />
      3:1 Ratio
    </p>
    <div className="pointer-events-none absolute inset-0 bg-linear-to-t from-background/80 to-transparent" />
  </div >
);

const Stats = ({ athlete }: { athlete: AthleteShowcase }) => (
  <div className="grid grid-cols-3 gap-2 sm:gap-3">
    {athlete.stats.map((stat, index) => (
      <div key={stat.label} className="text-center">
        <p
          className={`font-display text-md sm:text-2xl font-bold ${index === 0 ? 'text-primary' : 'text-foreground'
            }`}
        >
          {stat.value}
        </p>
        <p className="mt-1 text-[0.6rem] font-semibold uppercase tracking-widest text-muted-foreground">
          {stat.label}
        </p>
      </div>
    ))}
  </div>
);

/** Showcase of the generated athlete site — the product, made tangible. */
export const AthleteProfileCard = ({
  athlete,
  variant = 'hero',
  className,
}: AthleteProfileCardProps) => {
  return (
    <div
      className={cn(
        'overflow-hidden rounded-2xl border border-border bg-card shadow-2xl shadow-black/40',
        className,
      )}
    >
      {variant === 'browser' && (
        <div className="flex items-center gap-2 border-b border-border bg-background px-4 py-3">
          <span className="size-2.5 rounded-full bg-border" />
          <span className="size-2.5 rounded-full bg-border" />
          <span className="size-2.5 rounded-full bg-border" />
          <span className="ml-3 truncate rounded-md bg-muted px-3 py-1 text-xs text-muted-foreground">
            nextatlet.dk/{athlete.slug}
          </span>
        </div>
      )}

      <Portrait />

      <span className='relative avatar avatar-auto -mt-8 xl:-mt-15 mb-2 mx-5'>MA</span>

      <div className=" space-y-4 px-5">
        <div className="flex flex-col justify-start">
          <h3 className="font-display text-base sm:text-lg font-bold">
            {athlete.name}
          </h3>
          <p className="text-[0.7rem] sm:text-xs text-muted-foreground">
            {athlete.club} · {athlete.weightClass} · {athlete.ageClass}
          </p>
        </div>

        {athlete.badges && athlete.badges.length > 0 && (
          <div className="flex flex-wrap gap-1.5 sm:gap-2">
            {athlete.badges.map((badge) => {
              const Icon = badge.icon ? badgeIcon[badge.icon] : null;
              return (
                <span key={badge.label} className={`badge badge-${badge.variant} badge-sm`}>
                  {Icon && <Icon className="ico-sm hidden sm:block" />}
                  {badge.label}
                </span>
              );
            })}
          </div>
        )}

        <hr className="border-border" />

        <Stats athlete={athlete} />
        <hr className="border-border" />
      </div>

      {variant === 'hero' && athlete.notification && (
        <div className="mx-5 mb-5 flex items-center gap-3 rounded-xl border border-border bg-muted/80 p-3">
          <span className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-primary-gold/15 text-primary-gold">
            <BellRing className="size-4" />
          </span>
          <div className="min-w-0">
            <p className="truncate text-xs font-semibold text-foreground">
              {athlete.notification.title}
            </p>
            <p className="truncate text-[0.7rem] text-muted-foreground">
              {athlete.notification.brand} — {athlete.notification.time}
            </p>
          </div>
        </div>
      )}
    </div>
  );
};
