using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using VPExtensionManager.Helpers;
using VPExtensionManager.Interfaces.Services;
using VPExtensionManager.Interfaces.Views;
using VPExtensionManager.Models;

namespace VPExtensionManager.Views;

public partial class MainPage : Page, INotifyPropertyChanged, INavigationAware
{
    private readonly ISystemService _systemService;
    private readonly IExtensionService _extensionService;
    private readonly INotificationService _notificationService;
    private readonly IApplicationUpdateService _applicationUpdateService;
    private VPExtension _selected;
    public VPExtension Selected
    {
        get => _selected;
        set
        {
            Set(ref _selected, value);
            ResetInstallPaths();
            OnPropertyChanged(nameof(CheckForUpdateEnabled));
            OnPropertyChanged(nameof(InstallEnabled));
            OnPropertyChanged(nameof(UpdateNotesEnabled));
        }
    }

    public bool CheckForUpdateEnabled => Selected != null;
    public bool InstallEnabled => Selected != null && (Selected.RepositoryWasFound || Selected.UpdateAvailable);
    public bool UpdateNotesEnabled => Selected != null && Selected.RepositoryWasFound && !string.IsNullOrEmpty(Selected.LatestReleaseNotes);

    private VPInstall _selectedInstall;
    public VPInstall SelectedInstall
    {
        get => _selectedInstall;
        set
        {
            Set(ref _selectedInstall, value);
            OnPropertyChanged(nameof(OpenFolderEnabled));
            OnPropertyChanged(nameof(SelectedInstallFolder));
            OnPropertyChanged(nameof(UpdateEnabled));
            OnPropertyChanged(nameof(UninstallEnabled));
        }
    }

    public bool OpenFolderEnabled => SelectedInstall != null && !string.IsNullOrEmpty(SelectedInstall.InstallPath);
    public string SelectedInstallFolder => SelectedInstall != null ? SelectedInstall.InstallPath : string.Empty;
    public bool UpdateEnabled => Selected != null && SelectedInstall != null
        && Selected.UpdateAvailable
        && Selected.LatestVersion != SelectedInstall.Version;
    public bool UninstallEnabled => SelectedInstall != null;

    private ObservableCollection<VPExtension> _extensionItems = new();
    public ObservableCollection<VPExtension> ExtensionItems
    {
        get => _extensionItems;
        set => Set(ref _extensionItems, value);
    }

    private ObservableCollection<VPInstall> _installPaths = new();
    public ObservableCollection<VPInstall> InstallPaths
    {
        get => _installPaths;
        set => Set(ref _installPaths, value);
    }

    private string _toBeSelected;

    public MainPage(
        ISystemService systemService,
        IExtensionService extensionService,
        INotificationService notificationService,
        IApplicationUpdateService applicationUpdateService)
    {
        _systemService = systemService;
        _extensionService = extensionService;
        _notificationService = notificationService;
        _applicationUpdateService = applicationUpdateService;

        InitializeComponent();
        DataContext = this;

        if (applicationUpdateService.ShouldCheckForUpdate())
        {
            _applicationUpdateService.SendUpdateNotification();
        }

        AppLinkHandler.NavigateToExtensionHandler += NavigateToExtension;
    }

    private VPExtension GetExtensionByName(string extension)
    {
        return ExtensionItems?.FirstOrDefault(x =>
        {
            var lower = x.ExtensionName.ToLower();
            return lower.Equals(extension)
                || lower.StartsWith(extension) || lower.EndsWith(extension)
                || extension.StartsWith(lower) || extension.EndsWith(lower);
        });
    }

    private void NavigateToExtension(string extension)
    {
        _toBeSelected = extension;

        // Application was already initialized
        if (Selected != null)
        {
            var ext = GetExtensionByName(extension);
            if (ext == null)
                return;

            Selected = ext;
        }
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

    public void OnNavigatedFrom() { }

    public void OnNavigatedTo(object parameter)
    {
        ExtensionItems.Clear();

        var data = _extensionService.InitializeExtensions();

        foreach (var item in data)
        {
            ExtensionItems.Add(item);
        }

        Selected = _toBeSelected != null
            ? GetExtensionByName(_toBeSelected)
            : ExtensionItems.First();

        Selected ??= ExtensionItems.First();
    }

    private void ResetInstallPaths()
    {
        InstallPaths.Clear();
        Selected.Installs.ForEach(InstallPaths.Add);
    }

    private void btnCheckForUpdate_Click(object sender, RoutedEventArgs e)
    {
        var success = _extensionService.RefreshLatestRelease(Selected);
        if (!success)
            return;

        var msg = string.Format(Properties.Resources.NotificationSuccessUpdateAvailable,
            Selected.ExtensionName,
            Selected.LatestVersion
        );

        _notificationService.Success(msg);
    }

    private void btnFindInstalls_Click(object sender, RoutedEventArgs e)
    {
        _extensionService.RefreshInstallFolders(Selected);
        ResetInstallPaths();

        var msg = string.Format(Properties.Resources.NotificationSuccessFoundInstall,
            Selected.Installs.Count,
            Selected.ExtensionName
        );

        _notificationService.Success(msg);
    }

    private void btnInstall_Click(object sender, RoutedEventArgs e)
    {
        var window = new InstallWindow(Selected, _extensionService.GetAvailableFolders(Selected));
        var res = window.ShowDialog();

        if (res is null or false)
            return;

        var (vpver, installPath, forceDownload) = window.GetSelectedValues();
        if (vpver is VPVersion.Unknown || installPath is null)
            return;

        var newInstall = _extensionService.Install(Selected, vpver, installPath, forceDownload);
        if (newInstall == null)
            return;

        Selected.Installs.Add(newInstall);
        Selected.SetInstalledVersion();
        ResetInstallPaths();

        var msg = string.Format(Properties.Resources.NotificationSuccessInstalled,
            Selected.ExtensionName,
            Selected.LatestVersion
        );

        _notificationService.Success(msg);
    }

    private void btnUpdateNotes_Click(object sender, RoutedEventArgs e)
    {
        if (Selected == null)
            return;

        var window = new UpdateNotesWindow(_systemService, Selected);
        window.ShowDialog();
    }

    private void btnUpdate_Click(object sender, RoutedEventArgs e)
    {
        var window = new InstallWindow(Selected, SelectedInstall);
        var res = window.ShowDialog();
        if (res is null or false)
            return;

        var (vpver, installPath, forceDownload) = window.GetSelectedValues();
        if (vpver is VPVersion.Unknown || installPath is null)
            return;

        var success = _extensionService.Update(Selected, vpver, installPath, forceDownload);
        if (!success)
            return;

        _extensionService.RefreshInstallFolders(Selected);
        Selected.SetInstalledVersion();
        ResetInstallPaths();

        var msg = string.Format(Properties.Resources.NotificationSuccessUpdated,
            Selected.ExtensionName,
            Selected.LatestVersion
        );

        _notificationService.Success(msg);
    }

    private void btnUninstall_Click(object sender, RoutedEventArgs e)
    {
        // (?) TODO:
        // Create a custom window and make them toggleable
        // in case someone knows XYZ is used for another Extension
        var dependencies = string.Join("\n", Selected.Dependencies.Select(d => $"- {d}"));

        var dependencyText = string.IsNullOrEmpty(dependencies)
            ? string.Empty
            : string.Format(Properties.Resources.MessageBoxUninstallDependencyText, dependencies);

        var content = string.Format(Properties.Resources.MessageBoxUninstallContent,
            Selected.ExtensionName,
            SelectedInstall.InstallPath)
            + $"\n\n{dependencyText}";

        var title = string.Format(Properties.Resources.MessageBoxTitleUninstall,
            Selected.ExtensionName,
            SelectedInstall.Version
        );

        var result = MessageBox.Show(
            content,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No
        );

        if (result != MessageBoxResult.Yes)
            return;

        var success = _extensionService.Uninstall(Selected, SelectedInstall);
        if (!success)
            return;

        ResetInstallPaths();

        var msg = string.Format(Properties.Resources.NotificationSuccessUninstalled, Selected.ExtensionName);
        _notificationService.Success(msg);
    }
}
