// in code, keyed by Plan.Key — Level 1 capability definitions
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;

namespace NextAtlet.Domain.ValueObjects;

public static class PlanCapabilities
{
    static public List<string > AthleteFree = new()
    {
        FeatureKeys.SectionHero,
        FeatureKeys.SectionBio,
        FeatureKeys.SectionResults,
        FeatureKeys.ThemeBasic,
        FeatureKeys.AnalyticsBasic,
    };
     static public List<string> AthletePlus = new()
    {
        // everything Free has, plus:
        FeatureKeys.SectionHero,
        FeatureKeys.SectionBio,
        FeatureKeys.SectionResults,
        FeatureKeys.SectionGallery,
        FeatureKeys.SectionSponsors,
        FeatureKeys.ThemeBasic,
        FeatureKeys.ThemeStandard,
        FeatureKeys.AnalyticsBasic,
        FeatureKeys.MentoringGuides,
    };
     static public List<string> AthletePro = new()
    {
        // everything Plus has, plus:
        FeatureKeys.SectionHero,
        FeatureKeys.SectionBio,
        FeatureKeys.SectionResults,
        FeatureKeys.SectionGallery,
        FeatureKeys.SectionSponsors,
        FeatureKeys.SectionVideo,
        FeatureKeys.ThemeBasic,
        FeatureKeys.ThemeStandard,
        FeatureKeys.ThemeAdvanced,
        FeatureKeys.AnalyticsBasic,
        FeatureKeys.AnalyticsFull,
        FeatureKeys.MentoringGuides,
        FeatureKeys.MentoringOneOnOne,
    };
}