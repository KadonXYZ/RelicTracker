using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;

[HarmonyPatch(typeof(Relax), "OnPlay")]
public static class PaelsRelaxPlayedPatch
{
    static void Postfix(Whistle __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        RelicStatCache.RecordCustomStat(
            "PAELS_HORN",
            new List<int> { 1 }
        );
    }
}