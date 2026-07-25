using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

[HarmonyPatch(typeof(ParryingShield), nameof(ParryingShield.AfterSideTurnEnd))]
public static class ParryingShieldPatch
{
    static void Postfix(
        ParryingShield __instance,
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants
    )
    {

        if (
            participants.Contains(__instance.Owner.Creature)
            && !((decimal)__instance.Owner.Creature.Block < __instance.DynamicVars.Block.BaseValue)
        )
        {
            int numCreatures = __instance.Owner.Creature.CombatState.HittableEnemies.Count;
            if (numCreatures != 0)
            {
                RelicStatCache.RecordCustomStat(
            __instance.Id.Entry,
            new List<int> { __instance.DynamicVars.Damage.IntValue }
        );
            }
        }
    }
}
