using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;

[HarmonyPatch(typeof(RunSaveManager),nameof(RunSaveManager.LoadRunSave))]
public static class LoadSingleplayerPatch
{
    static void Postfix(ReadSaveResult<SerializableRun> __result)
    {
        ModLog.Info("Loading singleplayer run save, loading relic stat cache...");
        RelicStatCache.LoadCacheFromSingleplayerSave();
    }
}

[HarmonyPatch(typeof(RunSaveManager),nameof(RunSaveManager.LoadMultiplayerRunSave))]
public static class LoadMultiplayerPatch
{
    static void Postfix(ReadSaveResult<SerializableRun> __result)
    {
        ModLog.Info("Loading multiplayer run save, loading relic stat cache...");
        RelicStatCache.LoadCacheFromMultiplayerSave();
    }
}

[HarmonyPatch(typeof(RunSaveManager), nameof(RunSaveManager.SaveRun), new Type[] { typeof(SerializableRun), typeof(bool) })]
public static class SaveRunPatch
{
    static void Prefix(Task __result, SerializableRun save, bool isMultiplayer)
    {
        ModLog.Info("Saving run, saving relic stat cache...");
        RelicStatCache.SaveCache(isMultiplayer);
    }
}

[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SaveRunHistory))]
public static class RunHistorySaver
{
    static void Prefix(RunHistory history)
    {
        ModLog.Info("Saving run history, saving relic stat cache...");
        RelicStatCache.SaveRunHistory(history.StartTime);
    }
}

[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadRunHistory))]
public static class RunHistoryLoader
{
    static void Postfix(string fileName)
    {
        ModLog.Info("Loading run history, loading relic stat cache...");
        RelicStatCache.LoadRunHistory(fileName);
    }
}