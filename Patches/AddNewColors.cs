using System;
using System.Collections.Generic;
using HarmonyLib;

namespace Needleforge.Patches;

[HarmonyPatch]
public class AddNewColors
{
    [HarmonyPatch(typeof(Enum), nameof(Enum.GetValues), typeof(Type))]
    [HarmonyPostfix]
    public static void AddNewColorsUnordered(Type enumType, ref Array __result)
    {
        if (enumType == typeof(ToolItemType))
        {
            List<ToolItemType> arrList = [];
            foreach (var color in __result)
            {
                arrList.Add((ToolItemType)color);
            }

            foreach (var color in NeedleforgePlugin.newColors)
            {
                arrList.Add(color.type);
            }

            __result = arrList.ToArray();
        }
    }

    [HarmonyPatch(typeof(EnumExtenstions), nameof(EnumExtenstions.GetValuesWithOrder), typeof(Type))]
    [HarmonyPostfix]
    public static void AddNewColorsOrdered(Type type, ref IEnumerable<int> __result)
    {
        if (type == typeof(ToolItemType))
        {
            for (int i = 0; i < NeedleforgePlugin.newColors.Count; i++)
            {
                int index = i + 4;
                __result = __result.AddItem(index);
            }
        }
    }
}