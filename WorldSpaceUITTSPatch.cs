using HarmonyLib;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ChatDurationPlus;

[HarmonyPatch(typeof(WorldSpaceUITTS))]
static class WorldSpaceUITTSPatch
{
    private static readonly ConditionalWeakTable<WorldSpaceUITTS, TimerData> timers = new ConditionalWeakTable<WorldSpaceUITTS, TimerData>();

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

    [HarmonyPostfix, HarmonyPatch("Update")]
    static void PostfixUpdate(WorldSpaceUITTS __instance)
    {
        if (isExtend(__instance))
        {
            // 延長中は強制的に表示を維持
            var textAlphaTargetField = AccessTools.Field(typeof(WorldSpaceUITTS), "textAlphaTarget");
            var textAlphaField = AccessTools.Field(typeof(WorldSpaceUITTS), "textAlpha");
            var textColorField = AccessTools.Field(typeof(WorldSpaceUITTS), "textColor");

            textAlphaTargetField.SetValue(__instance, 1f);

            // textAlphaを直接1.0に近づける
            var currentAlpha = (float)textAlphaField.GetValue(__instance);
            var newAlpha = Mathf.Lerp(currentAlpha, 1f, 30f * Time.deltaTime);
            textAlphaField.SetValue(__instance, newAlpha);

            // テキスト色を強制更新
            var color = (Color)textColorField.GetValue(__instance);
            __instance.text.color = new Color(color.r, color.g, color.b, newAlpha);
        }
    }
}

// Object.Destroyをブロックするパッチ
[HarmonyPatch(typeof(Object), "Destroy", typeof(Object))]
static class ObjectDestroyPatch
{
    static bool Prefix(Object obj)
    {
        if (obj is GameObject gameObject)
        {
            var worldSpaceUI = gameObject.GetComponent<WorldSpaceUITTS>();
            if (worldSpaceUI != null && WorldSpaceUITTSPatch.isExtend(worldSpaceUI))
            {
                // 延長中はDestroyをブロック
                return false;
            }
        }
        return true;
    }
}
