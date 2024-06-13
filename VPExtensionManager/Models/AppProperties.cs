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
    /// The folder where the GitHub releases should be downloaded to
    /// </summary>
    public const string DownloadsFolder = "DownloadsFolder";

    /// <summary>
    /// The last time the app's latest version was checked
    /// </summary>
    public const string LastChecked = "LastChecked";

    /// <summary>
    /// Whether the app should check for updates on Start
    /// </summary>
    public const string CheckForUpdate = "CheckForUpdate";

    /// <summary>
    /// Link to the latest version from GitHub
    /// </summary>
    public const string LatestVersionURL = "LatestVersionURL";

    /// <summary>
    /// The latest version from GitHub
    /// </summary>
    public const string LatestVersion = "LatestVersion";

    /// <summary>
    /// Whether the URL Handler registry creation was asked on Start
    /// </summary>
    public const string AskedRegistryQuestion = "AskedRegistryQuestion";

    public static bool Contains(string key)
    {
        return App.Current.Properties.Contains(key);
    }

    public static bool Get(string key, out string value)
    {
        value = null;

        if (!Contains(key))
        {
            return false;
        }

        value = App.Current.Properties[key].ToString();
        return true;
    }

    public static void Set(string key, string value)
    {
        App.Current.Properties[key] = value;
    }
}
