using HarmonyLib;
using TeamCherry.NestedFadeGroup;
using UnityEngine;

namespace Needleforge.Patches.Inventory;

[HarmonyPatch]
internal class InventoryCloning
{
    [HarmonyPatch(typeof(InventoryToolCrest), nameof(InventoryToolCrest.OnValidate))]
    [HarmonyPostfix]
    private static void AddTemplateSlots(InventoryToolCrest __instance)
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

    [HarmonyPatch(typeof(InventoryItemTool), nameof(InventoryItemTool.OnValidate))]
    [HarmonyPostfix]
    private static void AddAnimators(InventoryItemTool __instance)
    {
        foreach (var color in NeedleforgePlugin.newColors)
        {
            RuntimeAnimatorController controller =
                __instance.slotAnimatorControllers[color.isAttackType ? 0 : 1];

            __instance.slotAnimatorControllers[(int)color.type] = controller;
        }
    }

    [HarmonyPatch(typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.OnValidate))]
    [HarmonyPostfix]
    private static void AddHeaders(InventoryItemToolManager __instance)
    {
        foreach (var color in NeedleforgePlugin.newColors)
        {
            if (__instance.listSectionHeaders[(int)color.type]) // To avoid duplicate header objects
            {
                continue;
            }

            NestedFadeGroupSpriteRenderer originalHeader = __instance.listSectionHeaders[1];
            NestedFadeGroupSpriteRenderer header = Object.Instantiate(originalHeader, originalHeader.transform.parent);
            header.name = $"{color.name} Section Header";
            if (color.header != null)
            {
                header.Sprite = color.header;
            }
            __instance.listSectionHeaders[(int)color.type] = header;
        }
    }
}