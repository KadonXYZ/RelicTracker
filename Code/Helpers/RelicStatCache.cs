using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;

public class RelicStatData
{
    public int UsageCount { get; set; }
    public List<int> CustomValues { get; set; } = new();
    public List<string> AdditionalData { get; set; } = new();
}

public static class RelicStatCache
{
    private static int _currentRunId;
    private static readonly string SavePath = Path.Combine(OS.GetUserDataDir(), "RelicTracker");
    private static readonly string RunHistoryPath = Path.Combine(SavePath, "RunHistory");
    private static Dictionary<string, RelicStatData> _cache = new();
    private static readonly object _lock = new();
    private static int _ignoreNextCreation;

    private static void EnsureInitialized()
    {
        _cache ??= new Dictionary<string, RelicStatData>();
    }

    public static void RecordTriggerStat(string id)
    {
        if (!RelicTracker.RelicTrackerSettings.ShouldTrack)
        {
            return;
        }

        EnsureInitialized();
        lock (_lock)
        {
            GetOrCreate(id).UsageCount++;
        }
    }

    public static void RecordCustomStat(string id, List<int> values)
    {
        if (!RelicTracker.RelicTrackerSettings.ShouldTrack)
        {
            return;
        }

        EnsureInitialized();
        lock (_lock)
        {
            RelicStatData data = GetOrCreate(id);
            if (data.CustomValues.Count == 0)
            {
                data.CustomValues = new List<int>(values);
                return;
            }

            for (int i = 0; i < values.Count; i++)
            {
                if (i < data.CustomValues.Count)
                {
                    data.CustomValues[i] += values[i];
                }
                else
                {
                    data.CustomValues.Add(values[i]);
                }
            }
        }
    }

    public static void RecordAdditionalStat(string id, List<string> values)
    {
        if (!RelicTracker.RelicTrackerSettings.ShouldTrack)
        {
            return;
        }

        EnsureInitialized();
        lock (_lock)
        {
            RelicStatData data = GetOrCreate(id);
            if (data.AdditionalData.Count == 0)
            {
                data.AdditionalData = new List<string>(values);
                return;
            }

            for (int i = 0; i < values.Count; i++)
            {
                if (i < data.AdditionalData.Count)
                {
                    data.AdditionalData[i] += values[i];
                }
                else
                {
                    data.AdditionalData.Add(values[i]);
                }
            }
        }
    }

    public static bool HasStatsForRelic(string id)
    {
        EnsureInitialized();
        lock (_lock)
        {
            return _cache.ContainsKey(id);
        }
    }

    public static int GetTriggeredCount(string id)
    {
        EnsureInitialized();
        lock (_lock)
        {
            return _cache.TryGetValue(id, out RelicStatData? data) ? data.UsageCount : 0;
        }
    }

    public static List<int>? GetCustomValues(string id)
    {
        EnsureInitialized();
        lock (_lock)
        {
            return _cache.TryGetValue(id, out RelicStatData? data) ? data.CustomValues : null;
        }
    }

    public static List<string>? GetAdditionalValues(string id)
    {
        EnsureInitialized();
        lock (_lock)
        {
            return _cache.TryGetValue(id, out RelicStatData? data) && data.AdditionalData.Count > 0
                ? data.AdditionalData
                : null;
        }
    }

    public static void InitializeForNewRun()
    {
        WipeOldCache();
        _currentRunId++;
        _cache = new Dictionary<string, RelicStatData>();
    }

    public static void WipeOldCache()
    {
        string filePath = Path.Combine(SavePath, $"run_{_currentRunId}.json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public static void LoadCacheFromSingleplayerSave() =>
        LoadCacheFromFile("singleplayer_save.json");

    public static void LoadCacheFromMultiplayerSave() =>
        LoadCacheFromFile("multiplayer_save.json");

    public static void SaveCache(bool multiplayerSave)
    {
        string fileName = multiplayerSave ? "multiplayer_save.json" : "singleplayer_save.json";
        Directory.CreateDirectory(SavePath);
        File.WriteAllText(Path.Combine(SavePath, fileName), JsonSerializer.Serialize(_cache));
    }

    public static void SaveRunHistory(long runStartTime)
    {
        Directory.CreateDirectory(RunHistoryPath);
        File.WriteAllText(
            Path.Combine(RunHistoryPath, $"{runStartTime}.json"),
            JsonSerializer.Serialize(_cache)
        );
    }

    public static void LoadRunHistory(string fileName)
    {
        _ignoreNextCreation = 2; // Ignore the next two cache initializations when loading run history.
        LoadCacheFromFile(Path.Combine("RunHistory", fileName.Replace(".run", ".json")));
    }

    public static void CleanupOldHistory()
    {
        try
        {
            if (!Directory.Exists(RunHistoryPath))
            {
                return;
            }

            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long oneMonthInSeconds = 30L * 24 * 60 * 60;

            foreach (string file in Directory.GetFiles(RunHistoryPath))
            {
                if (!long.TryParse(Path.GetFileNameWithoutExtension(file), out long fileTime))
                {
                    continue;
                }

                if (currentTime - fileTime > oneMonthInSeconds)
                {
                    File.Delete(file);
                }
            }
        }
        catch (Exception e)
        {
            ModLog.Error("Error during cleanup of old run history files.", e);
        }
    }

    public static bool ShouldIgnoreNextCreation()
    {
        if (_ignoreNextCreation <= 0)
        {
            return false;
        }

        _ignoreNextCreation--;
        return true;
    }

    private static RelicStatData GetOrCreate(string id)
    {
        if (!_cache.TryGetValue(id, out RelicStatData? data))
        {
            data = new RelicStatData();
            _cache[id] = data;
        }

        return data;
    }

    private static void LoadCacheFromFile(string relativePath)
    {
        string filePath = Path.Combine(SavePath, relativePath);

        try
        {
            if (!File.Exists(filePath))
            {
                _cache = new Dictionary<string, RelicStatData>();
                return;
            }

            string json = File.ReadAllText(filePath);
            _cache =
                JsonSerializer.Deserialize<Dictionary<string, RelicStatData>>(json)
                ?? new Dictionary<string, RelicStatData>();
        }
        catch (Exception e)
        {
            ModLog.Error($"Error loading cache from {filePath}. Starting with an empty cache.", e);
            _cache = new Dictionary<string, RelicStatData>();
        }
    }
}
