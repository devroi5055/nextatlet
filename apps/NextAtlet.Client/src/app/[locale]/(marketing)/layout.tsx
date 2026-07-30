import { ReactNode } from 'react';
import { MarketingHeader } from '@/features/marketing/components/marketing-header';
import { MarketingFooter } from '@/features/marketing/components/marketing-footer';

export default function MarketingLayout({ children }: { children: ReactNode }) {
  return (
    <div className="min-h-screen bg-background font-display text-foreground antialiased">
      <MarketingHeader />
      {children}
      <MarketingFooter />
    </div>
  );
}