namespace VPExtensionManager.Helpers;

public class DateTimeHelper
{
    public static bool ShouldCheckForUpdate(long lastChecked)
    {
        return lastChecked - GetCurrentUnixTime() >= 100_000
            || lastChecked < 0;
    }

    public static long GetCurrentUnixTime()
    {
        return DateTimeOffset.Now.ToUnixTimeSeconds();
    }
}
