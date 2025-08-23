using System.IO;
using System.Globalization;
using BepInEx;

namespace ChatDurationPlus;

public class TimerData
{
    public float extraTime = 15f;
    public bool isInit = false;
    public string lastText = "";

    public TimerData()
    {
        // 使用 BepInEx 配置而不是 static defaultExtraTime
        extraTime = ChatDurationPlus.ExtraTimeConfig?.Value ?? 15f;
    }

    public void reset()
    {
        extraTime = ChatDurationPlus.ExtraTimeConfig?.Value ?? 15f;
        isInit = false;
    }
    public float GetExtraTime()
    {
        return ChatDurationPlus.ExtraTimeConfig?.Value ?? extraTime;
    }
    public bool IsPluginEnabled()
    {
        return ChatDurationPlus.EnablePluginConfig?.Value ?? true;
    }
    public bool IsDebugMode()
    {
        return ChatDurationPlus.DebugModeConfig?.Value ?? false;
    }
}

