using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Needleforge.Patches;

[HarmonyPatch]
public class InventoryPatches
{
    [HarmonyPatch(typeof(InventoryToolCrest), nameof(InventoryToolCrest.OnValidate))]
    [HarmonyPostfix]
    public static void AddTemplateSlots(InventoryToolCrest __instance)
    {
        foreach (var color in NeedleforgePlugin.newColors)
        {
            __instance.templateSlots[(int)color.type] = __instance.templateSlots[1];
        }
    }

    [HarmonyPatch(typeof(InventoryItemTool), nameof(InventoryItemTool.SetData))]
    [HarmonyPrefix]
    public static void AddAnimators(InventoryItemTool __instance, ToolItem newItemData)
    {
        List<RuntimeAnimatorController> newControllers = [..__instance.slotAnimatorControllers];
        foreach (var color in NeedleforgePlugin.newColors)
        {
            newControllers.Add(__instance.slotAnimatorControllers[1]);
        }

        __instance.slotAnimatorControllers = newControllers.ToArray();
    }

    [HarmonyPatch(typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.OnValidate))]
    [HarmonyPostfix]
    public static void AddHeaders(InventoryItemToolManager __instance)
    {
        foreach (var color in NeedleforgePlugin.newColors)
        {
            var og = __instance.listSectionHeaders[1];
            var obj = UnityEngine.Object.Instantiate(og, og.transform.parent);
            obj.name = color.name;
            __instance.listSectionHeaders[(int)color.type] = obj;
        }
    }
}