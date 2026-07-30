import { landingSections } from './landing-sections';



/**
 * The public marketing landing page: sticky header, the ordered section
 * registry, and the footer. Sections are rendered from `landingSections` so
 * the page stays closed for modification but open for extension.
 */
export const LandingPage = () => {
  return (
    <div className="min-h-screen bg-background font-display text-foreground antialiased color">
      <main>
        {landingSections.map((Section, index) => (
          <Section key={index} />
        ))}
      </main>
    </div>
  );
};
