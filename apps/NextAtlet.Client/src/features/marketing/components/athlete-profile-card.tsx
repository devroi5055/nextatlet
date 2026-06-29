import { BellRing, User } from 'lucide-react';

import { cn } from '@/utils/cn';

import { type AthleteShowcase } from '../types';

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
  <div className="relative flex aspect-[4/3] items-center justify-center overflow-hidden bg-gradient-to-br from-brand-surface to-brand-ink">
    <User className="size-20 text-brand-gold/40" strokeWidth={1.25} />
    <div className="pointer-events-none absolute inset-0 bg-gradient-to-t from-brand-ink/80 to-transparent" />
  </div>
);

const Stats = ({ athlete }: { athlete: AthleteShowcase }) => (
  <div className="grid grid-cols-3 gap-3">
    {athlete.stats.map((stat) => (
      <div key={stat.label} className="text-center">
        <p className="font-display text-2xl font-bold text-brand-gold">
          {stat.value}
        </p>
        <p className="mt-1 text-[0.6rem] font-semibold uppercase tracking-widest text-brand-muted">
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
        'overflow-hidden rounded-2xl border border-brand-line bg-brand-ink-soft shadow-2xl shadow-black/40',
        className,
      )}
    >
      {variant === 'browser' && (
        <div className="flex items-center gap-2 border-b border-brand-line bg-brand-ink px-4 py-3">
          <span className="size-2.5 rounded-full bg-brand-line" />
          <span className="size-2.5 rounded-full bg-brand-line" />
          <span className="size-2.5 rounded-full bg-brand-line" />
          <span className="ml-3 truncate rounded-md bg-brand-surface px-3 py-1 text-xs text-brand-muted">
            nextatlet.dk/{athlete.slug}
          </span>
        </div>
      )}

      <Portrait />

      <div className="space-y-4 p-5">
        <div>
          <h3 className="font-display text-lg font-bold text-brand-cream">
            {athlete.name}
          </h3>
          <p className="text-xs text-brand-muted">
            {athlete.club} · {athlete.weightClass} · {athlete.ageClass}
          </p>
        </div>

        <Stats athlete={athlete} />

        {variant === 'browser' && (
          <div className="flex flex-wrap gap-2 pt-1">
            {athlete.tags.map((tag) => (
              <span
                key={tag}
                className="rounded-full border border-brand-line bg-brand-surface px-3 py-1 text-[0.65rem] font-medium text-brand-cream"
              >
                {tag}
              </span>
            ))}
          </div>
        )}
      </div>

      {variant === 'hero' && athlete.notification && (
        <div className="mx-5 mb-5 flex items-center gap-3 rounded-xl border border-brand-line bg-brand-surface/80 p-3">
          <span className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-brand-gold/15 text-brand-gold">
            <BellRing className="size-4" />
          </span>
          <div className="min-w-0">
            <p className="truncate text-xs font-semibold text-brand-cream">
              {athlete.notification.title}
            </p>
            <p className="truncate text-[0.7rem] text-brand-muted">
              {athlete.notification.brand} — {athlete.notification.time}
            </p>
          </div>
        </div>
      )}
    </div>
  );
};
