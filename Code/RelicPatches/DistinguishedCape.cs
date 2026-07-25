
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

[HarmonyPatch]
public static class ApparitionCardPlayedPatch
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(
            typeof(Apparition),
            "OnPlay",
            [typeof(PlayerChoiceContext), typeof(CardPlay)]
        );
    }

    [HarmonyPrefix]
    public static void Prefix(Apparition __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if(LocalContext.IsMe(__instance.Owner))
        {
            RelicStatCache.RecordCustomStat("DISTINGUISHED_CAPE", new List<int> { 1 });
        }
    }

}

[HarmonyPatch(typeof(DistinguishedCape), nameof(DistinguishedCape.AfterObtained))]
public static class DistinguishedCapedTriggerPatch
{
    public static bool IsCapeRunning = false;

    static void Prefix(PandorasBox __instance)
    {
        IsCapeRunning = true;

    }

    static void Postfix()
    {
        IsCapeRunning = false;
    }
}


[HarmonyPatch(
    typeof(CardCmd),
    nameof(CardCmd.PreviewCardPileAdd),
    new Type[] { typeof(IReadOnlyList<CardPileAddResult>), typeof(float), typeof(CardPreviewStyle) }
)]
public static class AddCurseInformation
{
    
    public static void Prefix(IReadOnlyList<CardPileAddResult> results, float time = 1.2f, CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
    {
        if (DistinguishedCapedTriggerPatch.IsCapeRunning && results != null)
        {
            List<string> res = new List<string>();
            res.Add("Added these curses:");
            foreach (CardPileAddResult result in results)
            {
                
                res.Add($"[red]{result.cardAdded.Title}[/red]");
                
            }
            
            RelicStatCache.RecordCustomStat("DISTINGUISHED_CAPE", new List<int> { 0 });
            RelicStatCache.RecordAdditionalStat("DISTINGUISHED_CAPE", res);
        }

        return;
    }

}