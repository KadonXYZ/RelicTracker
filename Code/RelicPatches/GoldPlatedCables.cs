using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Relics;

[HarmonyPatch(typeof(GoldPlatedCables), nameof(GoldPlatedCables.AfterModifyingOrbPassiveTriggerCount))]
public static class GoldPlatedCablesPatch
{
    static void Postfix(GoldPlatedCables __instance, OrbModel orb)
    {
        RelicStatCache.RecordCustomStat(__instance.Id.Entry, new List<int> { 1 });
    }
}
