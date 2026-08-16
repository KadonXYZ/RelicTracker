using System.Collections.Generic;

public static class RelicLabelRenamer
{
    private static readonly Dictionary<string, int> RelicMultipliers = new()
    {
        { "ANCHOR", 10 },
        { "CAPTAINS_WHEEL", 18 },
        { "CHANDELIER", 3 },
        { "EMBER_TEA", 2 },
        { "FUNERARY_MASK", 3 },
        { "GORGET", 4 },
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
        { "STONE_HUMIDIFIER", 5 },
    };

    public static string GetAlternateLabel(string relicId, int value)
    {
        string? locText = LocalizationHelper.GetLocalizedString(relicId);
        if (string.IsNullOrWhiteSpace(locText))
        {
            return "";
        }

        int multiplier = RelicMultipliers.GetValueOrDefault(relicId, 1);
        try
        {
            return string.Format(locText, value * multiplier);
        }
        catch (FormatException)
        {
            return "";
        }
    }
}
