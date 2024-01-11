using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VPExtensionManager.Core.Models;

public class VPExtension : INotifyPropertyChanged
{
    // General
    public string Creator { get; set; } = "creator";
    public string ExtensionName { get; set; } = "name";
    public VPExtensionType Type { get; set; }

    // GitHub / latest Release
    [JsonIgnore]
    public bool RepositoryWasFound { get; set; } = true;

    private string _installedVersion = "not installed";
    public string InstalledVersion
    {
        get => _installedVersion;
        set
        {
            Set(ref _installedVersion, value);
            OnPropertyChanged(nameof(InstalledVersion));
            OnPropertyChanged(nameof(UpdateAvailable));
            OnPropertyChanged(nameof(VersionDisplay));
        }
    }

    private string _latestVersion = string.Empty;
    [JsonIgnore]
    public string LatestVersion
    {
        get => _latestVersion;
        set
        {
            Set(ref _latestVersion, value);
            OnPropertyChanged(nameof(LatestVersion));
            OnPropertyChanged(nameof(UpdateAvailable));
            OnPropertyChanged(nameof(VersionDisplay));
        }
    }

    [JsonIgnore]
    public bool UpdateAvailable =>
        RepositoryWasFound
        && !string.IsNullOrEmpty(LatestVersion)
        && InstalledVersion != LatestVersion
        && Installs.Any(x => x.Version != LatestVersion);

    [JsonIgnore]
    public string VersionDisplay =>
        !RepositoryWasFound ? $"{InstalledVersion} ({LatestVersion})"
        : UpdateAvailable ? $"{InstalledVersion} -> {LatestVersion}"
        : InstalledVersion;

    [JsonIgnore]
    public List<ShortReleaseAsset> Assets { get; set; } = new();

    // Installed instances
    public List<VPInstall> Installs { get; set; } = new();

    public List<string> ReferenceFiles { get; set; } = [];

    public VPExtension() { }
    public VPExtension(string creator, string extensionName, VPExtensionType type, List<string> refereceFiles)
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
        var result = assets.FirstOrDefault(x => x.Name.EndsWith(Type.DownloadFileExtension))?.BrowserDownloadUrl
            ?? "Download link not found";

        return result;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    private void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (Equals(storage, value))
            return;

        storage = value;
        OnPropertyChanged(propertyName);
    }
}
