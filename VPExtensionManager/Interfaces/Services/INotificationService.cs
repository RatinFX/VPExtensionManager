namespace VPExtensionManager.Interfaces.Services;

public interface INotificationService
{
    void Information(string message, Action click = null, bool close = false);
    void Success(string message, Action click = null, bool close = false);
    void Warning(string message, Action click = null, bool close = false);
    void Error(string message, Action click = null, bool close = false);
}
