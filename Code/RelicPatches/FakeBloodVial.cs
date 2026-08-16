using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

[HarmonyPatch(typeof(FakeBloodVial), nameof(FakeBloodVial.AfterPlayerTurnStartLate))]
public static class FakeBloodVialPatch
{
    static void Prefix(FakeBloodVial __instance, PlayerChoiceContext choiceContext, Player player)
    {
        if (player != __instance.Owner || player.Creature.CombatState.RoundNumber > 1)
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
