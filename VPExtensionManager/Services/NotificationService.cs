using ToastNotifications.Core;
using VPExtensionManager.Interfaces.Services;
using VPExtensionManager.Views;

namespace VPExtensionManager.Services;

public class NotificationService : INotificationService
{
    private readonly ToastViewModel _vm = new();

    private static void Show(Action<string, MessageOptions> show, string message, Action click = null)
    {
        var options = new MessageOptions
        {
            UnfreezeOnMouseLeave = true,
            NotificationClickAction = notif =>
            {
                if (click == null) notif.Close();
                else click.Invoke();
            }
        };

        show(message, options);
    }

    public void Information(string message, Action click = null) => Show(_vm.ShowInformation, message, click);
    public void Success(string message, Action click = null) => Show(_vm.ShowSuccess, message, click);
    public void Warning(string message, Action click = null) => Show(_vm.ShowWarning, message, click);
    public void Error(string message, Action click = null) => Show(_vm.ShowError, message, click);
}
