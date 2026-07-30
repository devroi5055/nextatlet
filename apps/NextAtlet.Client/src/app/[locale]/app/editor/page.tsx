import { LayoutTemplate, Palette } from 'lucide-react';
import { useTranslations } from 'next-intl';
import { getTranslations } from 'next-intl/server';

export async function generateMetadata() {
  const t = await getTranslations('Editor');
  return {
    title: t('metaTitle'),
    description: t('metaDescription'),
  };
}

/**
 * The athlete's own site draft editor — the post-onboarding landing. Reaching
 * this route means the `/app` decision gate confirmed an authenticated user
 * with a completed profile. The section + theme editing surfaces are stubbed
 * for now (build order §4/§5); this page just establishes the landing.
 */
const EditorPage = () => {
  const t = useTranslations('Editor');

  const placeholders = [
    { key: 'sections', icon: LayoutTemplate },
    { key: 'themes', icon: Palette },
  ] as const;

  return (
    <div className="mx-auto w-full max-w-4xl">
      <div className="flex items-center gap-3">
        <h1 className="text-2xl font-bold text-foreground">{t('title')}</h1>
        <span className="rounded-full bg-primary-gold/15 px-3 py-1 text-xs font-semibold uppercase tracking-wider text-primary-gold">
          {t('draftBadge')}
        </span>
      </div>
      <p className="mt-2 max-w-2xl text-sm text-muted-foreground">
        {t('subtitle')}
      </p>

      <div className="mt-8 grid gap-4 sm:grid-cols-2">
        {placeholders.map(({ key, icon: Icon }) => (
          <section
            key={key}
            className="rounded-2xl border border-dashed border-border bg-card p-6"
          >
            <span className="flex size-11 items-center justify-center rounded-xl bg-primary-gold/15 text-primary-gold">
              <Icon className="size-5" />
            </span>
            <h2 className="mt-4 font-semibold text-foreground">
              {t(`${key}Title`)}
            </h2>
            <p className="mt-1 text-sm text-muted-foreground">
              {t(`${key}Body`)}
            </p>
            <span className="mt-4 inline-block text-xs font-medium uppercase tracking-wider text-muted-foreground/70">
              {t('comingSoon')}
            </span>
          </section>
        ))}
      </div>
    </div>
  );
};

export default EditorPage;
