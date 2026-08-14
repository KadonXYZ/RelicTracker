using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;

[HarmonyPatch(typeof(SneckoSkull), nameof(SneckoSkull.AfterModifyingPowerAmountGiven))]
public static class SneckoSkullPatch
{
    static void Postfix(SneckoSkull __instance, PowerModel power)
    {
        if (power is not PoisonPower)
        {
            return;
        }

        RelicStatCache.RecordCustomStat(
            __instance.Id.Entry,
            new List<int> { __instance.DynamicVars.Poison.IntValue }
        );
    }
}
