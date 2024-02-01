using System.Diagnostics;
using VPExtensionManager.Helpers;
using VPExtensionManager.Interfaces.Services;
using VPExtensionManager.Models;

namespace VPExtensionManager.Services;

public class ApplicationUpdateService : IApplicationUpdateService
{
    private readonly IGitHubService _gitHubService;
    private readonly IApplicationInfoService _applicationInfoService;
    private readonly INotificationService _notificationService;

    public ApplicationUpdateService(
        IGitHubService gitHubService,
        IApplicationInfoService applicationInfoService,
        INotificationService notificationService)
    {
        _gitHubService = gitHubService;
        _applicationInfoService = applicationInfoService;
        _notificationService = notificationService;
    }

    public bool ShouldCheckForUpdate()
    {
        if (AppProperties.Get(AppProperties.CheckForUpdate, out string checkForUpdate))
        {
            return bool.Parse(checkForUpdate);
        }

        return true;
    }

    public void SetLocalLatestVersion()
    {
        AppProperties.Set(AppProperties.LatestVersion,
            _gitHubService.GetLatestRelease(RFXStrings.VPExtensionManager).TagName
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

        return "Latest version not found";
    }

    public void SendUpdateNotification(bool forceUpdate = false)
    {
        try
        {
            var latestVersion = GetLatestVersion();

            if (forceUpdate ||
                string.IsNullOrEmpty(latestVersion) || (
                AppProperties.Get(AppProperties.LastChecked, out string lastChecked)
                && DateTimeHelper.ShouldCheckForUpdate(long.Parse(lastChecked))
            ))
            {
                SetLocalLatestVersion();
            }

            var msg = _applicationInfoService.GetVersionShort() == latestVersion
                ? $"You're using the latest version"
                : $"New version available:\n" + latestVersion;

            _notificationService.Information(msg);
        }
        catch (Exception ex)
        {
            ex = ex.GetBaseException();

            var msg = "Error while checking the latest version for this app:\n"
                + "- " + _gitHubService.GetRateLimitExceptionErrorMessage(ex);

            Debug.WriteLine(msg);
            _notificationService.Error(msg);
        }
    }
}
