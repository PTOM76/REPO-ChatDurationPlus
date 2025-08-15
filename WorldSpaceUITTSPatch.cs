using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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

    [HarmonyTranspiler, HarmonyPatch(nameof(WorldSpaceUITTS.Update))]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        var codes = new List<CodeInstruction>(instructions);
        var isExtendFunc = AccessTools.Method(typeof(WorldSpaceUITTSPatch), nameof(isExtend));

        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Call && ((MethodInfo) codes[i].operand) == AccessTools.Method(typeof(Object), nameof(Object.Destroy), new[] { typeof(Object) }))
            {
                var skipLabel = il.DefineLabel();
                var nop = new CodeInstruction(OpCodes.Nop) { labels = new List<Label> { skipLabel } };
                codes.Insert(i + 2, nop);
                codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(i + 3, new CodeInstruction(OpCodes.Call, isExtendFunc));
                codes.Insert(i + 4, new CodeInstruction(OpCodes.Brtrue_S, skipLabel));
            }
        }

        return codes;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(WorldSpaceUITTS.Update))]
    static void PostfixUpdate(WorldSpaceUITTS __instance)
    {
        if (timers.TryGetValue(__instance, out TimerData timerData) && timerData.extraTime > 0f)
        {
            __instance.textAlphaTarget = 1f;

            if (__instance.followTransform)
            {
                __instance.followPosition = Vector3.Lerp(__instance.followPosition, __instance.followTransform.position, 10f * Time.deltaTime);
                __instance.worldPosition = __instance.followPosition + __instance.curveIntro.Evaluate(__instance.curveLerp) * Vector3.up * 0.025f;
            }

            __instance.text.color = new Color(__instance.textColor.r, __instance.textColor.g, __instance.textColor.b, __instance.textAlpha);
        }
    }
}
