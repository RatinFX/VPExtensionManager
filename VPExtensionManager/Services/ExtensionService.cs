﻿using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using VPExtensionManager.Helpers;
using VPExtensionManager.Interfaces.Services;
using VPExtensionManager.Models;

namespace VPExtensionManager.Services;

public class ExtensionService : IExtensionService
{
    public static List<VPExtensionBase> Extensions = new();

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

    private bool DependenciesAreOK()
    {
        return Extensions.All(ext =>
        {
            var def = VPExtensionBase.DefaultExtensions.FirstOrDefault(d => ext.ExtensionName == d.ExtensionName);

            return def != null
                && def.Dependencies.Count == ext.Dependencies.Count
                && Enumerable.SequenceEqual(def.Dependencies.OrderBy(d => d), ext.Dependencies.OrderBy(d => d));
        });
    }

    public IEnumerable<VPExtensionBase> InitializeExtensions()
    {
        if (Extensions.Any() && DependenciesAreOK())
        {
            var shouldLocateInstalls = Extensions.All(x => !x.Installs.Any());

            foreach (var extension in Extensions)
            {
                if (extension.ShouldCheckForUpdate())
                {
                    SetLatestRelease(extension);
                }

                SetInstallFolders(extension, shouldLocateInstalls);
            }

            return Extensions;
        }

        Extensions = VPExtensionBase.DefaultExtensions;

        foreach (var extension in Extensions)
        {
            SetLatestRelease(extension);
            SetInstallFolders(extension, true);
        }

        return Extensions;
    }

    public bool RefreshLatestRelease(VPExtensionBase extension)
    {
        return SetLatestRelease(extension);
    }

    // TODO: move into Extension(?)
    public void RefreshInstallFolders(VPExtensionBase extension)
    {
        SetInstallFolders(extension, true);
    }

    private bool SetLatestRelease(VPExtensionBase extension)
    {
        Debug.WriteLine($">>> Before CheckForUpdate - Remaining API calls: {_gitHubService.GetRemainingCalls()}");

        var success = false;

        try
        {
            extension.LastChecked = DateTimeHelper.GetCurrentUnixTime();

            var release = _gitHubService.GetLatestRelease(extension.Creator, extension.ExtensionName);

            extension.LatestVersion = release.TagName;
            extension.LatestHtmlUrl = release.HtmlUrl;
            extension.LatestReleaseNotes = release.Body;

            extension.ReleaseAssets = release.Assets.Where(x =>
                x.BrowserDownloadUrl.EndsWith(VPExtensionType.Extension.DownloadFileExtension)
                || x.BrowserDownloadUrl.EndsWith(VPExtensionType.Script.DownloadFileExtension))
                .Select(x => new ShortReleaseAsset(x, ShortReleaseAsset.GetVersion(x.Name, release.TagName)))
                .ToList();

            extension.RepositoryWasFound = true;

            success = true;
        }
        catch (Exception ex)
        {
            extension.LatestVersion = Properties.Resources.TextGitHubError;
            extension.LatestHtmlUrl = string.Empty;
            extension.LatestReleaseNotes = string.Empty;

            extension.ReleaseAssets = [];

            extension.RepositoryWasFound = false;

            var msg = string.Format(Properties.Resources.NotificationErrorGitHubLookup, extension.ExtensionName);
            HandleExceptions(msg, ex);

            success = false;
        }

        Debug.WriteLine($">>> >>> After CheckForUpdate - Remaining API calls: {_gitHubService.GetRemainingCalls()}");
        Debug.WriteLine($">>> >>> >>> Time until Rate Limit reset: {_gitHubService.GetRateLimitResetTime()}");

        return success;
    }

    private void SetInstallFolders(VPExtensionBase extension, bool shouldLocateInstalls)
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
            var msg = string.Format(Properties.Resources.NotificationErrorSearchingInstall, extension.ExtensionName);
            HandleExceptions(msg, ex);
        }
    }

    private List<VPInstall> GetInstallPaths(VPExtensionBase extension)
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
                    /// TODO: We could also check if Dependencies are present in the given folder
                    /// and present information to the user when something is missing
                    // var files = Directory.GetFiles(Directory.GetParent(filePath))
                    //      .Where(x => extension.Dependencies.Any(r => x.Contains(r));

                    /// Show error
                    // if (files.Count() != extension.Dependencies.Count()) { }

                    var vInfo = FileVersionInfo.GetVersionInfo(filePath);

                    var version = $"{vInfo.ProductMajorPart}.{vInfo.ProductMinorPart}.{vInfo.ProductBuildPart}";

                    /// TODO: Find/Create a reliable way to check the VEGAS version of and Extension
                    /// (hint: there is none yet)
                    /// - this only matters when the user wants to Update:
                    //   > extension.GetDownloadLink(VPVersion..)
                    /// - could add "for Sony/Magix" or "13/14" in the Dll File's Description
                    var vp = vInfo.FileDescription.Contains(VPVersion.Magix.ToString("G")) ? VPVersion.Magix
                        : vInfo.FileDescription.Contains(VPVersion.Sony.ToString("G")) ? VPVersion.Sony
                        : VPVersion.Unknown;

                    var installPath = Directory.GetParent(filePath).FullName;

                    installs.Add(new VPInstall(version, installPath, vp));
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                var msg = string.Format("Error while locating \"{0}\" at:\r\n{1}",
                    extension.ExtensionName,
                    path
                );

                Debug.WriteLine(msg + "\n\n" + ex.Message);
            }
            catch (Exception ex)
            {
                var msg = string.Format(Properties.Resources.NotificationErrorSearchingInstallAtPath,
                    extension.ExtensionName,
                    path
                );

                HandleExceptions(msg, ex);
            }
        }

        return installs;
    }

    public List<string> GetAvailableFolders(VPExtensionBase extension)
    {
        return extension.Type.Equals(VPExtensionType.Extension)
            ? ExtensionFolders
            : ScriptFolders;
    }

    public bool PerformInstall(VPExtensionBase extension, VPVersion vp, string installPath, bool forceDownload)
    {
        var downloadLink = extension.GetDownloadLink(vp);
        if (downloadLink == null)
        {
            var msg = string.Format(Properties.Resources.NotificationErrorDownloadNotFoundForVP,
                extension.ExtensionName,
                vp
            );

            _notificationService.Error(msg);
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

        List<string> filesToOverwrite = [extension.ExtensionName + RFXStrings.Dll, .. extension.Dependencies];

        if (extension.Type.DownloadFileExtension == RFXStrings.Zip)
        {
            ZipFile.ExtractToDirectory(downloadPath, installPath, true);
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
    public VPInstall Install(VPExtensionBase extension, VPVersion vp, string installPath, bool forceDownload)
    {
        Debug.WriteLine($">>> Before Install - Remaining API calls: {_gitHubService.GetRemainingCalls()}");

        VPInstall newInstall = null;

        try
        {
            var success = PerformInstall(extension, vp, installPath, forceDownload);

            if (success)
            {
                newInstall = new VPInstall(extension.LatestVersion, installPath, vp);
            }
        }
        catch (Exception ex)
        {
            var msg = string.Format(Properties.Resources.NotificationErrorDuringInstall,
                extension.ExtensionName,
                extension.LatestVersion
            );

            HandleExceptions(msg, ex);
        }

        Debug.WriteLine($">>> >>> After CheckForUpdate - Remaining API calls: {_gitHubService.GetRemainingCalls()}");
        Debug.WriteLine($">>> >>> >>> Time until Rate Limit reset: {_gitHubService.GetRateLimitResetTime()}");

        return newInstall;
    }

    public bool Update(VPExtensionBase extension, VPVersion vp, string installPath, bool forceDownload)
    {
        Debug.WriteLine($">>> Before Update - Remaining API calls: {_gitHubService.GetRemainingCalls()}");

        bool success;

        try
        {
            success = PerformInstall(extension, vp, installPath, forceDownload);
        }
        catch (Exception ex)
        {
            var msg = string.Format(Properties.Resources.NotificationErrorDuringUpdate,
                extension.ExtensionName,
                extension.LatestVersion
            );

            HandleExceptions(msg, ex);

            success = false;
        }

        Debug.WriteLine($">>> >>> After Update - Remaining API calls: {_gitHubService.GetRemainingCalls()}");
        Debug.WriteLine($">>> >>> >>> Time until Rate Limit reset: {_gitHubService.GetRateLimitResetTime()}");

        return success;
    }

    public bool Uninstall(VPExtensionBase extension, VPInstall selectedInstall)
    {
        try
        {
            // Main Extension
            var foundFile = Directory.GetFiles(selectedInstall.InstallPath, "*" + RFXStrings.Dll)
                .Select(Path.GetFileName)
                .FirstOrDefault(x => x.StartsWith(extension.ExtensionName, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(foundFile))
            {
                var filePath = Path.Combine(selectedInstall.InstallPath, foundFile);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            // Remaining Dependencies
            var hasNoConflictingInstalls = Extensions
                .Where(x => x.ExtensionName != extension.ExtensionName)
                .All(x => x.Installs.All(y => y.InstallPath != selectedInstall.InstallPath)
            );

            if (hasNoConflictingInstalls && extension.Dependencies.Any())
            {
                foreach (var dependency in extension.Dependencies)
                {
                    var path = Path.Combine(selectedInstall.InstallPath, dependency);

                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }

            extension.Installs.Remove(selectedInstall);
            extension.SetInstalledVersion();

            return true;
        }
        catch (Exception ex)
        {
            var msg = string.Format(Properties.Resources.NotificationErrorDuringUninstall,
                extension.ExtensionName,
                selectedInstall.Version
            );

            HandleExceptions(msg, ex);

            return false;
        }
    }

    private void HandleExceptions(string details, Exception ex)
    {
        ex = ex.GetBaseException();

        var msg = $"{details}\n"
            + $"- {GitHubService.GetRateLimitExceptionErrorMessage(ex)}";

        Debug.WriteLine(msg);
        _notificationService.Error(msg);
    }
}
