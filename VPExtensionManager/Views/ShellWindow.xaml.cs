using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using VPExtensionManager.Interfaces.Services;
using VPExtensionManager.Interfaces.Views;

namespace VPExtensionManager.Views;

public partial class ShellWindow : MetroWindow, IShellWindow, INotifyPropertyChanged
{
    private readonly INavigationService _navigationService;
    private readonly IRightPaneService _rightPaneService;
    private readonly IApplicationInfoService _applicationInfoService;

    private bool _canGoBack;
    public bool CanGoBack
    {
        get => _canGoBack;
        set => Set(ref _canGoBack, value);
    }

    public ShellWindow(INavigationService navigationService, IRightPaneService rightPaneService, IApplicationInfoService applicationInfoService)
    {
        _navigationService = navigationService;
        _navigationService.Navigated += OnNavigated;

        _rightPaneService = rightPaneService;
        _applicationInfoService = applicationInfoService;

        InitializeComponent();
        DataContext = this;

        Title = $"{Properties.Resources.AppDisplayName} - {_applicationInfoService.GetVersionShort()}";
        OnPropertyChanged(nameof(Title));
    }

    public Frame GetNavigationFrame()
        => shellFrame;

    public Frame GetRightPaneFrame()
        => rightPaneFrame;

    public void ShowWindow()
        => Show();

    public void CloseWindow()
        => Close();

    public SplitView GetSplitView()
        => splitView;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _rightPaneService.CleanUp();
    }

    private void OnGoBack(object sender, RoutedEventArgs e)
    {
        _navigationService.GoBack();
    }

    private void OnNavigated(object sender, Type pageType)
    {
        CanGoBack = _navigationService.CanGoBack;
    }

    private void OnMenuFileExit(object sender, RoutedEventArgs e)
        => Application.Current.Shutdown();

    private void OnMenuViewsMain(object sender, RoutedEventArgs e)
        => _navigationService.NavigateTo(typeof(MainPage), null, true);

    private void OnMenuSettings(object sender, RoutedEventArgs e)
        => _rightPaneService.OpenInRightPane(typeof(SettingsPage));

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    private void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (Equals(storage, value))
        {
            return;
        }

        storage = value;
        OnPropertyChanged(propertyName);
    }
}
