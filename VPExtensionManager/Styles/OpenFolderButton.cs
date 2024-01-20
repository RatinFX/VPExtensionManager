using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VPExtensionManager.Styles;

public class OpenFolderButton : Button
{
    public static readonly DependencyProperty FolderProperty =
        DependencyProperty.Register("Folder", typeof(string), typeof(OpenFolderButton));

    public string Folder
    {
        get { return (string)GetValue(FolderProperty); }
        set { SetValue(FolderProperty, value); }
    }

    protected override void OnClick()
    {
        base.OnClick();

        if (!string.IsNullOrEmpty(Folder) && System.IO.Directory.Exists(Folder))
        {
            Process.Start("explorer.exe", Folder);
        }
    }
}
