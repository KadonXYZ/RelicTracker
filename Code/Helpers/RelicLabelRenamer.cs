using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;

public static class RelicLabelRenamer
{
    private static Dictionary<string, int> relicMultipliers = new Dictionary<string, int>()
    {
        { "ANCHOR", 10 },
        { "CAPTAINS_WHEEL", 18 },
        { "CHANDELIER", 3 },
        { "EMBER_TEA", 2 },
        { "FUNERARY_MASK", 3 },
        { "GORGET", 4 },
        {"DIAMOND_DIADEM", 20},
        { "HAND_DRILL", 2 },
        { "HORN_CLEAT", 14 },
        { "NINJA_SCROLL", 3 },
        { "ORNAMENTAL_FAN", 4 },
        { "PERMAFROST", 7 },
        { "REPTILE_TRINKET", 3 },
        { "RIPPLE_BASIN", 4 },
        { "RUNIC_CAPACITOR", 3 },
        { "SAI", 7 },
        { "SWORD_OF_JADE", 3 },
        { "VENERABLE_TEA_SET", 2 },
        { "CENTENNIAL_PUZZLE", 3 },
        { "STONE_CRACKER", 2 },
        { "STONE_HUMIDIFIER", 5},
    };

    public static string GetAlternateLabel(string relicId, int value)
    {
        string locText = LocalizationHelper.GetLocalizedString(relicId);

        if (string.IsNullOrWhiteSpace(locText))
        {
            return ""; // Hide tooltip strings that are intentionally left blank or missing
        }

        int multiplier = relicMultipliers.TryGetValue(relicId, out int storedMultiplier)
            ? storedMultiplier
            : 1;
        int displayValue = DefaultValueFormatter(value, multiplier);

        try
        {
            return string.Format(locText, displayValue);
        }
        catch (System.FormatException)
        {
            return "";
        }
    }

    private static int DefaultValueFormatter(int rawValue, int factor)
    {
        return rawValue * factor;
    }
}
