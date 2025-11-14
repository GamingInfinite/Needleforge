using HarmonyLib;
using Needleforge.Data;
using UnityEngine;

namespace Needleforge.Patches;

[HarmonyPatch]
public class HighlightTools
{
    [HarmonyPatch(typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.RefreshTools), [typeof(bool), typeof(bool)])]
    [HarmonyPostfix]
    public static void RefreshTools(InventoryItemToolManager __instance)
    {
        for (int i = 0; i < __instance.listSectionHeaders.Length; i++)
        {
            Color color = __instance.listSectionHeaders[i].Color;
            ColorData headerData = null;
            if (i > 3)
            {
                headerData = NeedleforgePlugin.newColors[i - 4];
            }

            if (__instance.SelectedSlot && (int)__instance.SelectedSlot.Type > 3)
            {
                ColorData slotData =  NeedleforgePlugin.newColors[(int)__instance.SelectedSlot.Type - 4];
                bool forceIncludeHeader = false;
                if (headerData != null)
                {
                    forceIncludeHeader = headerData.allColorsValid;
                }

                if (!(slotData.ValidTypes.Contains((ToolItemType)i) || forceIncludeHeader || slotData.allColorsValid))
                {
                    color.a = 0.5f;
                }
                else
                {
                    color.a = 1f;
                }
                __instance.listSectionHeaders[i].Color = color;
            }
        }
    }

    [HarmonyPatch(typeof(InventoryItemTool), nameof(InventoryItemTool.UpdateEquippedDisplay))]
    [HarmonyPostfix]
    public static void UpdateEquippedDisplay(InventoryItemTool __instance)
    {
        if (__instance.manager.SelectedSlot != null)
        {
            if ((int)__instance.manager.SelectedSlot.Type > 3)
            {
                Color color;
                ColorData slotData = NeedleforgePlugin.newColors[(int)__instance.manager.SelectedSlot.Type - 4];
                ColorData itemData = null;
                if ((int)__instance.itemData.Type > 3)
                {
                    itemData = NeedleforgePlugin.newColors[(int)__instance.itemData.Type - 4];
                }
                
                bool forceItemAvailable = false;
                if (itemData != null)
                {
                    forceItemAvailable = itemData.allColorsValid;
                }
                if (!(slotData.ValidTypes.Contains(__instance.itemData.Type) || slotData.allColorsValid || forceItemAvailable))
                {
                    color = InventoryToolCrestSlot.InvalidItemColor;
                }
                else
                {
                    color = Color.white;
                }

                if (__instance.itemIcon != null)
                {
                    __instance.itemIcon.color = color;
                }
            }
        }
    }
}