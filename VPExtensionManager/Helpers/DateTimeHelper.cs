namespace VPExtensionManager.Helpers;

public class DateTimeHelper
{
    public static bool ShouldCheckForUpdate(long lastChecked)
    {
        return GetCurrentUnixTime() - lastChecked >= 3_600
            || lastChecked < 0;
    }

    public static long GetCurrentUnixTime()
    {
        return DateTimeOffset.Now.ToUnixTimeSeconds();
    }
}
