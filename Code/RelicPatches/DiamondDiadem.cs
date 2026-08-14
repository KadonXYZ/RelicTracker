using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.Relics;

[HarmonyPatch(typeof(DiamondDiadem), nameof(DiamondDiadem.AfterSideTurnStart))]
public static class DiamondDiademPatch
{
    static void Postfix(DiamondDiadem __instance, CombatSide side, CombatState combatState)
    {
        if (CombatManager.Instance == null || !CombatManager.Instance.IsInProgress)
        {
            return;
        }

        if (side != __instance.Owner.Creature.Side)
        {
            return;
        }

        RelicStatCache.RecordCustomStat(
            __instance.Id.Entry,
            new List<int> { __instance.DynamicVars.Block.IntValue }
        );
    }
}
