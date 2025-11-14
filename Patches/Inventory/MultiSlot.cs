using System.Collections.Generic;
using HarmonyLib;
using Needleforge.Data;

namespace Needleforge.Patches;

[HarmonyPatch]
public class MultiSlot
{
    //IsAttackType
    [HarmonyPatch(typeof(ToolItemTypeExtensions), nameof(ToolItemTypeExtensions.IsAttackType))]
    [HarmonyPostfix]
    public static void CustomColorAttackType(ToolItemType type, ref bool __result)
    {
        if (!__result && (int)type > 3)
        {
            ColorData color = NeedleforgePlugin.newColors[(int)type - 4];
            __result = color.isAttackType;
        }
    }

    [HarmonyPatch(typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.GetAvailableSlotCount))]
    [HarmonyPostfix]
    public static void GetAvailableSlotCountMultiColor(IEnumerable<InventoryToolCrestSlot> slots,
        ToolItemType? toolType, ref int __result)
    {
        int count = 0;
        foreach (var slot in slots)
        {
            if (!slot.IsLocked)
            {
                ToolItemType realToolType = toolType.GetValueOrDefault();
                if ((int)slot.Type > 3)
                {
                    ColorData color = NeedleforgePlugin.newColors[(int)slot.Type - 4];
                    if (color.ValidTypes.Contains(realToolType) || color.allColorsValid)
                    {
                        count++;
                    }
                }
                else if ((int)realToolType > 3)
                {
                    ColorData toolColor = NeedleforgePlugin.newColors[(int)realToolType - 4];
                    if (toolColor.ValidTypes.Contains(slot.Type) || toolColor.allColorsValid)
                    {
                        count++;
                    }
                }
            }
        }

        __result += count;
    }

    [HarmonyPatch(typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.GetAvailableSlot))]
    [HarmonyPostfix]
    public static void GetAvailableSlotMultiColor(IEnumerable<InventoryToolCrestSlot> slots, ToolItemType toolType,
        ref InventoryToolCrestSlot __result)
    {
        if (__result == null)
        {
            foreach (var slot in slots)
            {
                if (!slot.IsLocked)
                {
                    if ((int)slot.Type > 3)
                    {
                        ColorData color = NeedleforgePlugin.newColors[(int)slot.Type - 4];
                        if (color.ValidTypes.Contains(toolType) || color.allColorsValid)
                        {
                            if (!slot.EquippedItem)
                            {
                                __result = slot;
                            }
                        }
                    }
                    else if ((int)toolType > 3)
                    {
                        ColorData toolColor = NeedleforgePlugin.newColors[(int)toolType - 4];
                        if (toolColor.ValidTypes.Contains(slot.Type) || toolColor.allColorsValid)
                        {
                            if (!slot.EquippedItem)
                            {
                                __result = slot;
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.PlaceTool))]
    [HarmonyPrefix]
    public static bool PlaceMultiColorTool(InventoryItemToolManager __instance, InventoryToolCrestSlot slot,
        bool isManual)
    {
        void Selected()
        {
            __instance.SetSelected(__instance.selectedBeforePickup, null, false);
            __instance.selectedBeforePickup = null;
        }

        if (slot == null)
        {
            return true;
        }

        ToolItem tool;
        if ((int)slot.Type > 3)
        {
            ColorData color = NeedleforgePlugin.newColors[(int)slot.Type - 4];
            tool = __instance.PickedUpTool;
            if (color.ValidTypes.Contains(tool.Type) || color.allColorsValid)
            {
                RealPlace();
            }

            return false;
        }

        if ((int)__instance.PickedUpTool.Type > 3)
        {
            ColorData toolColor = NeedleforgePlugin.newColors[(int)__instance.PickedUpTool.Type - 4];
            if (toolColor.ValidTypes.Contains(slot.Type) || toolColor.allColorsValid)
            {
                tool = __instance.PickedUpTool;
                RealPlace();
            }

            return false;
        }

        void RealPlace()
        {
            __instance.PickedUpTool = null;
            __instance.EquipState = InventoryItemToolManager.EquipStates.None;

            if (isManual)
            {
                slot.SetEquipped(tool, true, true);
            }

            if (!__instance.selectedBeforePickup)
            {
                return;
            }

            if (isManual)
            {
                slot.PreOpenSlot();
            }

            if (__instance.tweenTool && slot)
            {
                __instance.tweenTool.DoPlace(__instance.selectedBeforePickup.transform.position,
                    slot.transform.position, tool, Selected);
                return;
            }

            Selected();
        }

        return true;
    }
}