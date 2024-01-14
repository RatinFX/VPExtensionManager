
namespace VPExtensionManager.Models
{
    public class AppProperties
    {
        public const string Theme = "Theme";
        public const string VPVersions = "VPVersions";
        public const string ForceDownload = "ForceDownload";
        public const string DownloadsFolder = "DownloadsFolder";

        public static bool Contains(string key)
        {
            return App.Current.Properties.Contains(key);
        }

        public static bool Get(string key, out string value)
        {
            value = null;

            if (!Contains(key))
                return false;

            value = App.Current.Properties[key].ToString();
            return true;
        }

        public static void Set(string key, string value)
        {
            App.Current.Properties[key] = value;
        }
    }
}
