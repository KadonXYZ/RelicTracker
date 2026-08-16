using System.Reflection;
using BaseLib.Config;

namespace RelicTracker;

public sealed class RelicTrackerConfig : SimpleModConfig
{
    [ConfigSection("General")]
    public static bool EnableRelicTracker
    {
        get => Settings.Get(nameof(EnableRelicTracker));
        set => Settings.Set(nameof(EnableRelicTracker), value);
    }

    public static bool ShowTooltipStats
    {
        get => Settings.Get(nameof(ShowTooltipStats));
        set => Settings.Set(nameof(ShowTooltipStats), value);
    }

    public static bool ShowNoDataYet
    {
        get => Settings.Get(nameof(ShowNoDataYet));
        set => Settings.Set(nameof(ShowNoDataYet), value);
    }

    public static bool KeepRecordingStats
    {
        get => Settings.Get(nameof(KeepRecordingStats));
        set => Settings.Set(nameof(KeepRecordingStats), value);
    }
}

public static class BaseLibConfigRegistration
{
    public static void Register() =>
        ModConfigRegistry.Register("RelicTracker", new RelicTrackerConfig());
}

// Reflect into RelicTracker.dll instead of referencing it (avoids load-context failures).
file static class Settings
{
    private static Type? _settingsType;

    private static Type SettingsType =>
        _settingsType ??= AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType("RelicTracker.RelicTrackerSettings"))
            .FirstOrDefault(type => type is not null)
        ?? throw new InvalidOperationException("RelicTracker.RelicTrackerSettings was not found.");

    public static bool Get(string name) => (bool)Property(name).GetValue(null)!;

    public static void Set(string name, bool value) => Property(name).SetValue(null, value);

    private static PropertyInfo Property(string name) =>
        SettingsType.GetProperty(name, BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException($"RelicTrackerSettings.{name} was not found.");
}
