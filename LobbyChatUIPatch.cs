using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ChatDurationPlus;

[HarmonyPatch(typeof(LobbyChatUI))]
internal static class LobbyChatUIPatch
{
    private static readonly ConditionalWeakTable<LobbyChatUI, TimerData> timers = new ConditionalWeakTable<LobbyChatUI, TimerData>();
    private static readonly FieldInfo ttsVoiceField = AccessTools.Field(typeof(LobbyChatUI), "ttsVoice");

    [HarmonyPostfix, HarmonyPatch(nameof(LobbyChatUI.Update))]
    private static void DisplayTimePatch(LobbyChatUI __instance)
    {
        TimerData timerData = timers.GetOrCreateValue(__instance);
        TTSVoice ttsVoice = (TTSVoice) ttsVoiceField.GetValue(__instance);

        if (ttsVoice == null) return;
        if (ttsVoice.isSpeaking)
        {
            timerData.reset();
            timerData.lastText = ttsVoice.voiceText;
            return;
        }

        if (timerData.extraTime > 0f)
        {
            timerData.extraTime -= Time.deltaTime;

            if (string.IsNullOrEmpty(__instance.uiText.text))
                __instance.uiText.text = timerData.lastText;
        }
    }
}
