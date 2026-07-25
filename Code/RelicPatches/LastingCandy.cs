using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

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

    static void Prefix(LastingCandy __instance, RewardsSet rewards, CombatRoom room)
    {
        if (rewards.Player != __instance.Owner)
		{
			return;
		}
        if (rewards.Rewards.All((Reward r) => !(r is CardReward)))
		{
			return;
		}
        if (WillTrigger(__instance) && CombatStartManager.IsNewCombat(ref _lastCombatID))
        {
            RelicStatCache.RecordCustomStat(__instance.Id.Entry, new List<int> { 1 });
        } else
        {
            RelicStatCache.RecordCustomStat(__instance.Id.Entry, new List<int> { 0 });
        }
    }
}
