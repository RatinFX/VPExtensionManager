using MahApps.Metro.Controls;
using System.Windows.Controls;

namespace VPExtensionManager.Interfaces.Services;

public interface IRightPaneService
{
    event EventHandler PaneOpened;

    event EventHandler PaneClosed;

    void OpenInRightPane(Type pageType, object parameter = null);

    void Initialize(Frame rightPaneFrame, SplitView splitView);

    void CleanUp();
}
