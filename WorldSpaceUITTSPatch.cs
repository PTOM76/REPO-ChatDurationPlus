using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ChatDurationPlus;

[HarmonyPatch(typeof(WorldSpaceUITTS))]
static class WorldSpaceUITTSPatch
{
    private static readonly ConditionalWeakTable<WorldSpaceUITTS, TimerData> timers = new ConditionalWeakTable<WorldSpaceUITTS, TimerData>();

    [HarmonyPrefix, HarmonyPatch(nameof(WorldSpaceUITTS.Update))]
    private static bool DisplayTimePatch(WorldSpaceUITTS __instance)
    {
        TimerData timerData = timers.GetOrCreateValue(__instance);

        if (__instance.ttsVoice != null && __instance.ttsVoice.isSpeaking) 
            return true;

        if (timerData.extraTime > 0f)
        {
            timerData.extraTime -= Time.deltaTime;
            __instance.UpdatePositionAndAlpha();
            return false;
        }

        return true;
    }
}

public static class WorldSpaceUITTS_Extension
{
    public static void UpdatePositionAndAlpha(this WorldSpaceUITTS __instance)
    {
        var textAlphaField = typeof(WorldSpaceUITTS).GetField("textAlpha", BindingFlags.NonPublic | BindingFlags.Instance);
        var textColorField = typeof(WorldSpaceUITTS).GetField("textColor", BindingFlags.NonPublic | BindingFlags.Instance);

        if (textAlphaField == null || textColorField == null) return;

        float textAlpha = (float) textAlphaField.GetValue(__instance);
        textAlpha = Mathf.Lerp(textAlpha, 1f, 30f * Time.deltaTime);
        textAlphaField.SetValue(__instance, textAlpha);

        Color textColor = (Color) textColorField.GetValue(__instance);
        __instance.text.color = new Color(textColor.r, textColor.g, textColor.b, textAlpha);

        if (__instance.followTransform)
            __instance.followPosition = __instance.followTransform.position;
        __instance.worldPosition = __instance.followPosition;
    }
}