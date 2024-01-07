using System.Diagnostics;
using VPExtensionManager.Core.Contracts.Services;
using VPExtensionManager.Core.Models;

namespace VPExtensionManager.Core.Services;

public class ExtensionService : IExtensionService
{
    public static List<VPExtension> Extensions = new();

    public async Task<IEnumerable<VPExtension>> InitializeExtensions()
    {
        if (Extensions.Any())
        {
            RefreshExtensionInfo();
            return Extensions;
        }

        Extensions =
        [
            new VPExtension(RFXStrings.RatinFX, RFXStrings.VPConsole, VPExtensionType.Zip, RFXStrings.VPConsoleRefs),
            new VPExtension(RFXStrings.RatinFX, RFXStrings.VegasProFlow, VPExtensionType.Zip, RFXStrings.VegasProFlowRefs),
            new VPExtension(RFXStrings.RatinFX, RFXStrings.ShortenExtendMedia, VPExtensionType.Dll, []),
            new VPExtension(RFXStrings.RatinFX, RFXStrings.CustomFades, VPExtensionType.Dll, []),
        ];

        RefreshExtensionInfo();

        await Task.CompletedTask;
        return Extensions;
    }

    private void RefreshExtensionInfo()
    {
        foreach (var extension in Extensions)
        {
            GetLatestRelease(extension);
            LocateInstalls(extension);
        }
    }

    private void GetLatestRelease(VPExtension extension)
    {
        try
        {
            var release = GitHubService.GetLatestRelease(extension.ExtensionName);

            extension.LatestVersion = release.TagName;

            extension.Assets = release.Assets
                .Where(x => x.BrowserDownloadUrl.EndsWith(VPExtensionType.Zip.Extension) || x.BrowserDownloadUrl.EndsWith(VPExtensionType.Dll.Extension))
                .Select(x => new ShortReleaseAsset(x, ShortReleaseAsset.GetVersion(x.Name, release.TagName)))
                .ToList();
        }
        catch (Exception ex)
        {
            extension.RepositoryWasFound = false;
            extension.LatestVersion = "GitHub lookup error";
            Debug.WriteLine($"Exception with the extension \"{extension.ExtensionName}\": {ex.Message}");
            return;
        }
    }

    private void LocateInstalls(VPExtension extension)
    {
        try
        {
            extension.Installs.Clear();

            // placeholder install locations
            extension.Installs.Add(new VPInstall
            {
                VPVersion = VPVersion.Sony,
                InstallPath = @"D:\Folder-14\somewhere\else\on\this\pc"
            });

            extension.Installs.Add(new VPInstall
            {
                VPVersion = VPVersion.Magix,
                // DL link test in the mean time
                InstallPath = extension.GetDownloadLink(VPVersion.Magix)
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception with locating the extension \"{extension.ExtensionName}\": {ex.Message}");
            return;
        }
    }
}
