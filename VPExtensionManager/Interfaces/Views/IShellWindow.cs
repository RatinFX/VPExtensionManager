using MahApps.Metro.Controls;
using System.Windows.Controls;

namespace VPExtensionManager.Interfaces.Views;

public interface IShellWindow
{
    Frame GetNavigationFrame();

    void ShowWindow();

    void CloseWindow();

    Frame GetRightPaneFrame();

    SplitView GetSplitView();
}
