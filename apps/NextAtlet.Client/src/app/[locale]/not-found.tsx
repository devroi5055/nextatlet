import { useTranslations } from 'next-intl';

import { Link } from '@/components/ui/link';
import { paths } from '@/config/paths';

const NotFoundPage = () => {
  const t = useTranslations('Errors');
  return (
    <div className="mt-52 flex flex-col items-center font-semibold">
      <h1>{t('notFoundTitle')}</h1>
      <p>{t('notFoundBody')}</p>
      <Link href={paths.home.getHref()} replace>
        {t('goHome')}
      </Link>
    </div>
  );
};

export default NotFoundPage;
