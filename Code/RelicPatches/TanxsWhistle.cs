using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;

[HarmonyPatch(typeof(Whistle), "OnPlay")]
public static class TanxsWhistlePlayedPatch
{
    static void Postfix(Whistle __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        RelicStatCache.RecordCustomStat(
            "TANXS_WHISTLE",
            new List<int> { 1 }
        );
    }
}