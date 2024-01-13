using System.Diagnostics;
using VPExtensionManager.Core.Contracts.Services;
using VPExtensionManager.Core.Models;

namespace VPExtensionManager.Core.Services;

public class ExtensionService : IExtensionService
{
    public static List<VPExtension> Extensions = new();

    private string _downloadPath = string.Empty;
    public List<string> ScriptFolders = new();
    public List<string> ExtensionFolders = new();

    public void SetConfigPath(string configPath)
    {
        _downloadPath = configPath;
    }

    public void SetPossibleFolders(List<int> localVersions)
    {
        ScriptFolders = VPFolders.ScriptFolders([.. localVersions]);
        ExtensionFolders = VPFolders.ExtensionFolders([.. localVersions]);
    }

    public IEnumerable<VPExtension> InitializeExtensions()
    {
        if (Extensions.Any())
        {
            var shouldLocateInstalls = Extensions.All(x => !x.Installs.Any());

            foreach (var extension in Extensions)
            {
                SetLatestRelease(extension);
                SetInstallFolders(extension, shouldLocateInstalls);
            }

            return Extensions;
        }

        Extensions =
        [
            new(RFXStrings.RatinFX, RFXStrings.VPConsole, VPExtensionType.Extension, RFXStrings.VPConsoleRefs),
            new(RFXStrings.RatinFX, RFXStrings.VegasProFlow, VPExtensionType.Extension, RFXStrings.VegasProFlowRefs),
            new(RFXStrings.RatinFX, RFXStrings.ShortenExtendMedia, VPExtensionType.Script, []),
            new(RFXStrings.RatinFX, RFXStrings.CustomFades, VPExtensionType.Script, []),
        ];

        foreach (var extension in Extensions)
        {
            SetLatestRelease(extension);
            SetInstallFolders(extension, true);
        }

        return Extensions;
    }

    public void RefreshLatestRelease(VPExtension extension)
    {
        SetLatestRelease(extension);
    }

    public void RefreshInstallFolders(VPExtension extension)
    {
        SetInstallFolders(extension, true);
    }

    private void SetLatestRelease(VPExtension extension)
    {
        try
        {
            var release = GitHubService.GetLatestRelease(extension.ExtensionName);

            extension.LatestVersion = release.TagName;

            extension.ReleaseAssets = release.Assets
                .Where(x => x.BrowserDownloadUrl.EndsWith(VPExtensionType.Extension.DownloadFileExtension)
                || x.BrowserDownloadUrl.EndsWith(VPExtensionType.Script.DownloadFileExtension))
                .Select(x => new ShortReleaseAsset(x, ShortReleaseAsset.GetVersion(x.Name, release.TagName)))
                .ToList();
        }
        catch (Exception ex)
        {
            extension.RepositoryWasFound = false;
            extension.LatestVersion = "GitHub error";
            Debug.WriteLine($"Exception with the extension \"{extension.ExtensionName}\": {ex.Message}");
            return;
        }
    }

    private void SetInstallFolders(VPExtension extension, bool shouldLocateInstalls)
    {
        if (!extension.RepositoryWasFound)
            return;

        try
        {
            if (shouldLocateInstalls)
            {
                extension.Installs.Clear();
                extension.Installs = GetInstallPaths(extension);
            }

            extension.SetInstalledVersion();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception while locating \"{extension.ExtensionName}\": {ex.Message}");
            return;
        }
    }

    private List<VPInstall> GetInstallPaths(VPExtension extension)
    {
        var installs = new List<VPInstall>();

        var paths = extension.Type.Equals(VPExtensionType.Script) ? ScriptFolders
            : extension.Type.Equals(VPExtensionType.Extension) ? ExtensionFolders
            : [];

        foreach (var path in paths)
        {
            try
            {
                var filePaths = Directory
                    .GetFiles(path, "*" + RFXStrings.Dll, SearchOption.AllDirectories)
                    .Where(x => x.Contains(extension.ExtensionName)
                );

                foreach (var filePath in filePaths)
                {
                    /// TODO: Find a reliable way to check or mark the VEGAS version of and Extension
                    /// - this only matters when we want to call Update:
                    //   > extension.GetDownloadLink(VPVersion..)
                    /// - maybe add "for Sony/Magix" or "13/14" in the File Description or smth

                    /// TODO: Find or Ask for installed versions of Vegas Pro for Folders
                    // var ver = localVersions.FirstOrDefault(x => path.Contains(x.ToString()));
                    // var vpver = ver == 0 ? VPVersion.Unknown
                    //     : ver >= (int)VPVersion.Magix ? VPVersion.Magix
                    //     : VPVersion.Sony;

                    /// TODO: We could also check if Dependencies are present in the given folder
                    /// and present information to the user when something is missing
                    // var files = Directory.GetFiles(Directory.GetParent(filePath))
                    //      .Where(x => extension.Dependencies.Any(r => x.Contains(r));
                    // 
                    /// Show error
                    // if (files.Count() != extension.Dependencies.Count()) { }

                    var vInfo = FileVersionInfo.GetVersionInfo(filePath);
                    var version = $"{vInfo.ProductMajorPart}.{vInfo.ProductMinorPart}.{vInfo.ProductBuildPart}";

                    installs.Add(new VPInstall(version, filePath));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception while locating \"{extension.ExtensionName}\" at: {path} - {ex.Message}");
            }
        }

        return installs;
    }

    public List<string> GetAvailableFolders(VPExtension extension)
    {
        return extension.Type.Equals(VPExtensionType.Extension)
            ? ExtensionFolders
            : ScriptFolders;
    }

    public void Uninstall(VPExtension selected, VPInstall selectedInstall)
    {
        try
        {
            var parent = Directory.GetParent(selectedInstall.InstallPath);

            if (File.Exists(selectedInstall.InstallPath))
                File.Delete(selectedInstall.InstallPath);

            var hasNoConflictingInstalls = Extensions
                .Where(x => x.ExtensionName != selected.ExtensionName)
                .All(x => x.Installs.All(y =>
                Directory.GetParent(y.InstallPath).FullName != parent.FullName)
            );

            if (hasNoConflictingInstalls)
            {
                foreach (var d in selected.Dependencies)
                {
                    var path = Path.Combine(parent.FullName, $"{d}{RFXStrings.Dll}");
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }

            selected.Installs.Remove(selectedInstall);
            selected.SetInstalledVersion();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception while uninstalling \"{selected.ExtensionName} ({selectedInstall.Version})\" from: {selectedInstall.InstallPath} - {ex.Message}");
        }
    }
}
