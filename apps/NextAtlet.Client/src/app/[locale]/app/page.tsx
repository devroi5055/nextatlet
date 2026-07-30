import { getTranslations } from 'next-intl/server';

import { DashboardInfo } from './_components/dashboard-info';

export async function generateMetadata() {
  const t = await getTranslations('Metadata');
  return {
    title: t('dashboardTitle'),
    description: t('dashboardDescription'),
  };
}

const DashboardPage = async () => {
  return <DashboardInfo />;
};

export default DashboardPage;
