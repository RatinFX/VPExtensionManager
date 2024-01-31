namespace VPExtensionManager.Models;

public class AppProperties
{
    /// <summary>
    /// The selected app Theme
    /// </summary>
    public const string Theme = "Theme";
    /// <summary>
    /// A list of installed VP versions
    /// </summary>
    public const string VPVersions = "VPVersions";
    /// <summary>
    /// Whether an install should be re-downloaded or not
    /// </summary>
    public const string ForceDownload = "ForceDownload";
    /// <summary>
    /// The folder where the GitHub releases should be downloaded to
    /// </summary>
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
