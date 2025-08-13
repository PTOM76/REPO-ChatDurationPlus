using System.IO;
using Newtonsoft.Json;
using BepInEx;

public class TimerData
{
    public float extraTime;

    public static float defaultExtraTime = 10f;
    public static string path = Path.Combine(Paths.ConfigPath, "ChatDurationPlus.json");

    static TimerData()
    {
        
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonConvert.SerializeObject(new { extraTime = defaultExtraTime }, Formatting.Indented));
        }
        else
        {
            try
            {
                string json = File.ReadAllText(path);
                var data = JsonConvert.DeserializeObject<TimerData>(json);
                if (data != null)
                    defaultExtraTime = data.extraTime;
            }
            catch
            {

            }
        }
    }

    public TimerData()
    {
        extraTime = defaultExtraTime;
    }
}