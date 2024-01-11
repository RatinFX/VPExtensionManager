using System.Diagnostics;
using VPExtensionManager.Core.Contracts.Services;
using VPExtensionManager.Core.Models;

namespace VPExtensionManager.Core.Services;

public class ExtensionService : IExtensionService
{
    public static List<VPExtension> Extensions = new();

    public IEnumerable<VPExtension> InitializeExtensions()
    {
        if (Extensions.Any())
        {
            var shouldLocateInstalls = Extensions.All(x => !x.Installs.Any());

            foreach (var extension in Extensions)
            {
                SetLatestRelease(extension);
                SetInstallFolders(extension, false);
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

            extension.Assets = release.Assets
                .Where(x => x.BrowserDownloadUrl.EndsWith(VPExtensionType.Extension.FileExtension) || x.BrowserDownloadUrl.EndsWith(VPExtensionType.Script.FileExtension))
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

                /// TODO: properly look/ask for installed vegas versions
                // int[] localVersions = [14, 18, 19];
                // extension.Installs = GetInstallPaths(extension, localVersions);
                extension.Installs = GetInstallPaths(extension, []);
            }

            if (extension.Installs.Count > 1)
            {
                extension.InstalledVersion = "Multiple installs";
            }
            else if (extension.Installs.Count == 1)
            {
                extension.InstalledVersion = extension.Installs.First().Version;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception while locating \"{extension.ExtensionName}\": {ex.Message}");
            return;
        }
    }

    public List<VPInstall> GetInstallPaths(VPExtension extension, int[] localVersions)
    {
        var installs = new List<VPInstall>();

        var paths = extension.Type.Equals(VPExtensionType.Script) ? VPFolders.ScriptFolders(localVersions)
            : extension.Type.Equals(VPExtensionType.Extension) ? VPFolders.ExtensionFolders(localVersions)
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
                    //      .Where(x => extension.ReferenceFiles.Any(r => x.Contains(r));
                    // 
                    /// Show error
                    // if (files.Count() != extension.ReferenceFiles.Count()) { }

                    var vInfo = FileVersionInfo.GetVersionInfo(filePath);
                    var version = $"{vInfo.ProductMajorPart}.{vInfo.ProductMinorPart}.{vInfo.ProductBuildPart}";

                    installs.Add(new VPInstall(version, filePath));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception while locating \"{extension.ExtensionName}\" in: {path} - {ex.Message}");
            }
        }

        return installs;
    }
}
