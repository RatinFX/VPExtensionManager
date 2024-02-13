using Octokit;

namespace VPExtensionManager.Models;

public class ShortReleaseAsset
{
    public string Name { get; set; }
    public string BrowserDownloadUrl { get; set; }
    public VPVersion VP { get; set; }

    public ShortReleaseAsset() { }
    public ShortReleaseAsset(ReleaseAsset release, VPVersion vp)
    {
        BrowserDownloadUrl = release.BrowserDownloadUrl;
        Name = release.Name;
        VP = vp;
    }

    /// <summary>
    /// Checks for the following naming scheme: "ExtensionName + VP version"
    /// - for example: VPConsole14, VegasProFlow13
    /// </summary>
    public static VPVersion GetVersion(string name, string tag)
        => name.Replace(tag, "").Contains(VPVersion.Sony.ToString("D")) ? VPVersion.Sony : VPVersion.Magix;
}