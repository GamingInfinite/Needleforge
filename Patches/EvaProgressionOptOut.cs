using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Needleforge.Patches;

[HarmonyPatch(typeof(CountCrestUnlockPoints), nameof(CountCrestUnlockPoints.OnEnter))]
internal static class EvaProgressionOptOut
{
    private static void Prefix(CountCrestUnlockPoints __instance)
    {
        ToolCrestList list = ScriptableObject.CreateInstance<ToolCrestList>();

        HashSet<ToolCrest> crestsToRemove = [..
            from x in NeedleforgePlugin.newCrestData
            where !x.slotsCountForEvaQuest
            select x.ToolCrest!
        ];

        foreach (ToolCrest crest in (ToolCrestList)__instance.CrestList.Value)
        {
            if (!crestsToRemove.Contains(crest))
                list.Add(crest);
        }

        __instance.CrestList.Value = list;
    }
}
