using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

[HarmonyPatch(typeof(Fiddle), nameof(Fiddle.ModifyHandDraw))]
public static class FiddlePatch
{
    static void Postfix(Fiddle __instance, Player player, decimal count)
    {
        
        if (player != __instance.Owner)
        {
            return;
        }

        RelicStatCache.RecordCustomStat(
            __instance.Id.Entry,
            new List<int> { __instance.DynamicVars.Cards.IntValue, 0 }
        );
    }
}

