using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

[HarmonyPatch(typeof(JeweledMask), nameof(JeweledMask.BeforeHandDraw))]
public static class JeweledMaskPatch
{
    static void Postfix(
        JeweledMask __instance,
        Player player,
        PlayerChoiceContext choiceContext,
        CombatState combatState
    )
    {
        if (CombatManager.Instance == null || !CombatManager.Instance.IsInProgress)
        {
            return;
        }

        if (player != __instance.Owner)
        {
            return;
        }

        // One free Power pulled from the draw pile at the start of each combat.
        if (combatState.RoundNumber == 1)
        {
            RelicStatCache.RecordCustomStat(__instance.Id.Entry, new List<int> { 1 });
        }
    }
}
