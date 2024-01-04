using Octokit;

namespace VPExtensionManager.Core.Models;

public class Install
{
    public string VPVersion { get; set; }
    public string InstallPath { get; set; }
}

public enum ExtensionType
{
    Extension,
    Script,
}

public class VPExtension
{
    public string Creator { get; set; } = "creator";
    public string ExtensionName { get; set; } = "name";

    public string InstalledVersion { get; set; } = "not installed";
    public string LatestVersion { get; set; } = string.Empty;
    public bool UpdateAvailable => !InstalledVersion.Equals(LatestVersion);
    public string Version => UpdateAvailable
        ? $"{InstalledVersion} -> {LatestVersion}"
        : InstalledVersion;

    public string WebsiteSlug { get; set; }
    public string RepositoryName => WebsiteSlug.Replace("-", "");

    public List<ReleaseAsset> Assets { get; set; } = new();

    public List<Install> Installs { get; set; } = new();

    public ExtensionType Type { get; set; }
    public string TypeName => Type.ToString("G");

    public VPExtension() { }

    public VPExtension(string creator, string extensionName, string websiteSlug, ExtensionType type)
    {
        Creator = creator;
        ExtensionName = extensionName;
        WebsiteSlug = websiteSlug;
        Type = type;
    }

    public string GetDownloadLink(string vpVersion)
    {
        if (string.IsNullOrEmpty(LatestVersion))
            return string.Empty;

        var assets = Assets.Where(x => x.Name.ToLower().Contains(RepositoryName.ToLower() + vpVersion));
        var extension = Type is ExtensionType.Extension ? ".zip" : ".dll";
        var result = assets.FirstOrDefault(x => x.Name.EndsWith(extension))?.BrowserDownloadUrl
            ?? "Download link not found";

        return result;
    }
}
