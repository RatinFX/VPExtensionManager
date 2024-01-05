using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

using VPExtensionManager.Contracts.Views;
using VPExtensionManager.Core.Contracts.Services;
using VPExtensionManager.Core.Models;

namespace VPExtensionManager.Views;

public partial class MainPage : Page, INotifyPropertyChanged, INavigationAware
{
    private readonly IExtensionService _extensionDataService;

    private VPExtension _selected;

    public VPExtension Selected
    {
        get { return _selected; }
        set { Set(ref _selected, value); }
    }

    public ObservableCollection<VPExtension> ExtensionItems { get; private set; } = new ObservableCollection<VPExtension>();

    public MainPage(IExtensionService extensionDataService)
    {
        _extensionDataService = extensionDataService;
        InitializeComponent();
        DataContext = this;
    }

    public async void OnNavigatedTo(object parameter)
    {
        ExtensionItems.Clear();

        var data = await _extensionDataService.GetListDetailsDataAsync();

        foreach (var item in data)
        {
            ExtensionItems.Add(item);
        }

        Selected = ExtensionItems.First();
    }

    public void OnNavigatedFrom()
    {
    }

    private void btnUpdate_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        // Update(Selected) - select-install window -> download zip/dll, extract, get path via installs.vpversion
    }

    private void btnInstall_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        // Install(Selected) - install window -> [vegas version] [install path], OK + Cancel | enter + esc
    }

    private void btnUninstall_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        // Uninstall(Selected) - select-install window
    }

    private void btnEditInstallPath_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        // Edit(SelectedInstall) - edit window
    }

    private void btnRemoveInstallPath_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        // Remove(SelectedInstall) - confirmation window
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (Equals(storage, value))
        {
            return;
        }

        storage = value;
        OnPropertyChanged(propertyName);
    }

    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
