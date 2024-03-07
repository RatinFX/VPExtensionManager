using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VPExtensionManager.Helpers;

internal class MessageBoxes
{
    public static MessageBoxResult Error(Exception ex, string content = null, string title = null)
    {
        title ??= Properties.Resources.MessageBoxErrorTitle;

        content = content == null
            ? ex.Message
            : string.Format(content, ex.Message);

        return MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
