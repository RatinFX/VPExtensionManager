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
    // General
    public string Creator { get; set; } = "creator";
    public string ExtensionName { get; set; } = "name";
    public ExtensionType Type { get; set; }
    public string TypeName => Type.ToString("G");

    // GitHub
    public string WebsiteSlug { get; set; }
    public string RepositoryName => WebsiteSlug.Replace("-", "");
    public bool RepositoryWasFound { get; set; } = true;

    // Release
    public string InstalledVersion { get; set; } = "not installed";
    public string LatestVersion { get; set; } = string.Empty;
    public bool UpdateAvailable => RepositoryWasFound
        && !string.IsNullOrEmpty(LatestVersion)
        && !InstalledVersion.Equals(LatestVersion);

    public string Version => !RepositoryWasFound ? LatestVersion
        : UpdateAvailable ? $"{InstalledVersion} -> {LatestVersion}"
        : InstalledVersion;

    public List<ReleaseAsset> Assets { get; set; } = new();

    // Installed instances
    public List<Install> Installs { get; set; } = new();

    // Buttons
    public bool UpdateEnabled => UpdateAvailable;
    public bool InstallEnabled => UpdateAvailable;
    public bool UninstallEnabled => UpdateAvailable && Installs.Any();

    public VPExtension() { }

    public VPExtension(string creator, string extensionName, ExtensionType type, string websiteSlug)
    {
        Creator = creator;
        ExtensionName = extensionName;
        Type = type;
        WebsiteSlug = websiteSlug;
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
