using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

[HarmonyPatch(typeof(BlackBlood), nameof(BlackBlood.AfterCombatVictory))]
public static class BlackBloodPatch
{
    static void Prefix(BlackBlood __instance, CombatRoom _)
    {
        if (!LocalContext.IsMe(__instance.Owner) || __instance.Owner.Creature.IsDead)
        {
            return;
        }

        Creature creature = __instance.Owner.Creature;
        int healthMissing = creature.MaxHp - creature.CurrentHp;
        int heal = __instance.DynamicVars.Heal.IntValue;

        RelicStatCache.RecordCustomStat(
            __instance.Id.Entry,
            new List<int> { Math.Min(healthMissing, heal) }
        );
    }
}
