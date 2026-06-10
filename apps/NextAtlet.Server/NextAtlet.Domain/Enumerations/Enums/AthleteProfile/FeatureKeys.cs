using System;
using System.Collections.Generic;
using System.Text;

namespace NextAtlet.Domain.Enumerations.Enums.AthleteProfile
{
    public static class FeatureKeys
    {
        // sections
        public const string SectionHero = "sections.hero";
        public const string SectionBio = "sections.bio";
        public const string SectionResults = "sections.results";
        public const string SectionGallery = "sections.gallery";
        public const string SectionSponsors = "sections.sponsors";
        public const string SectionVideo = "sections.video";

        // themes
        public const string ThemeBasic = "themes.basic";      // Free
        public const string ThemeStandard = "themes.standard";   // Plus
        public const string ThemeAdvanced = "themes.advanced";   // Pro

        // analytics
        public const string AnalyticsBasic = "analytics.basic";
        public const string AnalyticsFull = "analytics.full";

        // mentoring
        public const string MentoringGuides = "mentoring.guides";
        public const string MentoringOneOnOne = "mentoring.1on1";
    }
}
