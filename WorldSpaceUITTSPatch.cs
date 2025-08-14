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
            return false;
        }

        return true;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(WorldSpaceUITTS.Update))]
    private static void PostfixUpdateUI(WorldSpaceUITTS __instance)
    {
        TimerData timerData;
        if (timers.TryGetValue(__instance, out timerData) && timerData.extraTime > 0f)
            __instance.UpdatePositionAndAlpha(timerData);
    }
}

public static class WorldSpaceUITTS_Extension
{
    public static void UpdatePositionAndAlpha(this WorldSpaceUITTS __instance, TimerData timerData)
    {
        var textAlphaField = typeof(WorldSpaceUITTS).GetField("textAlpha", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var textColorField = typeof(WorldSpaceUITTS).GetField("textColor", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (textAlphaField == null || textColorField == null) return;

        float textAlpha = (float) textAlphaField.GetValue(__instance);
        float targetAlpha = (__instance.followTransform != null) ? 1f : 0f;

        textAlpha = Mathf.MoveTowards(textAlpha, targetAlpha, 30f * Time.deltaTime);
        textAlphaField.SetValue(__instance, textAlpha);

        Color textColor = (Color) textColorField.GetValue(__instance);
        __instance.text.color = new Color(textColor.r, textColor.g, textColor.b, textAlpha);

        var followPositionField = typeof(WorldSpaceUITTS).GetField("followPosition", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var worldPositionField = typeof(WorldSpaceUITTS).GetField("worldPosition", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (__instance.followTransform != null && followPositionField != null)
        {
            Vector3 currentFollowPos = (Vector3)followPositionField.GetValue(__instance);
            Vector3 targetPos = __instance.followTransform.position;

            if (!timerData.isInit)
            {
                currentFollowPos = targetPos;
                timerData.isInit = true;
            }

            Vector3 newFollowPos = Vector3.Lerp(currentFollowPos, targetPos, Mathf.Min(1f, 10f * Time.deltaTime));

            followPositionField.SetValue(__instance, newFollowPos);

            if (worldPositionField != null)
                worldPositionField.SetValue(__instance, newFollowPos);
        }
    }
}