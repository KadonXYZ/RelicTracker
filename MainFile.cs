using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace RelicTracker;

[ModInitializer(nameof(Initialize))]
public partial class MainFile
{
    public const string ModId = "RelicTracker";

    public static void Initialize()
    {
        ModLog.Init();
        RelicTrackerSettings.Load();

        new Harmony(ModId).PatchAll();
        RelicStatCache.CleanupOldHistory();

        BaseLibConfigLoader.Schedule();
        ModLog.Info("RelicTracker initialized successfully!");
    }
}
