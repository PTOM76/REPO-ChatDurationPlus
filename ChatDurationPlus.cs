using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace ChatDurationPlus;

[BepInPlugin("Pitan.ChatDurationPlus", "ChatDurationPlus", "0.0.2")]
public class ChatDurationPlus : BaseUnityPlugin
{
    internal static ChatDurationPlus Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger => Instance._logger;
    private ManualLogSource _logger => base.Logger;
    internal Harmony? Harmony { get; set; }

    // Configuration properties
    public static ConfigEntry<float> ExtraTimeConfig { get; private set; } = null!;
    public static ConfigEntry<bool> EnablePluginConfig { get; private set; } = null!;
    public static ConfigEntry<bool> DebugModeConfig { get; private set; } = null!;

    private void Awake()
    {
        Instance = this;
        
        // Initialize configuration
        InitializeConfig();
        
        // Prevent the plugin from being deleted
        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;

        Patch();

        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
        Logger.LogInfo($"Extra chat time configured to: {ExtraTimeConfig.Value} seconds");
    }

    private void InitializeConfig()
    {
        // 时长配置
        ExtraTimeConfig = Config.Bind("General", 
            "ExtraTime", 
            15f, 
            "Additional time in seconds to extend chat display duration (default: 15)");

        // 插件开关
        EnablePluginConfig = Config.Bind("General", 
            "EnablePlugin", 
            true, 
            "Enable or disable the ChatDurationPlus functionality");

        // Debug Mode（暂时没用）
        DebugModeConfig = Config.Bind("Debug", 
            "DebugMode", 
            false, 
            "Enable debug mode for additional logging information");

        // 添加更改事件 Handler
        ExtraTimeConfig.SettingChanged += (_, _) => {
            Logger.LogInfo($"Extra time changed to: {ExtraTimeConfig.Value} seconds");
        };

        EnablePluginConfig.SettingChanged += (_, _) => {
            Logger.LogInfo($"Plugin enabled: {EnablePluginConfig.Value}");
        };

        DebugModeConfig.SettingChanged += (_, _) => {
            Logger.LogInfo($"Debug mode: {DebugModeConfig.Value}");
        };
    }

    internal void Patch()
    {
        Harmony ??= new Harmony(Info.Metadata.GUID);
        Harmony.PatchAll();
    }

    internal void Unpatch()
    {
        Harmony?.UnpatchSelf();
    }

    private void Update()
    {
        // Code that runs every frame goes here
    }
}
