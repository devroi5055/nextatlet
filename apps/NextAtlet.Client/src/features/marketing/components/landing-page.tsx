import { landingSections } from './landing-sections';
import { MarketingFooter } from './marketing-footer';
import { MarketingHeader } from './marketing-header';

/**
 * The public marketing landing page: sticky header, the ordered section
 * registry, and the footer. Sections are rendered from `landingSections` so
 * the page stays closed for modification but open for extension.
 */
export const LandingPage = () => {
  return (
    <div className="min-h-screen bg-brand-ink font-display text-brand-cream antialiased">
      <MarketingHeader />
      <main>
        {landingSections.map((Section, index) => (
          <Section key={index} />
        ))}
      </main>
      <MarketingFooter />
    </div>
  );
};
