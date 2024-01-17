using ToastNotifications.Core;
using VPExtensionManager.Contracts.Services;
using VPExtensionManager.Views;

namespace VPExtensionManager.Services;

public class NotificationService : INotificationService
{
    private readonly ToastViewModel _vm = new();

    private static void Show(Action<string, MessageOptions> show, string message)
    {
        var options = new MessageOptions
        {
            UnfreezeOnMouseLeave = true,
            NotificationClickAction = notif => notif.Close()
        };

        show(message, options);
    }

    public void Information(string message) => Show(_vm.ShowInformation, message);
    public void Success(string message) => Show(_vm.ShowSuccess, message);
    public void Warning(string message) => Show(_vm.ShowWarning, message);
    public void Error(string message) => Show(_vm.ShowError, message);
}
