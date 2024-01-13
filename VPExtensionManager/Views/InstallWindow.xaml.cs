using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using VPExtensionManager.Core.Models;

namespace VPExtensionManager.Views;

public partial class InstallWindow : MetroWindow, INotifyPropertyChanged
{
    private VPVersion _vpVersion;
    public VPVersion VPVersion
    {
        get => _vpVersion;
        set => Set(ref _vpVersion, value);
    }

    private ObservableCollection<VPVersion> _vpVersionsSource = new();
    public ObservableCollection<VPVersion> VPVersionsSource
    {
        get => _vpVersionsSource;
        set => Set(ref _vpVersionsSource, value);
    }

    private string _installPath;
    public string InstallPath
    {
        get => _installPath;
        set => Set(ref _installPath, value);
    }

    private ObservableCollection<string> _installPathsSource = new();
    public ObservableCollection<string> InstallPathsSource
    {
        get => _installPathsSource;
        set => Set(ref _installPathsSource, value);
    }

    private bool _forceDownload;
    public bool ForceDownload
    {
        get => _forceDownload;
        set => Set(ref _forceDownload, value);
    }

    public InstallWindow(VPExtension extension, List<string> availableFolders)
    {
        InitializeComponent();
        DataContext = this;

        Title = $"Install {extension.ExtensionName}";

        if (extension.ReleaseAssets.Any(x => x.VP == VPVersion.Magix))
            VPVersionsSource.Add(VPVersion.Magix);

        if (extension.ReleaseAssets.Any(x => x.VP == VPVersion.Sony))
            VPVersionsSource.Add(VPVersion.Sony);

        foreach (var folder in availableFolders)
        {
            // Skip already existing paths
            if (extension.Installs.Any(x => Directory.GetParent(x.InstallPath).FullName == folder))
                continue;

            InstallPathsSource.Add(folder);
        }

        VPVersion = VPVersionsSource.FirstOrDefault();
        InstallPath = InstallPathsSource.FirstOrDefault();

        ForceDownload = App.Current.Properties.Contains("ForceDownload")
            ? bool.Parse(App.Current.Properties["ForceDownload"].ToString())
            : false;
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

    private void tbxForceDownload_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        ForceDownload = !ForceDownload;
    }

    private void btnOK_Click(object sender, RoutedEventArgs e)
    {
        CloseWindow(true);
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        CloseWindow(false);
    }

    private void CloseWindow(bool result)
    {
        App.Current.Properties["ForceDownload"] = ForceDownload.ToString();

        DialogResult = result;
        Close();
    }

    internal (VPVersion, string, bool) GetSelectedValues() => (VPVersion, InstallPath, ForceDownload);
}
