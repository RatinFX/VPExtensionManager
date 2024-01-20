using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using VPExtensionManager.Interfaces.Services;
using VPExtensionManager.Interfaces.Views;
using VPExtensionManager.Models;

namespace VPExtensionManager.Views;

public partial class MainPage : Page, INotifyPropertyChanged, INavigationAware
{
    private readonly IExtensionService _extensionService;
    private readonly INotificationService _notificationService;

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
        }
    }

    public bool CheckForUpdateEnabled => Selected != null && Selected.RepositoryWasFound;
    public bool InstallEnabled => Selected != null && (Selected.RepositoryWasFound || Selected.UpdateAvailable);

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
    public string SelectedInstallFolder => SelectedInstall != null ? Directory.GetParent(SelectedInstall.InstallPath).FullName : string.Empty;
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

    public MainPage(IExtensionService extensionService, INotificationService notificationService)
    {
        _extensionService = extensionService;
        _notificationService = notificationService;
        InitializeComponent();
        DataContext = this;
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

        Selected = ExtensionItems.First();
    }

    private void ResetInstallPaths()
    {
        InstallPaths.Clear();
        Selected.Installs.ForEach(x => InstallPaths.Add(x));
    }

    private void btnCheckForUpdate_Click(object sender, RoutedEventArgs e)
    {
        _extensionService.RefreshLatestRelease(Selected);

        _notificationService.Success($"Latest version for {Selected.ExtensionName}: {Selected.LatestVersion}");
    }

    private void btnFindInstalls_Click(object sender, RoutedEventArgs e)
    {
        _extensionService.RefreshInstallFolders(Selected);
        ResetInstallPaths();

        _notificationService.Success($"Found {Selected.Installs.Count}x {Selected.ExtensionName} install");
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

        _notificationService.Success($"Installed {Selected.ExtensionName} ({Selected.LatestVersion})");
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

        _extensionService.Update(Selected, vpver, installPath, forceDownload);
        _extensionService.RefreshInstallFolders(Selected);
        Selected.SetInstalledVersion();
        ResetInstallPaths();

        _notificationService.Success($"Updated {Selected.ExtensionName} to {Selected.LatestVersion}");
    }

    private void btnUninstall_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Create a custom window and make them toggleable
        // in case someone knows X.dll is used for another Extension
        var dependencies = string.Join("\n", Selected.Dependencies.Select(x => $"- {x}{RFXStrings.Dll}"));
        var dependencyText = string.IsNullOrEmpty(dependencies)
            ? ""
            : $"\n\nThis might also remove the following items:\n{dependencies}";

        var result = MessageBox.Show(
            $"Are you sure you want to remove {Selected.ExtensionName} from the following path?\n\n"
            + $"{SelectedInstall.InstallPath}" + dependencyText,
            $"Uninstall {Selected.ExtensionName} {SelectedInstall.Version}",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No
        );

        if (result == MessageBoxResult.No)
            return;

        if (result == MessageBoxResult.Yes)
        {
            _extensionService.Uninstall(Selected, SelectedInstall);
            ResetInstallPaths();

            _notificationService.Success($"Uninstalled {Selected.ExtensionName} from the selected path");
        }
    }
}
