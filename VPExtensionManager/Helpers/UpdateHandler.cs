using System.Diagnostics;
using System.IO;
using VPExtensionManager.Models;

namespace VPExtensionManager.Helpers;

internal class UpdateHandler
{
    public static void StartUpdate(string downloadPath)
    {
        var res = MessageBoxes.YesNo(
            Properties.Resources.MessageBoxYesNoUpdateVPEMContent,
            Properties.Resources.MessageBoxYesNoUpdateVPEMTitle
        );

        if (res != System.Windows.MessageBoxResult.Yes)
        {
            return;
        }

        var success = AppProperties.Get(AppProperties.LatestVersionURL, out string releaseLink);
        if (!success)
        {
            MessageBoxes.Error(content: Properties.Resources.ErrorLatestVersionURLNotFound);
            return;
        }

        var installPath = Environment.CurrentDirectory;
        var updaterToOverwrite = Path.Combine(installPath, "VPExtensionManager.Updater.exe");
        var updaterToRun = Path.Combine(installPath, "Updater.exe");

        // Copy the current Updater so everything can be overwritten during the update process
        File.Copy(updaterToOverwrite, updaterToRun, true);

        var psi = new ProcessStartInfo
        {
            FileName = updaterToRun,
            Arguments = @$"""{releaseLink}"" ""{downloadPath}"" ""{installPath}""",
            Verb = "runas",
            UseShellExecute = true
        };

        Process.Start(psi);

        App.Current.Shutdown(0);
    }
}
