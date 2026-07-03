using System;
using System.Collections.Generic;
using System.Text;

namespace NextAtlet.Domain.strings
{
    public class Strings
    {
        public static List<string> ReservedSlugs = ["admin", "api", "about", "contact", "terms", "privacy", "login", "signup", "settings", "dashboard"];

        public class Capabilities
        {
            public const string Hero = "hero";
            public const string Bio = "bio";
            public const string Contact = "contact";
            public const string Gallery = "gallery";
            public const string Results = "results";
            public const string Sponsors = "sponsors";
            public const string Video = "video";
        }
        public class ThemeKeys
        {
            public const string Classic = "classic";
            public const string Momentum = "momentum";
            public const string Elite = "elite";
        }
        public class Fonts
        {

            public const string Inter = "Inter";
            public const string Sora = "Sora";
            public const string SpaceGrotesk = "Space Grotesk";
            public const string Manrope = "Manrope";
            public const string WorkSans = "Work Sans";
            public const string SourceSans = "Source Sans 3";
            public const string Archivo = "Archivo";
            public const string LibreFranklin = "Libre Franklin";
            public const string Fraunces = "Fraunces";

            // the curated set — what's legal to use in a theme
            public static readonly HashSet<string> All =
            [
                Inter, Sora, SpaceGrotesk, Manrope, WorkSans,
                SourceSans, Archivo, LibreFranklin, Fraunces
            ];
        }
        public class StyleKeys
        {
            public const string Radius = "radius";
            public const string Fill = "fill";
            public const string Shadow = "shadow";
            public const string Border = "border";
            public const string Size = "size";
        }
        public class StyleValues
        {
            // shared primitive values — defined once
            public const string Large = "large";
            public const string Medium = "medium";
            public const string Small = "small";

            public const string Sharp = "sharp";
            public const string Rounded = "rounded";
            public const string Pill = "pill";
            public const string Ghost = "ghost";

            public const string Glass = "glass";
            public const string Outline = "outline";
            public const string Solid = "solid";

            public const string Thick = "thick";
            public const string Normal = "normal";
            public const string Thin = "thin";

            public const string Strong = "strong";
            public const string Subtle = "subtle";
            public const string None = "none";

            // legal-value sets per key — reference the shared constants
            public static readonly HashSet<string> Radius = [None, Sharp, Rounded, Pill];
            public static readonly HashSet<string> Fill = [None, Solid, Outline, Ghost, Glass];
            public static readonly HashSet<string> Border = [None, Thin, Normal, Thick, Ghost];
            public static readonly HashSet<string> Shadow = [None, Subtle, Medium, Strong];
            public static readonly HashSet<string> Size = [Small, Medium, Large];
        }
    }
}
