using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;

public class RelicStatData
{
    public int UsageCount { get; set; }
    public List<int> CustomValues { get; set; } = new List<int>();
    public List<string> AdditionalData { get; set; } = new List<string>();
}

public static class RelicStatCache
{
    private static int _currentRunId = 0; // You can increment this at the start of each run to keep separate files
    private static readonly string SavePath = Path.Combine(OS.GetUserDataDir(), "RelicTracker");
    private static readonly string RunHistoryPath = Path.Combine(
        OS.GetUserDataDir(),
        "RelicTracker",
        "RunHistory"
    );
    private static Dictionary<string, RelicStatData> _cache;
    private static readonly object _lock = new();
    private static int _ignoreNextCreation = 0;

    // Ensures we don't crash even if someone forgets to call Initialize()
    private static void EnsureInitialized()
    {
        if (_cache != null)
            return;
        else
        {
            _cache = new Dictionary<string, RelicStatData>();
        }
    }

    /// <summary>
    /// Increments the trigger count for a given relic
    /// </summary>
    /// <param name="id"></param>
    public static void RecordTriggerStat(string id)
    {
        if (!RelicTracker.RelicTrackerSettings.ShouldTrack)
        {
            return;
        }

        ModLog.Info($"[Trigger] \nRecording trigger stat for {id}\n");
        EnsureInitialized();

        lock (_lock)
        {
            if (!_cache.TryGetValue(id, out var data))
            {
                data = new RelicStatData();
                _cache[id] = data;
            }
            data.UsageCount++;
        }
    }

    /// <summary>
    /// Records custom stat values directly
    /// </summary>
    /// <param name="id"></param>
    /// <param name="values"></param>
    public static void RecordCustomStat(string id, List<int> values)
    {
        if (!RelicTracker.RelicTrackerSettings.ShouldTrack)
        {
            return;
        }

        ModLog.Info(
            $"[Custom Stat] \nRecording custom stat for {id} with values {string.Join(", ", values)}\n"
        );
        EnsureInitialized();

        lock (_lock)
        {
            if (!_cache.TryGetValue(id, out var data))
            {
                data = new RelicStatData();
                _cache[id] = data;
            }

            if (data.CustomValues == null || data.CustomValues.Count == 0)
            {
                data.CustomValues = new List<int>(values);
            }
            else
            {
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
    }

    public static void RecordAdditionalStat(string id, List<string> values)
    {
        if (!RelicTracker.RelicTrackerSettings.ShouldTrack)
        {
            return;
        }

        ModLog.Info(
            $"[Custom Stat] \nRecording additional data for {id} with values {string.Join(", ", values)}\n"
        );
        EnsureInitialized();

        lock (_lock)
        {
            if (!_cache.TryGetValue(id, out var data))
            {
                data = new RelicStatData();
                _cache[id] = data;
            }

            if (data.AdditionalData == null || data.AdditionalData.Count == 0)
            {
                data.AdditionalData = new List<string>(values);
            }
            else
            {
                for (int i = 0; i < values.Count; i++)
                {
                    if (i < data.CustomValues.Count)
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
            return _cache.TryGetValue(id, out var data) ? data.UsageCount : 0;
        }
    }

    public static List<int> GetCustomValues(string id)
    {
        EnsureInitialized();
        lock (_lock)
        {
            return _cache.TryGetValue(id, out var data) ? data.CustomValues : null;
        }
    }

    public static List<string>? GetAdditionalValues(string id)
    {
        EnsureInitialized();
        lock (_lock)
        {
            return _cache.TryGetValue(id, out var data)
                ? (data.AdditionalData.Count > 0 ? data.AdditionalData : null)
                : null;
        }
    }

    public static int SetRunId(int newRunId)
    {
        _currentRunId = newRunId;
        return _currentRunId;
    }

    public static void InitializeForNewRun()
    {
        ModLog.Info($"Initializing cache for new run. Current run id: {_currentRunId}\n");
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

    #region - Saving & Loading

    public static void LoadCacheFromSingleplayerSave()
    {
        try
        {
            string filePath = Path.Combine(SavePath, $"singleplayer_save.json");
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                _cache =
                    JsonSerializer.Deserialize<Dictionary<string, RelicStatData>>(json)
                    ?? new Dictionary<string, RelicStatData>();
            }
            else
            {
                ModLog.Error(
                    $"Singleplayer save file not found at {filePath}. Starting with an empty cache.",
                    new FileNotFoundException("Save file not found")
                );
                _cache = new Dictionary<string, RelicStatData>();
            }
        }
        catch (System.Exception)
        {
            ModLog.Error(
                $"Error loading singleplayer save file. Starting with an empty cache.",
                new System.Exception("Error loading save file")
            );
            _cache = new Dictionary<string, RelicStatData>();
        }
    }

    public static void LoadCacheFromMultiplayerSave()
    {
        try
        {
            string filePath = Path.Combine(SavePath, $"multiplayer_save.json");
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                _cache =
                    JsonSerializer.Deserialize<Dictionary<string, RelicStatData>>(json)
                    ?? new Dictionary<string, RelicStatData>();
            }
            else
            {
                ModLog.Error(
                    $"Multiplayer save file not found at {filePath}. Starting with an empty cache.",
                    new FileNotFoundException("Save file not found")
                );
                _cache = new Dictionary<string, RelicStatData>();
            }
        }
        catch (System.Exception)
        {
            ModLog.Error(
                $"Error loading multiplayer save file. Starting with an empty cache.",
                new System.Exception("Error loading save file")
            );
            _cache = new Dictionary<string, RelicStatData>();
        }
    }

    public static void SaveCache(bool multiplayerSave)
    {
        string fileName = multiplayerSave ? "multiplayer_save.json" : "singleplayer_save.json";
        Directory.CreateDirectory(SavePath);
        string json = JsonSerializer.Serialize(_cache);
        File.WriteAllText(Path.Combine(SavePath, fileName), json);
    }

    public static void SaveRunHistory(long runStartTime)
    {
        string fileName = $"{runStartTime}.json";
        Directory.CreateDirectory(RunHistoryPath);
        string json = JsonSerializer.Serialize(_cache);
        File.WriteAllText(Path.Combine(RunHistoryPath, fileName), json);
    }

    public static void LoadRunHistory(string fileName)
    {
        try
        {
            var runID = fileName.Replace(".run", ".json");
            string filePath = Path.Combine(RunHistoryPath, runID);

            _ignoreNextCreation = 2; //Ignore next two cache initializations which happen when loading run history.

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                _cache =
                    JsonSerializer.Deserialize<Dictionary<string, RelicStatData>>(json)
                    ?? new Dictionary<string, RelicStatData>();
            }
            else
            {
                ModLog.Error(
                    $"Run history file not found at {filePath}. Starting with an empty cache.",
                    new FileNotFoundException("Run history file not found")
                );
                _cache = new Dictionary<string, RelicStatData>();
            }
        }
        catch (System.Exception)
        {
            ModLog.Error(
                $"Error loading run history file. Starting with an empty cache.",
                new System.Exception("Error loading run history file")
            );
            _cache = new Dictionary<string, RelicStatData>();
        }
    }

    /// <summary>
    /// Checks the filenames which are in Epoch time format, and deletes those older than a month.
    /// </summary>
    public static void CleanupOldHistory()
    {
        try
        {
            if (!Directory.Exists(RunHistoryPath))
                return;

            var files = Directory.GetFiles(RunHistoryPath);
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long oneMonthInSeconds = 30L * 24 * 60 * 60;

            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                if (long.TryParse(fileName, out long fileTime))
                {
                    if (currentTime - fileTime > oneMonthInSeconds)
                    {
                        File.Delete(file);
                        ModLog.Info($"Deleted old run history file: {file}");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            ModLog.Error("Error during cleanup of old run history files.", e);
        }
    }

    public static bool ShouldIgnoreNextCreation()
    {
        if (_ignoreNextCreation > 0)
        {
            _ignoreNextCreation--;
            return true;
        }
        return false;
    }

    #endregion
}
