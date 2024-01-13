using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using VPExtensionManager.Contracts.Services;
using VPExtensionManager.Contracts.Views;
using VPExtensionManager.Core.Models;
using VPExtensionManager.Models;

namespace VPExtensionManager.Views;

public partial class SettingsPage : Page, INotifyPropertyChanged, INavigationAware
{
    private readonly AppConfig _appConfig;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILocalVPVersionService _localVPVersionService;
    private readonly ISystemService _systemService;
    private readonly IApplicationInfoService _applicationInfoService;
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

    public SettingsPage(IOptions<AppConfig> appConfig,
        IThemeSelectorService themeSelectorService,
        ILocalVPVersionService localVPVersionService,
        ISystemService systemService,
        IApplicationInfoService applicationInfoService)
    {
        _appConfig = appConfig.Value;
        _themeSelectorService = themeSelectorService;
        _localVPVersionService = localVPVersionService;
        _systemService = systemService;
        _applicationInfoService = applicationInfoService;
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

    private void OnPrivacyStatementClick(object sender, RoutedEventArgs e)
    {
        // _systemService.OpenInWebBrowser(_appConfig.PrivacyStatement);
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
}
