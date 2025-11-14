using System.Collections.Generic;
using HarmonyLib;
using TeamCherry.NestedFadeGroup;
using UnityEngine;

namespace Needleforge.Patches;

[HarmonyPatch]
public class InventoryCloning
{
    [HarmonyPatch(typeof(InventoryToolCrest), nameof(InventoryToolCrest.OnValidate))]
    [HarmonyPostfix]
    public static void AddTemplateSlots(InventoryToolCrest __instance)
    {
        foreach (var color in NeedleforgePlugin.newColors)
        {
            if (color.isAttackType)
            {
                __instance.templateSlots[(int)color.type] = __instance.templateSlots[0];
            }
            else
            {
                __instance.templateSlots[(int)color.type] = __instance.templateSlots[1];
            }
        }
    }

    [HarmonyPatch(typeof(InventoryItemTool), nameof(InventoryItemTool.SetData))]
    [HarmonyPrefix]
    public static void AddAnimators(InventoryItemTool __instance, ToolItem newItemData)
    {
        List<RuntimeAnimatorController> newControllers = [..__instance.slotAnimatorControllers];
        foreach (var color in NeedleforgePlugin.newColors)
        {
            if (color.isAttackType)
            {
                newControllers.Add(__instance.slotAnimatorControllers[0]);
            }
            else
            {
                newControllers.Add(__instance.slotAnimatorControllers[1]);
            }
        }

        __instance.slotAnimatorControllers = newControllers.ToArray();
    }

    [HarmonyPatch(typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.OnValidate))]
    [HarmonyPostfix]
    public static void AddHeaders(InventoryItemToolManager __instance)
    {
        foreach (var color in NeedleforgePlugin.newColors)
        {
            NestedFadeGroupSpriteRenderer originalHeader = __instance.listSectionHeaders[1];
            NestedFadeGroupSpriteRenderer header = Object.Instantiate(originalHeader, originalHeader.transform.parent);
            header.name = color.name;
            if (color.header != null)
            {
                header.Sprite = color.header;
            }
            __instance.listSectionHeaders[(int)color.type] = header;
        }
    }
}