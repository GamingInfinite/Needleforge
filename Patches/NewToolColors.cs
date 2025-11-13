using System;
using System.Collections.Generic;
using GlobalSettings;
using HarmonyLib;
using UnityEngine;

namespace Needleforge.Patches;

[HarmonyPatch(typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.Awake))]
public class AddHeaders
{
    [HarmonyPostfix]
    public static void Postfix(InventoryItemToolManager __instance)
    {
        foreach (var color in NeedleforgePlugin.newColors) 
        {
            __instance.listSectionHeaders[(int)color.type] = __instance.listSectionHeaders[1];
        }
    }
}

[HarmonyPatch(typeof(InventoryItemTool), nameof(InventoryItemTool.SetData))]
public class AddAnimators
{
    [HarmonyPrefix]
    public static void Postfix(InventoryItemTool __instance, ToolItem newItemData)
    {
        List<RuntimeAnimatorController> newControllers = [..__instance.slotAnimatorControllers];
        foreach (var color in NeedleforgePlugin.newColors)
        {
            newControllers.Add(__instance.slotAnimatorControllers[1]);
        }
        __instance.slotAnimatorControllers = newControllers.ToArray();
    }
}

[HarmonyPatch(typeof(UI), nameof(UI.GetToolTypeColor))]
public class NewToolColors
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

[HarmonyPatch(typeof(EnumExtenstions), nameof(EnumExtenstions.GetValuesWithOrder), typeof(Type))]
public class ToolItemTypeEnumPatch
{
    [HarmonyPostfix]
    public static void Postfix(Type type, ref IEnumerable<int> __result)
    {
        if (type == typeof(ToolItemType))
        {
            for (int i = 0; i < NeedleforgePlugin.newColors.Count; i++)
            {
                int index = i + 4;
                __result.AddItem(index);
            }
        }
    }
}

[HarmonyPatch(typeof(Enum), nameof(Enum.GetValues), typeof(Type))]
public class ToolItemTypePatch2
{
    [HarmonyPostfix]
    public static void Postfix(Type enumType, ref Array __result)
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
}

[HarmonyPatch(typeof(InventoryToolCrest), nameof(InventoryToolCrest.OnValidate))]
public class InventoryToolCrestPatches
{
    [HarmonyPostfix]
    public static void Postfix(InventoryToolCrest __instance)
    {
        // OnValidate already expands the length of the list to accomodate the new
        // length of the enum, it just doesn't populate the empty spaces.
        foreach (var color in NeedleforgePlugin.newColors) 
        {
            __instance.templateSlots[(int)color.type] = __instance.templateSlots[1];
        }
    }
}