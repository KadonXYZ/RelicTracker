using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;

[HarmonyPatch(typeof(TinyMailbox), nameof(TinyMailbox.TryModifyRestSiteHealRewards))]
public static class TinyMailboxPatch
{
    static void Postfix(
        TinyMailbox __instance,
        Player player,
        List<Reward> rewards,
        bool isMimicked,
        bool __result
    )
    {
        if (!__result)
        {
            return;
        }

        if (player != __instance.Owner)
        {
            return;
        }

        // Tiny Mailbox adds two PotionRewards to the rest-site heal options.
        RelicStatCache.RecordCustomStat(__instance.Id.Entry, new List<int> { 2 });
    }
}
