using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

using VPExtensionManager.Contracts.Views;
using VPExtensionManager.Core.Contracts.Services;
using VPExtensionManager.Core.Models;

namespace VPExtensionManager.Views;

public partial class MainPage : Page, INotifyPropertyChanged, INavigationAware
{
    private readonly IExtensionService _extensionService;

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

    public bool CheckForUpdateEnabled => Selected.RepositoryWasFound;
    public bool InstallEnabled => Selected.RepositoryWasFound || Selected.UpdateAvailable;

    private VPInstall _selectedInstall;
    public VPInstall SelectedInstall
    {
        get => _selectedInstall;
        set
        {
            Set(ref _selectedInstall, value);
            OnPropertyChanged(nameof(UpdateEnabled));
            OnPropertyChanged(nameof(EditEnabled));
            OnPropertyChanged(nameof(UninstallEnabled));
        }
    }

    public bool UpdateEnabled => SelectedInstall != null && _selected.UpdateAvailable && _selected.Installs.Any();
    public bool EditEnabled => SelectedInstall != null;
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

    public MainPage(IExtensionService extensionService)
    {
        _extensionService = extensionService;
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
    }

    private void btnFindInstalls_Click(object sender, RoutedEventArgs e)
    {
        _extensionService.RefreshInstallFolders(Selected);
        ResetInstallPaths();
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
    }

    private void btnUpdate_Click(object sender, RoutedEventArgs e)
    {
        // Update(Selected) - select-install window -> download zip/dll, extract, get path via installs.vpversion
    }

    private void btnEditInstallPath_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Reconsider if we need Edit or not
        // Edit(SelectedInstall) - edit window
    }

    private void btnUninstallInstallPath_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            $"Are you sure you want to uninstall {Selected.ExtensionName} from the following path?\n\n{SelectedInstall.InstallPath}",
            $"Uninstall {Selected.ExtensionName} from selected path",
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
        }
    }
}
