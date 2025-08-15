using HarmonyLib;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ChatDurationPlus;

[HarmonyPatch(typeof(WorldSpaceUITTS))]
static class WorldSpaceUITTSPatch
{
    private static readonly ConditionalWeakTable<WorldSpaceUITTS, TimerData> timers = new ConditionalWeakTable<WorldSpaceUITTS, TimerData>();
    private static readonly FieldInfo textColorField = AccessTools.Field(typeof(WorldSpaceUITTS), "textColor");
    private static readonly FieldInfo textAlphaField = AccessTools.Field(typeof(WorldSpaceUITTS), "textAlpha");
    private static readonly FieldInfo textAlphaTargetField = AccessTools.Field(typeof(WorldSpaceUITTS), "textAlphaTarget");
    private static readonly FieldInfo curveLerpField = AccessTools.Field(typeof(WorldSpaceUITTS), "curveLerp");
    private static readonly FieldInfo followPositionField = AccessTools.Field(typeof(WorldSpaceUITTS), "followPosition");
    private static readonly FieldInfo worldPositionField = AccessTools.Field(typeof(WorldSpaceUITTS), "worldPosition");

    public static bool isExtend(WorldSpaceUITTS instance)
    {
        if (instance.ttsVoice != null && instance.ttsVoice.isSpeaking)
        {
            timers.GetOrCreateValue(instance).reset();
            return true;
        }

        if (timers.TryGetValue(instance, out TimerData timerData) && timerData.extraTime > 0f)
        {
            timerData.extraTime -= Time.deltaTime;
            return true;
        }

        return false;
    }

    [HarmonyPrefix, HarmonyPatch(nameof(WorldSpaceUITTS.Update))]
    static void PrefixUpdate(WorldSpaceUITTS __instance)
    {
        if (isExtend(__instance))
        {
            textAlphaTargetField.SetValue(__instance, 1f);
        }
    }

    [HarmonyPostfix, HarmonyPatch(nameof(WorldSpaceUITTS.Update))]
    static void PostfixUpdate(WorldSpaceUITTS __instance)
    {
        if (timers.TryGetValue(__instance, out TimerData timerData) && timerData.extraTime > 0f)
        {
            textAlphaTargetField.SetValue(__instance, 1f);

            if (__instance.followTransform)
            {
                var pos = (Vector3)followPositionField.GetValue(__instance);
                var newPos = Vector3.Lerp(pos, __instance.followTransform.position, 10f * Time.deltaTime);
                followPositionField.SetValue(__instance, newPos);

                var curveLerp = (float)curveLerpField.GetValue(__instance);
                var newWorldPos = newPos + __instance.curveIntro.Evaluate(curveLerp) * Vector3.up * 0.025f;
                worldPositionField.SetValue(__instance, newWorldPos);
            }

            var color = (Color) textColorField.GetValue(__instance);
            var alpha = (float) textAlphaField.GetValue(__instance);
            __instance.text.color = new Color(color.r, color.g, color.b, alpha);
        }
    }
}
