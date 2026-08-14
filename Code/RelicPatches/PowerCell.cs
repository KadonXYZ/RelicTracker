using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

[HarmonyPatch(typeof(PowerCell), nameof(PowerCell.BeforeSideTurnStart))]
public static class PowerCellPatch
{
    private static int _lastCombatId = -1;

    static void Prefix(
        PowerCell __instance,
        PlayerChoiceContext choiceContext,
        CombatSide side,
        CombatState combatState
    )
    {
        if (CombatManager.Instance == null || !CombatManager.Instance.IsInProgress)
        {
            return;
        }

        if (side != __instance.Owner.Creature.Side)
        {
            return;
        }

        // Same gate the game uses before flashing / selecting cards.
        if (__instance.Owner.PlayerCombatState?.TurnNumber != 1)
        {
            return;
        }

        // BeforeSideTurnStart can be entered more than once; only count once per combat.
        if (!CombatStartManager.IsNewCombat(ref _lastCombatId))
        {
            return;
        }

        int maxCards = __instance.DynamicVars.Cards.IntValue;
        int eligibleZeroCostCards = PileType.Draw
            .GetPile(__instance.Owner)
            .Cards.Count(IsZeroCostCard);

        int cardsAdded = Math.Min(maxCards, eligibleZeroCostCards);
        RelicStatCache.RecordCustomStat(__instance.Id.Entry, new List<int> { cardsAdded });
    }

    /// <summary>
    /// Mirrors PowerCell's filter: not an X-cost, and global-modified energy cost is 0.
    /// </summary>
    private static bool IsZeroCostCard(CardModel card)
    {
        if (card.EnergyCost.CostsX)
        {
            return false;
        }

        return card.EnergyCost.GetWithModifiers(CostModifiers.Global) == 0;
    }
}
