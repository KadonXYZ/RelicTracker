using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

[HarmonyPatch(typeof(ToastyMittens), nameof(ToastyMittens.AfterPlayerTurnStart))]
public static class ToastyMittensPatch
{
    static void Prefix(ToastyMittens __instance, PlayerChoiceContext choiceContext, Player player)
    {
        if (CombatManager.Instance == null || !CombatManager.Instance.IsInProgress)
        {
            return;
        }

        if (player != __instance.Owner)
        {
            return;
        }

        RelicStatCache.RecordCustomStat(
            __instance.Id.Entry,
            new List<int> { __instance.DynamicVars.Strength.IntValue }
        );
    }
}
