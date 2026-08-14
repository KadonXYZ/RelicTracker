using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

[HarmonyPatch(typeof(Toolbox), nameof(Toolbox.BeforeHandDraw))]
public static class ToolboxPatch
{
    private static int _lastCombatId = -1;

    static void Prefix(
        Toolbox __instance,
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

        // Toolbox only offers a colorless card at the start of combat.
        if (__instance.Owner.PlayerCombatState?.TurnNumber != 1)
        {
            return;
        }

        if (!CombatStartManager.IsNewCombat(ref _lastCombatId))
        {
            return;
        }

        RelicStatCache.RecordCustomStat(__instance.Id.Entry, new List<int> { 1 });
    }
}
