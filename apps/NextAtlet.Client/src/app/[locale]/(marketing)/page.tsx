import { use } from 'react';
import { setRequestLocale } from 'next-intl/server';
import { LandingPage } from '@/features/marketing';

const HomePage = ({ params }: { params: Promise<{ locale: string }> }) => {
  const { locale } = use(params);

  // Enable static rendering
  setRequestLocale(locale);

  return <LandingPage />;
};

export default HomePage;