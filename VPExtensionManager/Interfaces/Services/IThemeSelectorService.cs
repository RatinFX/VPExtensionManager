using VPExtensionManager.Models;

namespace VPExtensionManager.Interfaces.Services;

public interface IThemeSelectorService
{
    void InitializeTheme();

    void SetTheme(AppTheme theme);

    AppTheme GetCurrentTheme();
}
