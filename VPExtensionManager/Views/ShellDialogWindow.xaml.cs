using System.Windows;
using System.Windows.Controls;

using MahApps.Metro.Controls;
using VPExtensionManager.Interfaces.Views;

namespace VPExtensionManager.Views;

public partial class ShellDialogWindow : MetroWindow, IShellDialogWindow
{
    public ShellDialogWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    public Frame GetDialogFrame()
        => dialogFrame;

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
