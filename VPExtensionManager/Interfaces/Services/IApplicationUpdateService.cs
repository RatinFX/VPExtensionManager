namespace VPExtensionManager.Interfaces.Services;

public interface IApplicationUpdateService
{
    bool ShouldCheckForUpdate();
    void SetLocalLatestVersion();
    string GetLatestVersion();
    void SendUpdateNotification(bool forceUpdate = false);
}
