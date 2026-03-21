using GlobalSettings;
using HarmonyLib;
using Needleforge.Data;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using static Needleforge.NeedleforgePlugin;
using static Needleforge.Utils.ILUtils;
using BasicFrameAnims = BindOrbHudFrame.BasicFrameAnims;
using CoroutineFunction = BindOrbHudFrame.CoroutineFunction;

namespace Needleforge.Patches.HUD;

/// <summary>
/// Patches which enable custom crests to use custom HUD frames in-game.
/// </summary>
[HarmonyPatch(typeof(BindOrbHudFrame), nameof(BindOrbHudFrame.DoChangeFrame))]
internal static class ReplaceInGameHud
{
    const BindingFlags PUBLICSTATIC = BindingFlags.Public | BindingFlags.Static;

    /// <summary>
    /// Injects a branch into the crest selection process of <see cref="BindOrbHudFrame.DoChangeFrame"/>
    /// that allows setting custom HUD animations and coroutines for custom crests.
    /// </summary>
    /// <remarks>
    /// Thanks <see href="https://github.com/hamunii">Hamunii</see> for the help making
    /// this patch cleaner and more maintainable.
    /// </remarks>
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> ChangeHud(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator
    ) {
        MethodInfo
            get_HunterCrest2 = typeof(Gameplay)
                .GetProperty(nameof(Gameplay.HunterCrest2), PUBLICSTATIC).GetGetMethod(),
            get_ToolBase_IsEquipped = typeof(ToolBase)
                .GetProperty(nameof(ToolBase.IsEquipped)).GetGetMethod();
        int
            localsObject_idx = -1,
            hunter2_idx = -1;
        FieldInfo
            newFrameAnims_f = null!,
            customAnimRoutine_f = null!;
        Label
            elseIfCompleted_label = default,
            hunter2_newLabel = generator.DefineLabel(),
            returnFalse_label = generator.DefineLabel();

        return new CodeMatcher(instructions, generator)

        // find index of the runtime class in charge of
        // local variables newFrameAnims and customAnimRoutine
        .Start()
        .MatchStartForward([
            new(x => Ldloc(x, out localsObject_idx)),
            new(x => Ldfld(x, "newFrameAnims")),
        ])

        // find field references for newFrameAnims and customAnimRoutine
        .Start()
        .MatchStartForward([
            new(x => Stfld(x, "newFrameAnims", out newFrameAnims_f)),
        ])
        .Start()
        .MatchStartForward([
            new(x => Stfld(x, "customAnimRoutine", out customAnimRoutine_f)),
        ])

        // find index of hunter_v2
        .Start()
        .MatchStartForward([
            new(x => Call(x, get_HunterCrest2)),
            new(x => Stloc(x, out hunter2_idx)),
        ])

        // find injection site; hunter_v2 else-if block.
        // record label of the end of the if-statement, steal label of this else-if block
        .MatchStartForward([
            new(x => Br(x, out elseIfCompleted_label)),
            new(x => Ldloc(x, hunter2_idx)),
            new(x => Callvirt(x, get_ToolBase_IsEquipped)),
        ])
        .Advance(1)
        .StealLabel(hunter2_newLabel, out Label hunter2_oldLabel)

        // inject a new "else-if" block
        .Insert([
            // push args onto stack
            new(OpCodes.Ldarg_0) { labels = [hunter2_oldLabel] },

            new(OpCodes.Ldloc, localsObject_idx),
            new(OpCodes.Ldflda, newFrameAnims_f),

            new(OpCodes.Ldloc, localsObject_idx),
            new(OpCodes.Ldflda, customAnimRoutine_f),

            // handle the logic and push an int describing where to branch
            Transpilers.EmitDelegate(SetCustomHudVars),

            // branch based on result
            new(OpCodes.Switch, new Label[]{
                returnFalse_label, hunter2_newLabel, elseIfCompleted_label
            }),

            // the 'return false' branch
            new(OpCodes.Ldc_I4_0) { labels = [returnFalse_label] },
            new(OpCodes.Ret),
        ])

        .InstructionEnumeration();
    }

    /// <summary>
    /// Delegate for <see cref="ChangeHud"/> which handles the logic for either setting
    /// the HUD to the animations and coroutine of custom crests, or else branching
    /// appropriately if there's nothing to set.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="basicFrameAnims">
    ///     Reference to the local var responsible for setting the names of animations
    ///     used for basic HUD visuals like appearing and disappearing.
    /// </param>
    /// <param name="coroutineFunction">
    ///     Reference to local var responsible for setting the coroutine used for extra
    ///     visual effects in the HUD; ex. Architect's craft bind animation.
    /// </param>
    /// <returns>
    ///     A number indicating how the IL patch in <see cref="ChangeHud"/> should branch.
    /// </returns>
    private static ReturnBehaviour SetCustomHudVars(
        BindOrbHudFrame self,
        ref BasicFrameAnims basicFrameAnims,
        ref CoroutineFunction? coroutineFunction
    ) {
        foreach (var crest in newCrestData) {
            if (!crest.IsEquipped)
                continue;

            if (crest.ToolCrest == self.currentFrameCrest)
                return ReturnBehaviour.ReturnFalse;

            IEnumerator HudCoro() => crest.HudFrame.Coroutine(self);

            self.currentFrameCrest = crest.ToolCrest;
            basicFrameAnims =
                crest.HudFrame.HasRegularCustomBasicAnims
                    ? crest.HudFrame.CustomBasicFrameAnims()
                    : PresetBasicAnims(self, crest.HudFrame.Preset);
            if (crest.HudFrame.Coroutine != null)
                coroutineFunction = HudCoro;

            return ReturnBehaviour.ElseIfCompleted;
        }
        return ReturnBehaviour.NextElseIf;
    }

    /// <summary>
    /// Readable names for return values of <see cref="SetCustomHudVars"/> which describe
    /// the behaviour they trigger in the IL patch in <see cref="ChangeHud"/>.
    /// </summary>
    private enum ReturnBehaviour
    {
        ReturnFalse = 0,
        NextElseIf = 1,
        ElseIfCompleted = 2,
    }

    private static BasicFrameAnims PresetBasicAnims(BindOrbHudFrame self, VanillaCrest crest)
        => crest switch {
            VanillaCrest.HUNTER_V2 => self.hunterV2FrameAnims,
            VanillaCrest.HUNTER_V3 => self.hunterV3FrameAnims,
            VanillaCrest.BEAST => self.warriorFrameAnims,
            VanillaCrest.REAPER => self.reaperFrameAnims,
            VanillaCrest.WANDERER => self.wandererFrameAnims,
            VanillaCrest.WITCH => self.witchFrameAnims,
            VanillaCrest.ARCHITECT => self.toolmasterFrameAnims,
            VanillaCrest.SHAMAN => self.spellFrameAnims,
            VanillaCrest.CURSED => self.cursedV1FrameAnims,
            VanillaCrest.CLOAKLESS => self.cloaklessFrameAnims,
            _ => self.defaultFrameAnims
        };

}
