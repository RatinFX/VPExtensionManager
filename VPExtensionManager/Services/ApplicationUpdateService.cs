using Microsoft.Extensions.Options;
using System.Diagnostics;
using VPExtensionManager.Helpers;
using VPExtensionManager.Interfaces.Services;
using VPExtensionManager.Models;
using VPExtensionManager.Views;

namespace VPExtensionManager.Services;

public class ApplicationUpdateService : IApplicationUpdateService
{
    private readonly IGitHubService _gitHubService;
    private readonly IApplicationInfoService _applicationInfoService;
    private readonly INotificationService _notificationService;
    private readonly IRightPaneService _rightPaneService;

    public ApplicationUpdateService(
        IGitHubService gitHubService,
        IApplicationInfoService applicationInfoService,
        INotificationService notificationService,
        IRightPaneService rightPaneService)
    {
        _gitHubService = gitHubService;
        _applicationInfoService = applicationInfoService;
        _notificationService = notificationService;
        _rightPaneService = rightPaneService;
    }

    public bool ShouldCheckForUpdate()
    {
        if (AppProperties.Get(AppProperties.CheckForUpdate, out string checkForUpdate))
        {
            return bool.TryParse(checkForUpdate, out bool res) && res;
        }

        return true;
    }

    public void SetLocalLatestVersion()
    {
        var latestVersion = _gitHubService.GetLatestRelease(RFXStrings.VPExtensionManager);

        if (latestVersion.Assets.Count != 0)
        {
            var first = latestVersion.Assets.FirstOrDefault(x =>
                x.Name == $"{RFXStrings.VPExtensionManager}{RFXStrings.Zip}"
            );

            AppProperties.Set(AppProperties.LatestVersionURL,
                latestVersion.Assets[0].BrowserDownloadUrl
            );
        }

        AppProperties.Set(AppProperties.LatestVersion,
            latestVersion.TagName
        );

        AppProperties.Set(AppProperties.LastChecked,
            DateTimeHelper.GetCurrentUnixTime().ToString()
        );
    }

    public string GetLatestVersion()
    {
        if (AppProperties.Get(AppProperties.LatestVersion, out string latestVersion))
        {
            return latestVersion;
        }

        return Properties.Resources.TextLatestVersionNotFound;
    }

    private static bool ShouldCheckForUpdate(string latestVersion)
    {
        if (string.IsNullOrEmpty(latestVersion) || latestVersion.Equals(Properties.Resources.TextLatestVersionNotFound))
        {
            return true;
        }

        var propExists = AppProperties.Get(AppProperties.LastChecked, out string lastChecked);

        var shouldCheck = string.IsNullOrEmpty(lastChecked)
            || DateTimeHelper.ShouldCheckForUpdate(long.Parse(lastChecked));

        return propExists && shouldCheck;
    }

    public void SendUpdateNotification(bool forceUpdate = false)
    {
        Debug.WriteLine($">>> Before App CheckForUpdate - Remaining API calls: {_gitHubService.GetRemainingCalls()}");

        try
        {
            var latestVersion = GetLatestVersion();

            // Locally saved LatestVersion requires update
            if (forceUpdate || ShouldCheckForUpdate(latestVersion))
            {
                SetLocalLatestVersion();
                latestVersion = GetLatestVersion();
            }

            // Latest not found
            if (latestVersion == Properties.Resources.TextLatestVersionNotFound)
            {
                var err = string.Format(Properties.Resources.NotificationErrorCheckingForLatestVersion,
                    latestVersion
                );

                _notificationService.Error(err);
                return;
            }

            // Update available
            if (_applicationInfoService.GetVersionShort() != latestVersion)
            {
                _notificationService.Information(
                    string.Format(Properties.Resources.NotificationInfoNewVersionAvailable, latestVersion),
                    () => _rightPaneService.OpenInRightPane(typeof(SettingsPage))
                );
                return;
            }

            // Using latest version
            _notificationService.Success(Properties.Resources.NotificationSuccessUsingLatestVersion);
        }
        catch (Exception ex)
        {
            ex = ex.GetBaseException();

            var msg = string.Format(
                Properties.Resources.NotificationErrorCheckingForLatestVersion,
                _gitHubService.GetRateLimitExceptionErrorMessage(ex)
            );

            Debug.WriteLine(msg);
            _notificationService.Error(msg);
        }

        Debug.WriteLine($">>> >>> After App CheckForUpdate - Remaining API calls: {_gitHubService.GetRemainingCalls()}");
        Debug.WriteLine($">>> >>> >>> Time until Rate Limit reset: {_gitHubService.GetRateLimitResetTime()}");
    }
}
