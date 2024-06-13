using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using VPExtensionManager.Models;

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
        set
        {
            Set(ref _installPath, value);
            OnPropertyChanged(nameof(OpenFolderEnabled));
        }
    }

    private bool _forceDownload;
    public bool ForceDownload
    {
        get => _forceDownload;
        set => Set(ref _forceDownload, value);
    }

    private ObservableCollection<string> _installPathsSource = new();
    public ObservableCollection<string> InstallPathsSource
    {
        get => _installPathsSource;
        set => Set(ref _installPathsSource, value);
    }

    private bool _installPathEnabled = true;
    public bool InstallPathEnabled
    {
        get => _installPathEnabled;
        set => Set(ref _installPathEnabled, value);
    }

    public bool OpenFolderEnabled => Directory.Exists(InstallPath);

    public InstallWindow(VPExtensionBase extension, VPInstall selectedInstall)
    {
        var title = string.Format(Properties.Resources.WindowTitleUpdate, extension.ExtensionName);
        
        Init(title, extension);

        InstallPathsSource.Add(selectedInstall.InstallPath);

        SetDefaults();

        InstallPathEnabled = false;
    }

    public InstallWindow(VPExtensionBase extension, List<string> availableFolders)
    {
        var title = string.Format(Properties.Resources.WindowTitleInstall, extension.ExtensionName);

        Init(title, extension);

        foreach (var folder in availableFolders)
        {
            // Skip already existing paths - is this an issue?
            // one would probably not want to install 2 different
            // versions of the same extension in the same folder
            if (extension.Installs.Any(x => x.InstallPath == folder))
            {
                continue;
            }

            InstallPathsSource.Add(folder);
        }

        SetDefaults();
    }

    private void Init(string title, VPExtensionBase extension)
    {
        InitializeComponent();
        DataContext = this;

        Title = title;

        if (extension.ReleaseAssets.Any(x => x.VP == VPVersion.Magix))
        {
            VPVersionsSource.Add(VPVersion.Magix);
        }

        if (extension.ReleaseAssets.Any(x => x.VP == VPVersion.Sony))
        {
            VPVersionsSource.Add(VPVersion.Sony);
        }
    }

    private void SetDefaults()
    {
        VPVersion = VPVersionsSource.FirstOrDefault();
        InstallPath = InstallPathsSource.FirstOrDefault();
    }

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
        DialogResult = result;
        Close();
    }

    internal (VPVersion, string, bool) GetSelectedValues() => (VPVersion, InstallPath, ForceDownload);
}
