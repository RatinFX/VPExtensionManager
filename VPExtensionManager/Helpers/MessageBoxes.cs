using System.Windows;

namespace VPExtensionManager.Helpers;

internal class MessageBoxes
{
    public static MessageBoxResult YesNo(string content, string title)
    {
        return MessageBox.Show(
            content,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Information,
            MessageBoxResult.No
        );
    }

    public static MessageBoxResult Error(Exception ex = null, string content = null, string title = null)
    {
        title ??= Properties.Resources.MessageBoxErrorTitle;

        var exMessage = ex != null
            ? ex.Message
            : Properties.Resources.MessageBoxErrorUnknownException;

        content = content == null
            ? exMessage
            : string.Format(content, exMessage);

        return MessageBox.Show(
            content,
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );
    }
}
