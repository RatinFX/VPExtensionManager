﻿using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VPExtensionManager.Helpers;

namespace VPExtensionManager.Models;

public class VPExtensionBase : INotifyPropertyChanged
{
    // General
    public virtual string Creator { get; set; } = "creator";
    public virtual string ExtensionName { get; set; } = "name";
    public virtual VPExtensionType Type { get; set; }

    // GitHub / latest Release
    public string LatestHtmlUrl { get; set; } = string.Empty;
    public string LatestReleaseNotes { get; set; } = string.Empty;
    public bool RepositoryWasFound { get; set; } = true;

    [JsonIgnore]
    private string _installedVersion = Properties.Resources.TextNotInstalled;
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

    [JsonIgnore]
    private string _latestVersion = string.Empty;
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

    public long LastChecked { get; set; } = -1;

    [JsonIgnore]
    public virtual bool UpdateAvailable =>
        RepositoryWasFound
        && !string.IsNullOrEmpty(LatestVersion)
        && InstalledVersion != LatestVersion
        && (!Installs.Any() || Installs.Any(x => x.Version != LatestVersion));

    [JsonIgnore]
    public virtual string VersionDisplay =>
        !RepositoryWasFound ? $"{InstalledVersion} ({LatestVersion})"
        : UpdateAvailable ? $"{InstalledVersion} -> {LatestVersion}"
        : InstalledVersion;

    public List<ShortReleaseAsset> ReleaseAssets { get; set; } = new();

    // Installed instances
    public List<VPInstall> Installs { get; set; } = new();

    public List<string> Dependencies { get; set; } = [];

    public VPExtensionBase() { }
    public VPExtensionBase(string creator, string extensionName, VPExtensionType type, List<string> dependencies)
    {
        Creator = creator;
        ExtensionName = extensionName;
        Type = type;
        Dependencies = dependencies;
    }

    public virtual void SetInstalledVersion()
    {
        InstalledVersion =
            !Installs.Any() ? Properties.Resources.TextNotInstalled
            : Installs.Count > 1 ? Properties.Resources.TextMultipleInstalls
            : Installs.FirstOrDefault()?.Version
            ?? Properties.Resources.TextUnknownError;
    }

    public virtual bool ShouldCheckForUpdate()
    {
        return string.IsNullOrEmpty(LatestVersion)
            //|| !ReleaseAssets.Any()
            || DateTimeHelper.ShouldCheckForUpdate(LastChecked);
    }

    public virtual string GetDownloadLink(VPVersion vp)
    {
        if (string.IsNullOrEmpty(LatestVersion))
        {
            return string.Empty;
        }

        var result = ReleaseAssets.FirstOrDefault(x => x.VP == vp && x.Name.EndsWith(Type.DownloadFileExtension))
            ?.BrowserDownloadUrl
            ?? Properties.Resources.ErrorDownloadLinkNotFound;

        return result;
    }

    // TODO: add Install, Update, Uninstall

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    private void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (Equals(storage, value))
        {
            return;
        }

        storage = value;
        OnPropertyChanged(propertyName);
    }

    public static VPExtensionBase VPConsole { get; } = new(RFXStrings.RatinFX, RFXStrings.VPConsole, VPExtensionType.Extension, RFXStrings.VPConsoleRefs);
    public static VPExtensionBase VPFlow { get; } = new(RFXStrings.RatinFX, RFXStrings.VegasProFlow, VPExtensionType.Extension, RFXStrings.VegasProFlowRefs);
    public static VPExtensionBase ShortenExtendMedia { get; } = new(RFXStrings.RatinFX, RFXStrings.ShortenExtendMedia, VPExtensionType.Script, []);
    public static VPExtensionBase CustomFades { get; } = new(RFXStrings.RatinFX, RFXStrings.CustomFades, VPExtensionType.Script, []);

    public static List<VPExtensionBase> DefaultExtensions = [VPConsole, VPFlow, ShortenExtendMedia, CustomFades];
}
