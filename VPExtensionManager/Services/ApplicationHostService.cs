using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.IO;
using VPExtensionManager.Contracts.Activation;
using VPExtensionManager.Contracts.Services;
using VPExtensionManager.Contracts.Views;
using VPExtensionManager.Models;
using VPExtensionManager.Views;

namespace VPExtensionManager.Services;

public class ApplicationHostService : IHostedService
{
    private readonly AppConfig _appConfig;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly INavigationService _navigationService;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILocalVPVersionService _localVPVersionService;
    private readonly IRightPaneService _rightPaneService;
    private readonly IPersistAndRestoreService _persistAndRestoreService;
    private readonly IExtensionService _extensionService;
    private readonly IFolderService _folderService;
    private IShellWindow _shellWindow;
    private bool _isInitialized;

    public ApplicationHostService(IOptions<AppConfig> appConfig,
        IServiceProvider serviceProvider,
        IEnumerable<IActivationHandler> activationHandlers,
        INavigationService navigationService,
        IThemeSelectorService themeSelectorService,
        ILocalVPVersionService localVPVersionService,
        IRightPaneService rightPaneService,
        IPersistAndRestoreService persistAndRestoreService,
        IExtensionService extensionService,
        IFolderService folderService)
    {
        _appConfig = appConfig.Value;
        _serviceProvider = serviceProvider;
        _activationHandlers = activationHandlers;
        _navigationService = navigationService;
        _themeSelectorService = themeSelectorService;
        _localVPVersionService = localVPVersionService;
        _rightPaneService = rightPaneService;
        _persistAndRestoreService = persistAndRestoreService;
        _extensionService = extensionService;
        _folderService = folderService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Initialize services that you need before app activation
        await InitializeAsync();

        await HandleActivationAsync();

        // Tasks after activation
        await StartupAsync();
        _isInitialized = true;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _persistAndRestoreService.PersistData();
        await Task.CompletedTask;
    }

    private async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            _persistAndRestoreService.RestoreData();
            _themeSelectorService.InitializeTheme();

            _extensionService.SetDownloadsPath(_folderService.GetDownloadsFolder());
            _extensionService.SetPossibleFolders(_localVPVersionService.GetLocalVersions());

            await Task.CompletedTask;
        }
    }

    private async Task StartupAsync()
    {
        if (!_isInitialized)
        {
            await Task.CompletedTask;
        }
    }

    private async Task HandleActivationAsync()
    {
        var activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle());

        if (activationHandler != null)
        {
            await activationHandler.HandleAsync();
        }

        await Task.CompletedTask;

        if (App.Current.Windows.OfType<IShellWindow>().Count() == 0)
        {
            // Default activation that navigates to the apps default page
            _shellWindow = _serviceProvider.GetService(typeof(IShellWindow)) as IShellWindow;
            _navigationService.Initialize(_shellWindow.GetNavigationFrame());
            _rightPaneService.Initialize(_shellWindow.GetRightPaneFrame(), _shellWindow.GetSplitView());
            _shellWindow.ShowWindow();
            _navigationService.NavigateTo(typeof(MainPage));
            await Task.CompletedTask;
        }
    }
}
