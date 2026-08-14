using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

[HarmonyPatch(typeof(EmotionChip), nameof(EmotionChip.AfterPlayerTurnStart))]
public static class EmotionChipPatch
{
    private static readonly System.Reflection.PropertyInfo LostHpProp = AccessTools.Property(
        typeof(EmotionChip),
        "LostHpInPreviousTurn"
    );

    static void Prefix(EmotionChip __instance, PlayerChoiceContext choiceContext, Player player)
    {
        if (player != __instance.Owner)
        {
            return;
        }

        bool lostHp = LostHpProp?.GetValue(__instance) as bool? ?? false;
        if (!lostHp)
        {
            return;
        }

        int orbCount = player.PlayerCombatState?.OrbQueue?.Orbs?.Count() ?? 0;
        if (orbCount <= 0)
        {
            return;
        }

        RelicStatCache.RecordCustomStat(__instance.Id.Entry, new List<int> { orbCount });
    }
}
