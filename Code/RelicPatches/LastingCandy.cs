using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

[HarmonyPatch(typeof(LastingCandy), nameof(LastingCandy.TryModifyCardRewardOptions))]
public static class LastingCandyPatch
{
    private static int _lastCombatID = -1;

    private static bool WillTrigger(LastingCandy __instance)
    {
        if (__instance.CombatRewardsSeen > 0)
        {
            return __instance.CombatRewardsSeen % 2 == 1;
        }
        return false;
    }

    static void Prefix(LastingCandy __instance, Player player, List<CardCreationResult> rewardOptions, CardCreationOptions creationOptions)
    {
       if (__instance.Owner != player)
		{
			return;
		}
		if (creationOptions.Source != CardCreationSource.Encounter)
		{
			return;
		}
		if (!WillTrigger(__instance))
		{
			return;
		}
		if (!creationOptions.Flags.HasFlag(CardCreationFlags.IsCardReward))
		{
			return;
		}
		if (!creationOptions.Flags.HasFlag(CardCreationFlags.IsFromCombat))
		{
			return;
		}
        if ( CombatStartManager.IsNewCombat(ref _lastCombatID))
        {
            RelicStatCache.RecordCustomStat(__instance.Id.Entry, new List<int> { 1 });
        } else
        {
            RelicStatCache.RecordCustomStat(__instance.Id.Entry, new List<int> { 0 });
        }
    }
}
