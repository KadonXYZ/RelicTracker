using System.Collections.Generic;
using System.IO;
using System.Reflection;

public static class LocalizationHelper
{
    private static readonly Dictionary<string, string> LocalizedStrings = new();

    public static void SetLanguage(string language)
    {
        LocalizedStrings.Clear();

        string? assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (string.IsNullOrEmpty(assemblyFolder))
        {
            ModLog.Error(
                "[RelicTracker] Could not resolve mod assembly folder for localization.",
                new InvalidOperationException("Assembly location is empty")
            );
            return;
        }

        string locFilePath = Path.Combine(assemblyFolder, "Localization", $"{language}.loc");
        if (!File.Exists(locFilePath))
        {
            ModLog.Info(
                $"[RelicTracker] Localization file not found for {language}, falling back to eng.loc"
            );
            locFilePath = Path.Combine(assemblyFolder, "Localization", "eng.loc");
        }

        if (!File.Exists(locFilePath))
        {
            ModLog.Error(
                $"[RelicTracker] Base eng.loc not found at {locFilePath}!",
                new FileNotFoundException("Base localization file not found", locFilePath)
            );
            return;
        }

        foreach (string line in File.ReadAllLines(locFilePath))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
            {
                continue;
            }

            string[] parts = line.Split('|', 2);
            if (parts.Length == 2)
            {
                LocalizedStrings[parts[0].Trim()] = parts[1].Trim();
            }
        }

        ModLog.Info(
            $"[RelicTracker] Loaded {LocalizedStrings.Count} localized strings for {language}."
        );
    }

    public static string? GetLocalizedString(string key) =>
        LocalizedStrings.TryGetValue(key, out string? value) ? value.Replace("\\n", "\n") : null;

    public static string GetLocalizedDefault(int value)
    {
        // eng.loc uses DEFAULT_TOOLTIP; DEFAULT_LABEL is a legacy alias.
        string? locText = GetLocalizedString("DEFAULT_TOOLTIP") ?? GetLocalizedString("DEFAULT_LABEL");
        if (!string.IsNullOrWhiteSpace(locText))
        {
            return string.Format(locText, value);
        }

        return $"[gold]Times Triggered:[/gold] [blue]{value}[/blue].";
    }

    public static string GetLocalizedNoDataYet()
    {
        string? locText = GetLocalizedString("EMPTY_TOOLTIP");
        return !string.IsNullOrWhiteSpace(locText) ? locText : "No data to display...";
    }
}
