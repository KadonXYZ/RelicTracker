using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;

[HarmonyPatch(typeof(RelicModel), nameof(RelicModel.Flash), new Type[0])]
public static class RelicFlashPatch
{
    // This "Prefix" runs BEFORE the actual Flash occurs
    static void Prefix(RelicModel __instance)
    {
        // Additional hail mary check
        if (!__instance.IsMutable)
        {
            return;
        }

        // __instance is the specific relic that just activated (e.g., Burning Blood, Anchor)
        string relicId = __instance.Id.Entry;

        // Save to your JSON or Dictionary here
        RelicStatCache.RecordTriggerStat(relicId);
    }
}

[HarmonyPatch(
    typeof(Player),
    "CreateForNewRun",
    
    new Type[] { typeof(CharacterModel), typeof(UnlockState), typeof(ulong) }
)]
public static class PlayerCreationPatch
{
    static void Postfix(Player __result)
    {
        if(!RelicStatCache.ShouldIgnoreNextCreation())
        {
            RelicStatCache.InitializeForNewRun();
        }
    }
}

[HarmonyPatch(typeof(RelicModel), "get_HoverTip")]
public static class RelicTooltipPatch
{
    public static void Postfix(RelicModel __instance, ref HoverTip __result)
    {
        if (!RelicTracker.RelicTrackerSettings.ShouldShowTooltips)
        {
            return;
        }

        if (__instance?.Id == null || string.IsNullOrEmpty(__instance.Id.Entry))
        {
            return;
        }

        // If the localized string is explicitly empty, do not show any tooltip addition.
        string locTest = LocalizationHelper.GetLocalizedString(__instance.Id.Entry);
        if (locTest == "")
        {
            return;
        }

        string newDescription;

        if (RelicStatCache.HasStatsForRelic(__instance.Id.Entry))
        {
            List<int> customValues = RelicStatCache.GetCustomValues(__instance.Id.Entry);
            List<string> additionalValues = RelicStatCache.GetAdditionalValues(__instance.Id.Entry);

            if (customValues != null && customValues.Count > 0)
            {
                // Has specific detailed stats. Fetch template from localization
                string locText = LocalizationHelper.GetLocalizedString(__instance.Id.Entry);

                if (string.IsNullOrWhiteSpace(locText))
                {
                    // Fall back to a missing loc string
                    newDescription = __result.Description + "\n\n[red][Relic Tracker][/red]\n[gold]Missing Localization[/gold]";
                }
                else
                {
                    try
                    {
                        // Box the ints to objects so string.Format can process them
                        object[] formattedValues = new object[customValues.Count];
                        for (int i = 0; i < customValues.Count; i++) formattedValues[i] = customValues[i];

                        string detailedStats = string.Format(locText, formattedValues);

                        if(additionalValues != null)
                        {
                            string additionalText = string.Join("\n",additionalValues);
                            newDescription = __result.Description + "\n\n[red][Relic Tracker][/red]\n" + detailedStats + "\n\n" + additionalText;
                        } else
                        {
                            newDescription = __result.Description + "\n\n[red][Relic Tracker][/red]\n" + detailedStats;
                        }

                        
                    }
                    catch (System.FormatException)
                    {
                        newDescription = __result.Description + "\n\n[red][Relic Tracker][/red]\n[gold]Format Error in .loc file[/gold]";
                    }
                }
            }
            else
            {
                // Does not have any
                //  specific detailed stats to show. Let's go try and fetch a label
                int triggerCount = RelicStatCache.GetTriggeredCount(__instance.Id.Entry);
                string alternateLabel = RelicLabelRenamer.GetAlternateLabel(__instance.Id.Entry, triggerCount);

                if (!string.IsNullOrEmpty(alternateLabel))
                {
                    // Has an alternate label to show. Show that instead of the raw trigger count.
                    newDescription = __result.Description + "\n\n[red][Relic Tracker][/red]\n" + alternateLabel;
                }
                else
                {
                    // No alternate label. Just show the raw trigger count.
                    string defaultLabel = LocalizationHelper.GetLocalizedDefault(triggerCount);
                    newDescription = __result.Description + $"\n\n[red][Relic Tracker][/red]\n" + defaultLabel;
                }
            }
        }
        else
        {
            if (!RelicTracker.RelicTrackerSettings.ShowNoDataYet)
            {
                return;
            }

            // No data yet. Just add a note about that.
            newDescription = __result.Description + "\n\n[red][Relic Tracker][/red]\n" + LocalizationHelper.GetLocalizedNoDataYet();
        }

        // Box the struct into an object so Reflection can modify it
        object customHoverTip = __result;

        // Grab the property using Harmony's AccessTools to bypass 'private set'
        PropertyInfo descriptionProp = AccessTools.Property(typeof(HoverTip), "Description");

        // Set the value directly on the property
        descriptionProp.SetValue(customHoverTip, newDescription);

        // Unbox it back into the HoverTip object
        __result = (HoverTip)customHoverTip;
    }
}
