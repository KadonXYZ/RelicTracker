using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Modding;

namespace RelicTracker;

[ModInitializer(nameof(Initialize))]
public partial class MainFile
{
    public const string ModId = "RelicTracker"; //At the moment, this is used only for the Logger and harmony names.

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        ModLog.Init();
        RelicTrackerSettings.Load();

        Harmony harmony = new(ModId);

        harmony.PatchAll();
        RelicStatCache.CleanupOldHistory();

        Callable.From(TryRegisterBaseLibConfig).CallDeferred();
        ModLog.Info("RelicTracker initialized successfully!");
    }

    private static void TryRegisterBaseLibConfig()
    {
        try
        {
            BaseLibConfigRegistration.Register();
        }
        catch (Exception ex)
        {
            ModLog.Warning($"BaseLib config not registered (RelicTracker still runs without it): {ex.Message}");
        }
    }
}
