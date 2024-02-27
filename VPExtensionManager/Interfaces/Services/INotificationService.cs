namespace VPExtensionManager.Interfaces.Services;

public interface INotificationService
{
    void Information(string message, Action click = null);
    void Success(string message, Action click = null);
    void Warning(string message, Action click = null);
    void Error(string message, Action click = null);
}
