using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Needleforge.Data;

namespace Needleforge.Patches.Inventory;

[HarmonyPatch]
internal class MultiSlotNav
{
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(InventoryItemSelectableDirectional),
        nameof(InventoryItemSelectableDirectional.GetNextSelectable), typeof(InventoryItemManager.SelectionDirection))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static InventoryItemSelectable BaseGetNextSelectable(InventoryItemTool instance,
        InventoryItemManager.SelectionDirection direction) => null;

    [HarmonyPatch(typeof(InventoryItemTool), nameof(InventoryItemTool.GetNextSelectable))]
    [HarmonyPostfix]
    private static void MultiColorNav(InventoryItemManager.SelectionDirection direction, InventoryItemTool __instance,
        ref InventoryItemSelectable __result)
    {
        if (__instance == __result)
        {
            InventoryItemSelectable nextSelectable = BaseGetNextSelectable(__instance, direction);
            InventoryItemTool inventoryItemTool = nextSelectable as InventoryItemTool;
            if (inventoryItemTool == null)
            {
                return;
            }

            if ((int)__instance.manager.SelectedSlot.Type > 3)
            {
                ColorData data = NeedleforgePlugin.newColors[(int)__instance.manager.SelectedSlot.Type - 4];
                ColorData toolData = null;
                if ((int)inventoryItemTool.itemData.Type > 3)
                {
                    toolData = NeedleforgePlugin.newColors[(int)inventoryItemTool.itemData.Type - 4];
                }

                bool forceItemAvailable = false;
                if (toolData != null)
                {
                    forceItemAvailable = toolData.allColorsValid;
                }

                if (data.ValidTypes.Contains(inventoryItemTool.ToolType) || forceItemAvailable || data.allColorsValid)
                {
                    __result = nextSelectable;
                }
            }
        }
    }

    [HarmonyPatch(typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.EndSelection))]
    [HarmonyPrefix]
    private static bool MultiColorEndSelection(InventoryItemTool tool, InventoryItemToolManager __instance)
    {
        if ((int)__instance.SelectedSlot.Type > 3)
        {
            if (!__instance.SelectedSlot)
            {
                return true;
            }

            ColorData slotData = NeedleforgePlugin.newColors[(int)__instance.SelectedSlot.Type - 4];
            if ((bool)tool && (bool)tool.ItemData &&
                (slotData.ValidTypes.Contains(tool.ToolType) || slotData.allColorsValid))
            {
                if ((bool)__instance.tweenTool)
                {
                    __instance.SelectedSlot.SetEquipped(tool.ItemData, isManual: true, refreshTools: true);
                    __instance.tweenTool.DoPlace(tool.transform.position, __instance.SelectedSlot.transform.position,
                        tool.ItemData, SelectionEnd);
                    return false;
                }

                __instance.SelectedSlot.SetEquipped(tool.ItemData, isManual: true, refreshTools: true);
            }

            SelectionEnd();

            void SelectionEnd()
            {
                __instance.PlayMoveSound();
                __instance.SetSelected(__instance.SelectedSlot, null);
                __instance.SelectedSlot = null;
                __instance.EquipState = InventoryItemToolManager.EquipStates.None;
                __instance.RefreshTools();
            }

            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.StartSelection))]
    [HarmonyPrefix]
    private static bool MultiColorStartSelection(InventoryToolCrestSlot slot, InventoryItemToolManager __instance)
    {
        if ((int)slot.Type > 3)
        {
            if (__instance.toolList == null)
            {
                return true;
            }

            ColorData slotData = NeedleforgePlugin.newColors[(int)slot.Type - 4];
            List<InventoryItemTool> tools = __instance.toolList.GetListItems<InventoryItemTool>(tool =>
                (slotData.ValidTypes.Contains(tool.ToolType) || slotData.allColorsValid) && !tool.itemData.IsEquipped);
            InventoryItemTool firstTool = null;
            if (tools.Count > 0)
            {
                firstTool = tools[0];
            }

            if (firstTool == null)
            {
                return false;
            }

            __instance.SelectedSlot = slot;
            __instance.EquipState = InventoryItemToolManager.EquipStates.SelectTool;
            __instance.PlayMoveSound();
            __instance.SetSelected(firstTool, null);
            __instance.RefreshTools();

            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(InventoryToolCrestSlot), nameof(InventoryToolCrestSlot.IsSlotInvalid))]
    [HarmonyPostfix]
    private static void MultiColorInvalid(ToolItemType type, InventoryToolCrestSlot nextSlot,
        InventoryToolCrestSlot __instance, ref bool __result)
    {
        if (__result)
        {
            ToolItemType selToolType = __instance.manager.PickedUpTool.Type;
            if (nextSlot.EquippedItem != null)
            {
                return;
            }

            if ((int)nextSlot.Type > 3)
            {
                ColorData nextData = NeedleforgePlugin.newColors[(int)nextSlot.Type - 4];

                __result = !(nextData.ValidTypes.Contains(selToolType) || nextData.allColorsValid) ||
                           nextSlot.IsLocked && !__instance.manager.CanUnlockSlot;
            }
        }
    }
}
