using System.Collections.Generic;
using System.IO;
using System.Reflection;

public static class LocalizationHelper
{
    //Assigns a key to the
    private static Dictionary<string, string> localizedStrings = new Dictionary<string, string>();

    public static void SetLanguage(string language)
    {
        localizedStrings.Clear();

        // Find the folder where the mod assembly is loaded
        string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string locFilePath = Path.Combine(assemblyFolder, "Localization", $"{language}.loc");

        if (!File.Exists(locFilePath))
        {
            ModLog.Info(
                $"[RelicTracker] Localization file not found for {language}, falling back to eng.loc"
            );
            locFilePath = Path.Combine(assemblyFolder, "Localization", "eng.loc");
        }

        if (File.Exists(locFilePath))
        {
            string[] lines = File.ReadAllLines(locFilePath);
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                    continue;

                string[] parts = line.Split(new char[] { '|' }, 2);
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim(); // Read the string, leaving it blank if it's empty
                    localizedStrings[key] = value;
                }
            }
            ModLog.Info(
                $"[RelicTracker] Loaded {localizedStrings.Count} localized strings for {language}."
            );
        }
        else
        {
            ModLog.Error(
                $"[RelicTracker] Base eng.loc not found at {locFilePath}!",
                new FileNotFoundException("Base localization file not found")
            );
        }
    }

    public static string GetLocalizedString(string key)
    {
        if (localizedStrings != null && localizedStrings.TryGetValue(key, out string value))
        {
            return value.Replace("\\n", "\n");
        }
        return null;
    }

    public static string GetLocalizedDefault(int value)
    {
        // eng.loc uses DEFAULT_TOOLTIP; keep DEFAULT_LABEL as a legacy alias.
        string locText = GetLocalizedString("DEFAULT_TOOLTIP") ?? GetLocalizedString("DEFAULT_LABEL");
        if (!string.IsNullOrWhiteSpace(locText))
        {
            return string.Format(locText.Replace("\\n", "\n"), value);
        }
        return $"[gold]Times Triggered:[/gold] [blue]{value}[/blue].";
    }

    public static string GetLocalizedNoDataYet()
    {
        string locText = GetLocalizedString("EMPTY_TOOLTIP");
        return !string.IsNullOrWhiteSpace(locText)
            ? locText.Replace("\\n", "\n")
            : "No data to display...";
    }
}
