using System.IO;
using System.Globalization;
using BepInEx;

namespace ChatDurationPlus;

public class TimerData
{
    public float extraTime = 15f;
    public bool isInit = false;
    public string lastText = "";

    public static float defaultExtraTime = 15f;
    public static string path = Path.Combine(Paths.ConfigPath, "ChatDurationPlus.cfg");

    static TimerData()
    {
        
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            SaveConfig();
        }
        else
        {
            LoadConfig();
        }
    }

    private static void LoadConfig()
    {
        try
        {
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                string[] parts = line.Split('=');
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    if (key == "extraTime")
                    {
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedValue))
                        {
                            defaultExtraTime = parsedValue;
                        }
                    }
                }
            }
        }
        catch
        {
            // If loading fails, create a new config file
            SaveConfig();
        }
    }

    private static void SaveConfig()
    {
        try
        {
            string configContent = $"# ChatDurationPlus Configuration\n# Time in seconds to extend chat display duration\nextraTime={defaultExtraTime.ToString(CultureInfo.InvariantCulture)}";
            File.WriteAllText(path, configContent);
        }
        catch
        {
            // Ignore save errors
        }
    }

    public void reset()
    {
        extraTime = defaultExtraTime;
        isInit = false;
    }

    public TimerData()
    {
        extraTime = defaultExtraTime;
    }
}