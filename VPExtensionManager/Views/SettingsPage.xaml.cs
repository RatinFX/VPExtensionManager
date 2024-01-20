using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using VPExtensionManager.Interfaces.Services;
using VPExtensionManager.Interfaces.Views;
using VPExtensionManager.Models;

namespace VPExtensionManager.Views;

public partial class SettingsPage : Page, INotifyPropertyChanged, INavigationAware
{
    private readonly AppConfig _appConfig;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILocalVPVersionService _localVPVersionService;
    private readonly ISystemService _systemService;
    private readonly IApplicationInfoService _applicationInfoService;
    private readonly IFolderService _folderService;
    private readonly IExtensionService _extensionService;
    private readonly INotificationService _notificationService;
    private bool _isInitialized;

    private AppTheme _theme;
    public AppTheme Theme
    {
        get => _theme;
        set => Set(ref _theme, value);
    }

    private string _versionDescription;
    public string VersionDescription
    {
        get => _versionDescription;
        set => Set(ref _versionDescription, value);
    }

    private ObservableCollection<SelectableObject<int>> _vpVersionsSource = new();
    public ObservableCollection<SelectableObject<int>> VPVersionsSource
    {
        get => _vpVersionsSource;
        set => Set(ref _vpVersionsSource, value);
    }

    private string _downloadsFolder;
    public string DownloadsFolder
    {
        get => _downloadsFolder;
        set => Set(ref _downloadsFolder, value);
    }

    public SettingsPage(IOptions<AppConfig> appConfig,
        IThemeSelectorService themeSelectorService,
        ILocalVPVersionService localVPVersionService,
        ISystemService systemService,
        IApplicationInfoService applicationInfoService,
        IFolderService folderService,
        IExtensionService extensionService,
        INotificationService notificationService)
    {
        _appConfig = appConfig.Value;
        _themeSelectorService = themeSelectorService;
        _localVPVersionService = localVPVersionService;
        _systemService = systemService;
        _applicationInfoService = applicationInfoService;
        _folderService = folderService;
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

    public void OnNavigatedTo(object parameter)
    {
        VersionDescription = $"{Properties.Resources.AppDisplayName} - {_applicationInfoService.GetVersion()}";
        Theme = _themeSelectorService.GetCurrentTheme();

        // Only support VP14 and up to VP21 for now
        var versions = _localVPVersionService.GetLocalVersions();
        var minVP = 14;
        var maxVP = 21 + 1;
        foreach (var i in Enumerable.Range(minVP, maxVP - minVP))
        {
            var s = new SelectableObject<int>(i, versions.Contains(i));
            VPVersionsSource.Add(s);
        }

        UpdateVPVersionText();

        DownloadsFolder = _folderService.GetDownloadsFolder();

        _isInitialized = true;
    }

    public void OnNavigatedFrom() { }

    private void OnLightChecked(object sender, RoutedEventArgs e)
    {
        if (_isInitialized)
        {
            _themeSelectorService.SetTheme(AppTheme.Light);
        }
    }

    private void OnDarkChecked(object sender, RoutedEventArgs e)
    {
        if (_isInitialized)
        {
            _themeSelectorService.SetTheme(AppTheme.Dark);
        }
    }

    private void OnDefaultChecked(object sender, RoutedEventArgs e)
    {
        if (_isInitialized)
        {
            _themeSelectorService.SetTheme(AppTheme.Default);
        }
    }

    private void OnVisitGitHubClick(object sender, RoutedEventArgs e)
    {
        _systemService.OpenInWebBrowser(_appConfig.GitHubPage);
    }

    private void OnVPVersionsSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var comboBox = (ComboBox)sender;
        var selected = (SelectableObject<int>)comboBox.SelectedItem;

        if (selected == null)
            return;

        foreach (SelectableObject<int> v in VPVersionsSource)
        {
            if (v.Data == selected.Data)
            {
                v.IsSelected = !v.IsSelected;
                break;
            }
        }

        comboBox.SelectedItem = null;
    }

    private void OnVPVersionCheckBoxChecked(object sender, RoutedEventArgs e)
    {
        UpdateVPVersionText();
    }

    private void UpdateVPVersionText()
    {
        var sb = new List<int>();

        foreach (var v in VPVersionsSource)
        {
            if (v.IsSelected)
                sb.Add(v.Data);
        }

        tbxVPVersions.Text = string.Join(", ", sb);

        if (string.IsNullOrEmpty(tbxVPVersions.Text))
            tbxVPVersions.Text = "Select your installed VP versions...";

        if (!IsInitialized)
            return;

        _localVPVersionService.SetLocalVersions(sb);
    }

    private void OnChangeDownloadsFolderClick(object sender, RoutedEventArgs e)
    {
        var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
        var result = dialog.ShowDialog() ?? false;

        if (!result || string.IsNullOrEmpty(dialog.SelectedPath))
            return;

        DownloadsFolder = dialog.SelectedPath;
        _folderService.SaveDownloadsFolder(DownloadsFolder);
        _extensionService.SetDownloadsPath(DownloadsFolder);

        _notificationService.Success($"Downloads folder was changed");
    }

    private void OnResetDownloadsFolderClick(object sender, RoutedEventArgs e)
    {
        _folderService.ResetDownloadsFolder();
        DownloadsFolder = _folderService.GetDownloadsFolder();
        _extensionService.SetDownloadsPath(DownloadsFolder);

        _notificationService.Success(
            $"Downloads folder was reset to:\n" +
            $"%localappdata%\\{_appConfig.MainFolder}\\{_appConfig.DownloadsFolder}"
        );
    }
}
