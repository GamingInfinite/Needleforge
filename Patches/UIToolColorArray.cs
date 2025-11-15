using GlobalSettings;
using HarmonyLib;
using UnityEngine;

namespace Needleforge.Patches;

[HarmonyPatch(typeof(UI), nameof(UI.GetToolTypeColor))]
public class UIToolColorArray
{
    [HarmonyPrefix]
    public static bool Prefix(ToolItemType type, ref Color __result)
    {
        if ((int)type > 3)
        {
            __result = NeedleforgePlugin.newColors[(int)type - 4].color;
            return false;
        }
        return true;
    }
}