namespace VPExtensionManager.Core.Models
{
    public class RFXStrings
    {
        // General
        public const string RatinFX = "RatinFX";

        // Extensions
        public const string VPConsole = "VPConsole";
        public const string VegasProFlow = "VegasProFlow";
        public const string ShortenExtendMedia = "ShortenExtendMedia";
        public const string CustomFades = "CustomFades";

        // References
        public const string VegasProData = "VegasProData";
        public const string RatinFXVP = "RatinFX.VP";
        public const string NewtonsoftJson = "Newtonsoft.Json";

        public static string[] VPConsoleRefs = [VegasProData, RatinFXVP, NewtonsoftJson];
        public static string[] VegasProFlowRefs = [VegasProData, RatinFXVP, NewtonsoftJson];
    }
}
