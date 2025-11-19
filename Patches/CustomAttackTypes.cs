using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace Needleforge.Patches;

static class CustomAttackTypes
{
    [HarmonyPatch]
    static class TreatCustomToolTypesAsTheirDefiningTypes
    {
        // be careful about the implications of adding or removing methods from this list!
        static IEnumerable<MethodBase> TargetMethods() => new[]
            {
                AccessTools.PropertyGetter(typeof(ToolItem), nameof(ToolItem.DisplayAmountText)),
                AccessTools.Method(typeof(ToolItem), nameof(ToolItem.HasLimitedUses)),
                AccessTools.Method(typeof(ToolItem), nameof(ToolItem.Unlock)),
                AccessTools.Method(
                    typeof(ToolItemManager), nameof(ToolItemManager.SetEquippedTools)
                ),
                AccessTools.Method(
                    typeof(ToolItemManager), nameof(ToolItemManager.AutoEquip),
                    [typeof(ToolCrest), typeof(bool), typeof(bool)]
                ),
                AccessTools.Method(
                    typeof(ToolItemManager), nameof(ToolItemManager.ResetPreviousCrest)
                ),
                AccessTools.Method(
                    typeof(HeroController), nameof(HeroController.CanThrowTool),
                    [typeof(ToolItem), typeof(AttackToolBinding), typeof(bool)]
                ),
                AccessTools.Method(typeof(HeroController), nameof(HeroController.ThrowTool)),
                AccessTools.Method(
                    typeof(DamageEnemies), nameof(DamageEnemies.DoDamage),
                    [typeof(GameObject), typeof(bool)]
                ),
                AccessTools.Method(typeof(ActiveCorpse), nameof(ActiveCorpse.DoQueuedBurnEffects)),
                AccessTools.Method(typeof(ToolHudIcon), nameof(ToolHudIcon.GetAmounts)),
                AccessTools.Method(typeof(ToolHudIcon), nameof(ToolHudIcon.GetIsEmpty)),
                AccessTools.Method(typeof(ToolHudIcon), nameof(ToolHudIcon.OnSilkSpoolRefreshed)),
                AccessTools.Method(
                    typeof(InventoryItemToolTween), nameof(InventoryItemToolTween.DoPlace)
                ),
                AccessTools.Method(
                    typeof(InventoryItemToolTween), nameof(InventoryItemToolTween.DoReturn)
                )
            }
            // compiler-generated method names:
            // - lambdas: "<{parentMethodName}>b__{numbers}"
            // - local functions: "<{parentMethodName}>g__{localFunctionName}|{numbers}"
            .Concat(typeof(ToolItemManager)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(m =>
                    m.Name.StartsWith("<AutoEquip>g__RemoveNonSkills|") ||
                    m.Name.StartsWith("<ReportToolUnlocked>b__")
                )
            );

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Action<CodeMatcher> insertPatchToolTypeCall = matcher => matcher
                .Advance(1)
                .Insert([
                    new(OpCodes.Call, AccessTools.Method(
                        typeof(CustomAttackTypes), nameof(CustomAttackTypes.PatchToolType)
                    ))
                ]);

            return new CodeMatcher(instructions)
                .MatchStartForward([
                    new(OpCodes.Ldfld, AccessTools.Field(typeof(ToolItem), nameof(ToolItem.type)))
                ])
                .Repeat(insertPatchToolTypeCall)
                .Start()
                .MatchStartForward([
                    new(OpCodes.Callvirt, AccessTools.PropertyGetter(
                        typeof(ToolItem), nameof(ToolItem.Type)
                    ))
                ])
                .Repeat(insertPatchToolTypeCall)
                .InstructionEnumeration();
        }
    }

    [HarmonyPatch(
        typeof(ToolItemManager), nameof(ToolItemManager.ReportToolUnlocked),
        [typeof(ToolItemType), typeof(bool)]
    )]
    [HarmonyPrefix]
    static void PatchReportToolUnlocked(ref ToolItemType type)
    {
        type = CustomAttackTypes.PatchToolType(type);
    }

    [HarmonyPatch(typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.ShowCursedMsg))]
    [HarmonyPrefix]
    static void PatchShowCursedMsg(ref ToolItemType toolType)
    {
        toolType = CustomAttackTypes.PatchToolType(toolType);
    }

    [HarmonyPatch(
        typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.ShowToolEquipMsg)
    )]
    [HarmonyPrefix]
    static void PatchShowToolEquipMsg(ref ToolItemType type)
    {
        type = CustomAttackTypes.PatchToolType(type);
    }

    [HarmonyPatch(
        typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.SetDisplay),
        [typeof(InventoryItemSelectable)]
    )]
    [HarmonyPostfix]
    static void PatchInventoryItemToolManagerSetDisplay(
        InventoryItemToolManager __instance,
        InventoryItemSelectable selectable
    )
    {
        ToolItem? item = null;

        var tool = selectable as InventoryItemTool;
        if (tool)
        {
            item = tool.ItemData;
        }

        var slot = selectable as InventoryToolCrestSlot;
        if (slot && !slot.IsLocked && slot.EquippedItem)
        {
            item = slot.EquippedItem;
        }

        var promptText = __instance.equipPromptText;
        if (item || slot && promptText)
        {
            var type = CustomAttackTypes.PatchToolType((item ? item.Type : slot!.Type));
            if (item && item.IsEquipped)
            {
                promptText.text = type == ToolItemType.Skill
                    ? __instance.unequipSkillText
                    : __instance.unequipText;
            }
            else
            {
                promptText.text = type == ToolItemType.Skill
                    ? __instance.equipSkillText
                    : __instance.equipText;
            }
        }
    }

    static ToolItemType PatchToolType(ToolItemType input)
    {
        if ((int)input > 3)
        {
            return NeedleforgePlugin.newColors[(int)input - 4].DefiningType;
        }
        else
        {
            return input;
        }
    }
}
