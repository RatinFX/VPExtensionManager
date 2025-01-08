namespace VPExtensionManager.Interfaces.Services;

public interface INotificationService
{
    void Information(string message, Action click = null, bool close = true);
    void Success(string message, Action click = null, bool close = true);
    void Warning(string message, Action click = null, bool close = true);
    void Error(string message, Action click = null, bool close = true);
}
