using System.IO;
using System.Text.Json;
using Godot;

namespace RelicTracker;

public static class RelicTrackerSettings
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly string SettingsPath = Path.Combine(
        OS.GetUserDataDir(),
        "RelicTracker",
        "settings.json"
    );

    private static bool _enableRelicTracker = true;
    private static bool _showTooltipStats = true;
    private static bool _showNoDataYet = true;
    private static bool _keepRecordingStats = true;
    private static bool _suppressSave;

    public static bool EnableRelicTracker
    {
        get => _enableRelicTracker;
        set => Set(ref _enableRelicTracker, value);
    }

    public static bool ShowTooltipStats
    {
        get => _showTooltipStats;
        set => Set(ref _showTooltipStats, value);
    }

    public static bool ShowNoDataYet
    {
        get => _showNoDataYet;
        set => Set(ref _showNoDataYet, value);
    }

    public static bool KeepRecordingStats
    {
        get => _keepRecordingStats;
        set => Set(ref _keepRecordingStats, value);
    }

    public static bool ShouldTrack => EnableRelicTracker && KeepRecordingStats;

    public static bool ShouldShowTooltips => EnableRelicTracker && ShowTooltipStats;

    public static void Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                Save();
                return;
            }

            string json = File.ReadAllText(SettingsPath);
            StoredSettings? stored = JsonSerializer.Deserialize<StoredSettings>(json);
            if (stored == null)
            {
                Save();
                return;
            }

            _suppressSave = true;
            EnableRelicTracker = stored.EnableRelicTracker;
            ShowTooltipStats = stored.ShowTooltipStats;
            ShowNoDataYet = stored.ShowNoDataYet;
            KeepRecordingStats = stored.KeepRecordingStats;
        }
        catch (Exception ex)
        {
            ModLog.Error("RelicTrackerSettings.Load", ex);
        }
        finally
        {
            _suppressSave = false;
        }
    }

    public static void Save()
    {
        if (_suppressSave)
        {
            return;
        }

        try
        {
            string? directory = Path.GetDirectoryName(SettingsPath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var stored = new StoredSettings
            {
                EnableRelicTracker = EnableRelicTracker,
                ShowTooltipStats = ShowTooltipStats,
                ShowNoDataYet = ShowNoDataYet,
                KeepRecordingStats = KeepRecordingStats
            };
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(stored, JsonOptions));
        }
        catch (Exception ex)
        {
            ModLog.Error("RelicTrackerSettings.Save", ex);
        }
    }

    private static void Set(ref bool field, bool value)
    {
        if (field == value)
        {
            return;
        }

        field = value;
        Save();
    }

    private sealed class StoredSettings
    {
        public bool EnableRelicTracker { get; set; } = true;
        public bool ShowTooltipStats { get; set; } = true;
        public bool ShowNoDataYet { get; set; } = true;
        public bool KeepRecordingStats { get; set; } = true;
    }
}
