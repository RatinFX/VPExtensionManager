using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Lifetime.Clear;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace VPExtensionManager.Views
{
    public class ToastViewModel : INotifyPropertyChanged
    {
        private readonly Notifier _notifier;

        public ToastViewModel()
        {
            _notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: Application.Current.MainWindow,
                    corner: Corner.TopRight,
                    offsetX: 20,
                    offsetY: 50
                );

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(10),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(5)
                );

                cfg.Dispatcher = Application.Current.Dispatcher;

                cfg.DisplayOptions.TopMost = false;
                cfg.DisplayOptions.Width = 250;
            });

            _notifier.ClearMessages(new ClearAll());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return;

            storage = value;
            OnPropertyChanged(propertyName);
        }

        public void OnUnloaded() => _notifier.Dispose();

        public void ShowInformation(string message) => _notifier.ShowInformation(message);
        public void ShowSuccess(string message) => _notifier.ShowSuccess(message);
        public void ShowWarning(string message) => _notifier.ShowWarning(message);
        public void ShowError(string message) => _notifier.ShowError(message);

        public void ShowInformation(string message, MessageOptions options) => _notifier.ShowInformation(message, options);
        public void ShowSuccess(string message, MessageOptions options) => _notifier.ShowSuccess(message, options);
        public void ShowWarning(string message, MessageOptions options) => _notifier.ShowWarning(message, options);
        public void ShowError(string message, MessageOptions options) => _notifier.ShowError(message, options);

        public void ClearMessages(string message) => _notifier.ClearMessages(new ClearByMessage(message));
        public void ClearAll() => _notifier.ClearMessages(new ClearAll());
    }

}
