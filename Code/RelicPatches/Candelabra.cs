using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.Relics;

[HarmonyPatch(typeof(Candelabra), nameof(Candelabra.AfterSideTurnStart))]
public static class CandelabraPatch
{
    private static readonly System.Reflection.FieldInfo EnergyTurnField = AccessTools.Field(
        typeof(Candelabra),
        "_energyTurn"
    );

    static void Postfix(Candelabra __instance, CombatSide side, CombatState combatState)
    {
        if (CombatManager.Instance == null || !CombatManager.Instance.IsInProgress)
        {
            return;
        }

        if (side != __instance.Owner.Creature.Side)
        {
            return;
        }

        int energyTurn = EnergyTurnField?.GetValue(__instance) as int? ?? -1;
        if (combatState.RoundNumber != energyTurn)
        {
            return;
        }

        RelicStatCache.RecordCustomStat(
            __instance.Id.Entry,
            new List<int> { __instance.DynamicVars.Energy.IntValue }
        );
    }
}
