using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyDamage))]
public static class PaperPhrogPreviewGuard
{
    [ThreadStatic]
    private static int PreviewDepth;

    public static bool IsPreview => PreviewDepth > 0;

    static void Prefix(CardPreviewMode previewMode, ref bool __state)
    {
        if (previewMode == CardPreviewMode.None)
        {
            PreviewDepth = 0;
            __state = false;
            return;
        }

        __state = true;
        PreviewDepth++;
    }

    static void Postfix(bool __state)
    {
        if (__state && PreviewDepth > 0)
        {
            PreviewDepth--;
        }
    }
}

[HarmonyPatch(typeof(VulnerablePower), nameof(VulnerablePower.ModifyDamageMultiplicative))]
public static class PaperPhrogPatch
{
    [ThreadStatic]
    private static int VulnDepth;

    [ThreadStatic]
    private static bool PhrogApplied;

    [ThreadStatic]
    private static PaperPhrog? PhrogInstance;

    [ThreadStatic]
    private static decimal DamageBeforeVulnerable;

    [ThreadStatic]
    private static decimal MultBeforePhrog;

    [ThreadStatic]
    private static decimal MultAfterPhrog;

    static void Prefix(decimal amount)
    {
        if (VulnDepth == 0)
        {
            PhrogApplied = false;
            PhrogInstance = null;
            DamageBeforeVulnerable = amount;
            MultBeforePhrog = 0m;
            MultAfterPhrog = 0m;
        }

        VulnDepth++;
    }

    static void Postfix(
        VulnerablePower __instance,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        decimal __result
    )
    {
        try
        {
            if (VulnDepth != 1)
            {
                return;
            }

            if (!PhrogApplied || PhrogInstance == null)
            {
                return;
            }

            if (PaperPhrogPreviewGuard.IsPreview)
            {
                return;
            }

            if (CombatManager.Instance == null || !CombatManager.Instance.IsInProgress)
            {
                return;
            }

            if (target != __instance.Owner)
            {
                return;
            }

            if (!props.IsPoweredAttackRelicTracker())
            {
                return;
            }

            // Attribute only the damage change from Phrog's own multiplier bump.
            int bonusDamage =
                (int)decimal.Floor(DamageBeforeVulnerable * MultAfterPhrog)
                - (int)decimal.Floor(DamageBeforeVulnerable * MultBeforePhrog);

            if (bonusDamage <= 0)
            {
                return;
            }

            RelicStatCache.RecordCustomStat(PhrogInstance.Id.Entry, new List<int> { bonusDamage });
        }
        finally
        {
            VulnDepth = Math.Max(0, VulnDepth - 1);
            if (VulnDepth == 0)
            {
                PhrogApplied = false;
                PhrogInstance = null;
            }
        }
    }

    internal static void MarkApplied(PaperPhrog phrog, decimal multBefore, decimal multAfter)
    {
        PhrogApplied = true;
        PhrogInstance = phrog;
        MultBeforePhrog = multBefore;
        MultAfterPhrog = multAfter;
    }
}

[HarmonyPatch(typeof(PaperPhrog), nameof(PaperPhrog.ModifyVulnerableMultiplier))]
public static class PaperPhrogAppliedPatch
{
    static void Postfix(PaperPhrog __instance, decimal amount, decimal __result)
    {
        if (__result == amount)
        {
            return;
        }

        PaperPhrogPatch.MarkApplied(__instance, amount, __result);
    }
}
