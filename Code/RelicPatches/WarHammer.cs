using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

[HarmonyPatch(typeof(WarHammer), nameof(WarHammer.AfterCombatVictory))]
public static class WarHammerPatch
{
    static void Prefix(WarHammer __instance, AbstractRoom room)
    {
        if (room.RoomType != RoomType.Elite)
        {
            return;
        }

        int maxUpgrades = __instance.DynamicVars.Cards.IntValue;
        int upgradableCards = PileType.Deck
            .GetPile(__instance.Owner)
            .Cards.Count(card => card.IsUpgradable);
        int cardsUpgraded = Math.Min(maxUpgrades, upgradableCards);

        if (cardsUpgraded <= 0)
        {
            return;
        }

        RelicStatCache.RecordCustomStat(__instance.Id.Entry, new List<int> { cardsUpgraded });
    }
}
