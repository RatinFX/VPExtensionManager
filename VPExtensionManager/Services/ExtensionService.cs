using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using VPExtensionManager.Helpers;
using VPExtensionManager.Interfaces.Services;
using VPExtensionManager.Models;

namespace VPExtensionManager.Services;

public class ExtensionService : IExtensionService
{
    public static List<VPExtension> Extensions = new();

    private readonly INotificationService _notificationService;
    private readonly IGitHubService _gitHubService;

    private string _downloadsPath = string.Empty;
    public List<string> ScriptFolders = new();
    public List<string> ExtensionFolders = new();

    public ExtensionService(INotificationService notificationService, IGitHubService gitHubService)
    {
        _notificationService = notificationService;
        _gitHubService = gitHubService;
    }

    public void SetDownloadsPath(string downloadsPath)
    {
        _downloadsPath = downloadsPath;
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
                if (extension.ShouldCheckForUpdate())
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

    public bool RefreshLatestRelease(VPExtension extension)
    {
        return SetLatestRelease(extension);
    }

    public void RefreshInstallFolders(VPExtension extension)
    {
        SetInstallFolders(extension, true);
    }

    private bool SetLatestRelease(VPExtension extension)
    {
        Debug.WriteLine($">>> Before CheckForUpdate - Remaining API calls: {_gitHubService.GetRemainingCalls()}");

        var success = false;

        try
        {
            extension.LastChecked = DateTimeHelper.GetCurrentUnixTime();

            var release = _gitHubService.GetLatestRelease(extension.ExtensionName);

            extension.LatestVersion = release.TagName;

            extension.ReleaseAssets = release.Assets.Where(x =>
                x.BrowserDownloadUrl.EndsWith(VPExtensionType.Extension.DownloadFileExtension)
                || x.BrowserDownloadUrl.EndsWith(VPExtensionType.Script.DownloadFileExtension))
                .Select(x => new ShortReleaseAsset(x, ShortReleaseAsset.GetVersion(x.Name, release.TagName)))
                .ToList();

            success = true;
        }
        catch (Exception ex)
        {
            extension.RepositoryWasFound = false;
            extension.LatestVersion = "GitHub error";

            var msg = $"Error while looking up \"{extension.ExtensionName}\" on GitHub:";
            HandleExceptions(msg, ex);

            success = false;
        }

        Debug.WriteLine($">>> >>> After CheckForUpdate - Remaining API calls: {_gitHubService.GetRemainingCalls()}");
        Debug.WriteLine($">>> >>> >>> Time until Rate Limit reset: {_gitHubService.GetRateLimitResetTime()}");

        return success;
    }

    private void SetInstallFolders(VPExtension extension, bool shouldLocateInstalls)
    {
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
            var msg = $"Error while searching for \"{extension.ExtensionName}\":";
            HandleExceptions(msg, ex);
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
                    /// TODO: Find/Create a reliable way to check the VEGAS version of and Extension
                    /// (hint: there is none yet)
                    /// - this only matters when the user wants to Update:
                    //   > extension.GetDownloadLink(VPVersion..)
                    /// - could add "for Sony/Magix" or "13/14" in the Dll File's Description

                    /// TODO: We could also check if Dependencies are present in the given folder
                    /// and present information to the user when something is missing
                    // var files = Directory.GetFiles(Directory.GetParent(filePath))
                    //      .Where(x => extension.Dependencies.Any(r => x.Contains(r));

                    /// Show error
                    // if (files.Count() != extension.Dependencies.Count()) { }

                    var vInfo = FileVersionInfo.GetVersionInfo(filePath);
                    var version = $"{vInfo.ProductMajorPart}.{vInfo.ProductMinorPart}.{vInfo.ProductBuildPart}";
                    var installPath = Directory.GetParent(filePath).FullName;

                    installs.Add(new VPInstall(version, installPath));
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                Debug.WriteLine($"Error while locating \"{extension.ExtensionName}\" at:\n" +
                    $"{path}\n\n" +
                    $"{ex.Message}");
            }
            catch (Exception ex)
            {
                var msg = $"Error while searching in \"{extension.ExtensionName}\" at:\n" + path;
                HandleExceptions(msg, ex);
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

    private bool PerformInstall(VPExtension extension, VPVersion vp, string installPath, bool forceDownload)
    {
        var downloadLink = extension.GetDownloadLink(vp);
        if (downloadLink == null)
        {
            _notificationService.Error($"Did not find a download link for {extension.ExtensionName} for version {vp}");
            return false;
        }

        var fileName = Path.GetFileName(downloadLink);
        var downloadPath = Path.Combine(_downloadsPath, fileName);

        // Download
        if (!File.Exists(downloadPath) || forceDownload)
        {
            using var client = new HttpClient();
            var bytes = client.GetByteArrayAsync(downloadLink).Result;

            Directory.CreateDirectory(_downloadsPath);

            File.WriteAllBytes(downloadPath, bytes);
        }

        // Extract
        Directory.CreateDirectory(installPath);

        List<string> filesToOverwrite = [extension.ExtensionName, .. extension.Dependencies];

        // - delete existing files as `ZipFile.ExtractToDirectory` cannot overwrite them
        if (extension.Type.DownloadFileExtension == RFXStrings.Zip)
        {
            foreach (var item in filesToOverwrite)
            {
                var file = Path.Combine(installPath, $"{item}{RFXStrings.Dll}");

                if (File.Exists(file))
                    File.Delete(file);
            }

            ZipFile.ExtractToDirectory(downloadPath, installPath);
        }
        else
        {
            File.Copy(downloadPath, Path.Combine(installPath, fileName), true);
        }

        return true;
    }

    /// <summary>
    /// TODO: Potentially generalize all of these
    /// and let the Extension handle Install, etc.
    /// This way it could have "Custom" extensions
    /// </summary>
    public VPInstall Install(VPExtension extension, VPVersion vp, string installPath, bool forceDownload)
    {
        Debug.WriteLine($">>> Before Install - Remaining API calls: {_gitHubService.GetRemainingCalls()}");

        VPInstall newInstall = null;

        try
        {
            var success = PerformInstall(extension, vp, installPath, forceDownload);

            if (success)
                newInstall = new VPInstall(extension.LatestVersion, installPath);
        }
        catch (Exception ex)
        {
            var msg = $"Error while installing \"{extension.ExtensionName} ({extension.LatestVersion})\":";
            HandleExceptions(msg, ex);
        }

        Debug.WriteLine($">>> >>> After CheckForUpdate - Remaining API calls: {_gitHubService.GetRemainingCalls()}");
        Debug.WriteLine($">>> >>> >>> Time until Rate Limit reset: {_gitHubService.GetRateLimitResetTime()}");

        return newInstall;
    }

    public bool Update(VPExtension extension, VPVersion vp, string installPath, bool forceDownload)
    {
        Debug.WriteLine($">>> Before Update - Remaining API calls: {_gitHubService.GetRemainingCalls()}");

        bool success = false;

        try
        {
            success = PerformInstall(extension, vp, installPath, forceDownload);
        }
        catch (Exception ex)
        {
            var msg = $"Error while updating \"{extension.ExtensionName} ({extension.LatestVersion})\":";
            HandleExceptions(msg, ex);

            success = false;
        }

        Debug.WriteLine($">>> >>> After Update - Remaining API calls: {_gitHubService.GetRemainingCalls()}");
        Debug.WriteLine($">>> >>> >>> Time until Rate Limit reset: {_gitHubService.GetRateLimitResetTime()}");

        return success;
    }

    public void Uninstall(VPExtension extension, VPInstall selectedInstall)
    {
        try
        {
            // Main Extension
            var foundFile = Directory.GetFiles(selectedInstall.InstallPath, "*" + RFXStrings.Dll)
                .Select(Path.GetFileName)
                .FirstOrDefault(x => x.StartsWith(extension.ExtensionName, StringComparison.OrdinalIgnoreCase));

            var filePath = Path.Combine(selectedInstall.InstallPath, foundFile);

            if (File.Exists(filePath))
                File.Delete(filePath);

            // Remaining Dependencies
            var hasNoConflictingInstalls = Extensions
                .Where(x => x.ExtensionName != extension.ExtensionName)
                .All(x => x.Installs.All(y => y.InstallPath != selectedInstall.InstallPath)
            );

            if (hasNoConflictingInstalls)
            {
                foreach (var dependency in extension.Dependencies)
                {
                    var path = Path.Combine(selectedInstall.InstallPath, $"{dependency}{RFXStrings.Dll}");

                    if (File.Exists(path))
                        File.Delete(path);
                }
            }

            extension.Installs.Remove(selectedInstall);
            extension.SetInstalledVersion();
        }
        catch (Exception ex)
        {
            var msg = $"Error while uninstalling \"{extension.ExtensionName} ({selectedInstall.Version})\" from:\n" + selectedInstall.InstallPath;
            HandleExceptions(msg, ex);
        }
    }

    private void HandleExceptions(string details, Exception ex)
    {
        ex = ex.GetBaseException();

        var msg = details + "\n" + "- " + _gitHubService.GetRateLimitExceptionErrorMessage(ex);

        Debug.WriteLine(msg);
        _notificationService.Error(msg);
    }
}
