using BaseLib.Config;

namespace RelicTracker;

public sealed class RelicTrackerConfig : SimpleModConfig
{
    [ConfigSection("General")]
    public static bool EnableRelicTracker
    {
        get => RelicTrackerSettings.EnableRelicTracker;
        set => RelicTrackerSettings.EnableRelicTracker = value;
    }

    public static bool ShowTooltipStats
    {
        get => RelicTrackerSettings.ShowTooltipStats;
        set => RelicTrackerSettings.ShowTooltipStats = value;
    }

    public static bool ShowNoDataYet
    {
        get => RelicTrackerSettings.ShowNoDataYet;
        set => RelicTrackerSettings.ShowNoDataYet = value;
    }

    public static bool KeepRecordingStats
    {
        get => RelicTrackerSettings.KeepRecordingStats;
        set => RelicTrackerSettings.KeepRecordingStats = value;
    }
}

internal static class BaseLibConfigRegistration
{
    public static void Register()
    {
        ModConfigRegistry.Register(MainFile.ModId, new RelicTrackerConfig());
        ModLog.Info("Registered RelicTracker config with BaseLib.");
    }
}
