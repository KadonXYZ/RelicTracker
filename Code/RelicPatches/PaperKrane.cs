using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

/// <summary>
/// Paper Krane: 0.75 → 0.60 Weak multiplier. Stage on Weak math, commit on real hits
/// (intents also use CardPreviewMode.None).
/// </summary>
[HarmonyPatch(typeof(WeakPower), nameof(WeakPower.ModifyDamageMultiplicative))]
public static class PaperKranePatch
{
    [ThreadStatic] private static int Depth;
    [ThreadStatic] private static decimal DamageBefore;
    [ThreadStatic] private static PaperKrane? Krane;
    [ThreadStatic] private static decimal MultBefore;
    [ThreadStatic] private static decimal MultAfter;
    [ThreadStatic] private static PaperKrane? Pending;
    [ThreadStatic] private static int PendingAmount;

    static void Prefix(decimal amount)
    {
        if (Depth++ == 0)
        {
            Krane = null;
            DamageBefore = amount;
            MultBefore = MultAfter = 0m;
        }
    }

    static void Postfix(Creature target, ValueProp props)
    {
        try
        {
            if (Depth != 1 || Krane == null || PaperPhrogPreviewGuard.IsPreview)
                return;
            if (CombatManager.Instance?.IsInProgress != true)
                return;
            if (target != Krane.Owner?.Creature || !props.IsPoweredAttackRelicTracker())
                return;

            int mitigated =
                (int)decimal.Floor(DamageBefore * MultBefore)
                - (int)decimal.Floor(DamageBefore * MultAfter);

            if (mitigated <= 0)
                return;

            Pending = Krane;
            PendingAmount = mitigated;
        }
        finally
        {
            if (--Depth <= 0)
            {
                Depth = 0;
                Krane = null;
            }
        }
    }

    internal static void MarkApplied(PaperKrane krane, decimal before, decimal after)
    {
        Krane = krane;
        MultBefore = before;
        MultAfter = after;
    }

    internal static void CommitPending(Creature target, ValueProp props)
    {
        if (Pending == null || PendingAmount <= 0
            || target != Pending.Owner?.Creature
            || !props.IsPoweredAttackRelicTracker())
        {
            Pending = null;
            PendingAmount = 0;
            return;
        }

        RelicStatCache.RecordCustomStat(Pending.Id.Entry, new List<int> { PendingAmount });
        Pending = null;
        PendingAmount = 0;
    }
}

[HarmonyPatch(typeof(PaperKrane), nameof(PaperKrane.ModifyWeakMultiplier))]
public static class PaperKraneAppliedPatch
{
    static void Postfix(PaperKrane __instance, decimal amount, decimal __result)
    {
        if (__result != amount)
            PaperKranePatch.MarkApplied(__instance, amount, __result);
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterDamageReceived))]
public static class PaperKraneCommitPatch
{
    static void Postfix(Creature target, ValueProp props)
        => PaperKranePatch.CommitPending(target, props);
}
