using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

[HarmonyPatch]
public static class WishCardPlayedPatch
{
    
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(
            typeof(Wish),
            "OnPlay",
            [typeof(PlayerChoiceContext), typeof(CardPlay)]
        );
    }

    [HarmonyPrefix]
    public static void Prefix(Wish __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if(LocalContext.IsMe(__instance.Owner))
        {
            RelicStatCache.RecordCustomStat("SERE_TALON", new List<int> { 1 });
        }
    }
}