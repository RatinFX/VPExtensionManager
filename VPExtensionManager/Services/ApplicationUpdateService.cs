﻿using System.Diagnostics;
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

        return Properties.Resources.TextLatestVersionNotFound;
    }

    public void SendUpdateNotification(bool forceUpdate = false)
    {
        Debug.WriteLine($">>> Before App CheckForUpdate - Remaining API calls: {_gitHubService.GetRemainingCalls()}");

        try
        {
            var latestVersion = GetLatestVersion();

            // Locally saved LatestVersion requires update
            if (forceUpdate ||
                string.IsNullOrEmpty(latestVersion) || (
                AppProperties.Get(AppProperties.LastChecked, out string lastChecked)
                && DateTimeHelper.ShouldCheckForUpdate(long.Parse(lastChecked))
            ))
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
                _notificationService.Warning(string.Format(Properties.Resources.NotificationInfoNewVersionAvailable, latestVersion));
                return;
            }

            // Using latest version
            _notificationService.Information(Properties.Resources.NotificationInfoUsingLatestVersion);
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
