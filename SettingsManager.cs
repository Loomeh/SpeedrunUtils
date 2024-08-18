using System.IO;
using System.Linq;

public class SettingsManager
{
    // Helper function to check and add setting if not present
    public static void CheckAndAddSetting(string filePath, string settingName, string defaultValue)
    {
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "");
        }

        var lines = File.ReadAllLines(filePath).ToList();
        var setting = lines.FirstOrDefault(l => l.StartsWith(settingName + ","));

        if (setting == null)
        {
            lines.Add($"{settingName},{defaultValue}");
            File.WriteAllLines(filePath, lines);
        }
    }

    // Helper function to read a setting
    public static string GetSetting(string filePath, string settingName, string defaultValue)
    {
        if (!File.Exists(filePath))
        {
            return defaultValue;
        }

        var lines = File.ReadAllLines(filePath);
        var setting = lines.FirstOrDefault(l => l.StartsWith(settingName + ","));

        if (setting != null)
        {
            var parts = setting.Split(',');
            if (parts.Length >= 2)
            {
                return parts[1];
            }
        }

        return defaultValue;
    }
}