using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using VPExtensionManager.Interfaces.Services;
using VPExtensionManager.Models;

namespace VPExtensionManager.Views;

public partial class UpdateNotesWindow : MetroWindow, INotifyPropertyChanged
{
    private readonly ISystemService _systemService;

    private string _githubLink;

    private string _updateNotes;
    public string UpdateNotes
    {
        get => _updateNotes;
        set => Set(ref _updateNotes, value);
    }

    public UpdateNotesWindow(ISystemService systemService, VPExtensionBase extension)
    {
        _systemService = systemService;

        InitializeComponent();

        Title = $"{extension.ExtensionName} - {extension.LatestVersion}";

        _githubLink = extension.LatestHtmlUrl;
        UpdateNotes = extension.LatestReleaseNotes;
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

    private void btnVisitGitHub_Click(object sender, RoutedEventArgs e)
    {
        _systemService.OpenInWebBrowser(_githubLink);
        Close();
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
