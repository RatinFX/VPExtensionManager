using ToastNotifications.Core;
using VPExtensionManager.Interfaces.Services;
using VPExtensionManager.Views;

namespace VPExtensionManager.Services;

public class NotificationService : INotificationService
{
    private readonly ToastViewModel _vm = new();

    private static void Show(Action<string, MessageOptions> show, string message, Action click = null, bool close = true)
    {
        var options = new MessageOptions
        {
            UnfreezeOnMouseLeave = true,
            NotificationClickAction = notif =>
            {
                click?.Invoke();

                if (close)
                {
                    notif.Close();
                }
            }
        };

        show(message, options);
    }

    public void Information(string message, Action click = null, bool close = true) => Show(_vm.ShowInformation, message, click, close);
    public void Success(string message, Action click = null, bool close = true) => Show(_vm.ShowSuccess, message, click, close);
    public void Warning(string message, Action click = null, bool close = true) => Show(_vm.ShowWarning, message, click, close);
    public void Error(string message, Action click = null, bool close = true) => Show(_vm.ShowError, message, click, close);
}
