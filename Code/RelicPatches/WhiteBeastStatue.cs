using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

[HarmonyPatch(typeof(WhiteBeastStatue), nameof(WhiteBeastStatue.ShouldForcePotionReward))]
public static class WhiteBeastStatuePatch
{
    static void Postfix(WhiteBeastStatue __instance, Player player, RoomType roomType, bool __result)
    {
        if (player != __instance.Owner)
        {
            return;
        }

        if (!roomType.IsCombatRoom())
        {
            return;
        }

        if (!__result)
        {
            return;
        }

        RelicStatCache.RecordCustomStat(__instance.Id.Entry, new List<int> { 1 });
    }
}
