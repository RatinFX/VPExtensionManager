using Newtonsoft.Json;

namespace VPExtensionManager.Core.Models;

public class VPExtension
{
    // General
    public string Creator { get; set; } = "creator";
    public string ExtensionName { get; set; } = "name";
    public VPExtensionType Type { get; set; }

    // GitHub / latest Release
    [JsonIgnore]
    public bool RepositoryWasFound { get; set; } = true;
    public string InstalledVersion { get; set; } = "not installed";
    [JsonIgnore]
    public string LatestVersion { get; set; } = string.Empty;
    [JsonIgnore]
    public bool UpdateAvailable => RepositoryWasFound
        && !string.IsNullOrEmpty(LatestVersion)
        && !InstalledVersion.Equals(LatestVersion);

    [JsonIgnore]
    public string Version => !RepositoryWasFound ? LatestVersion
        : UpdateAvailable ? $"{InstalledVersion} -> {LatestVersion}"
        : InstalledVersion;

    [JsonIgnore]
    public List<ShortReleaseAsset> Assets { get; set; } = new();

    // Installed instances
    public List<VPInstall> Installs { get; set; } = new();

    public string[] ReferenceFiles { get; set; }

    // Buttons
    [JsonIgnore]
    public bool UpdateEnabled => UpdateAvailable;
    [JsonIgnore]
    public bool InstallEnabled => UpdateAvailable;
    [JsonIgnore]
    public bool UninstallEnabled => UpdateAvailable && Installs.Any();

    public VPExtension() { }
    public VPExtension(string creator, string extensionName, VPExtensionType type, string[] refereceFiles)
    {
        Creator = creator;
        ExtensionName = extensionName;
        Type = type;
        ReferenceFiles = refereceFiles;
    }

    public string GetDownloadLink(VPVersion vp)
    {
        if (string.IsNullOrEmpty(LatestVersion))
            return string.Empty;

        var assets = Assets.Where(x => x.Name.ToLower().Contains(ExtensionName.ToLower()) && x.VP == vp);
        var result = assets.FirstOrDefault(x => x.Name.EndsWith(Type.Extension))?.BrowserDownloadUrl
            ?? "Download link not found";

        return result;
    }
}
