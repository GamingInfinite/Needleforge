using GlobalSettings;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static Needleforge.NeedleforgePlugin;
using static Needleforge.Utils.ILUtils;
using BasicFrameAnims = BindOrbHudFrame.BasicFrameAnims;
using CoroutineFunction = BindOrbHudFrame.CoroutineFunction;
using Needleforge.Data;
using System.Collections;
using System;

namespace Needleforge.Patches;

/// <summary>
/// Patches necessary for custom crests to be able to have their choice of
/// any base game HUD frame.
/// </summary>
[HarmonyPatch(typeof(BindOrbHudFrame), nameof(BindOrbHudFrame.DoChangeFrame))]
internal class CrestHUD {

	/// <summary>
	/// IL patch which injects an extra branch into the crest selection process of
	/// <see cref="BindOrbHudFrame.DoChangeFrame"/> to enable setting custom HUD
	/// animations for custom crests.
	/// </summary>
	/// <remarks>
	/// Many thanks to <see href="https://github.com/hamunii">Hamunii</see> for their
	/// help making this patch cleaner and more maintainable.
	/// </remarks>
	
	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> ChangeHud(
		IEnumerable<CodeInstruction> instructions,
		ILGenerator generator
	) {
		var cm = new CodeMatcher(instructions, generator);
		cm.Start();

		#region Locating local variable field references

		// Find ldloc index of the runtime class in charge of local vars newFrameAnims
		// and customAnimRoutine
		cm.MatchForward(useEnd: false, [
			new (LdlocRelaxed),
			new (ci => LdfldWithName(ci, "newFrameAnims"))
		]);
		if (cm.IsInvalid) {
			cm.ReportFailure(null, logger.LogError);
			return instructions;
		}
		var ldLocalVarsObj = cm.Instruction;

		// Find field references for newFrameAnims and customAnimRoutine
		var newFrameAnims =
			instructions.First(ci => StfldWithName(ci, "newFrameAnims"))
			.operand as FieldInfo;

		var customAnimRoutine =
			instructions.First(ci => StfldWithName(ci, "customAnimRoutine"))
			.operand as FieldInfo;

		#endregion

		#region Locating the injection site

		// Find the ldloc index of hunter crest 2
		cm.Start();
		cm.MatchForward(useEnd: true, [
			new (OpCodes.Call),
			new (StlocRelaxed),
			new (ci => CallWithMethodName(ci, $"get_{nameof(Gameplay.HunterCrest2)}")),
			new (StlocRelaxed)
		]);
		if (cm.IsInvalid) {
			cm.ReportFailure(null, logger.LogError);
			return instructions;
		}
		int locHunterCrest2 = GetStlocIndex(cm.Instruction);

		// Find the first instruction of the HunterCrest2 else-if block
		cm.MatchForward(useEnd: false, [
			new (BrRelaxed), // label for elseIfCompleted
			new (ci => LdlocWithIndex(ci, locHunterCrest2)), // set index here
			new (ci => CallvirtWithMethodName(ci, $"get_{nameof(ToolBase.IsEquipped)}")),
			new (BrfalseRelaxed)
		]);
		Label elseIfCompleted = (Label)cm.Operand;
		cm.Advance(1); // now at the Ldloc
		if (cm.IsInvalid) {
			cm.ReportFailure(null, logger.LogError);
			return instructions;
		}

		#endregion

		#region Performing the injection

		// Record and steal the label of the start of the hunter else-if
		Label hunter2startOld = cm.Instruction.labels[0],
			hunter2startNew = generator.DefineLabel();
		cm.Instruction.labels = [hunter2startNew];

		// Store the first instruction of the ret false path, with a label
		var returnFalse = new CodeInstruction(OpCodes.Ldc_I4_0) { labels = [generator.DefineLabel()] };

		// Inject a new "else-if" block....
		cm.Insert([
			new (OpCodes.Ldarg_0) { labels = [hunter2startOld] }, // arg 0: this
			
			new (ldLocalVarsObj), // arg 1: ref BasicFrameAnims
			new (OpCodes.Ldflda, newFrameAnims),

			new (ldLocalVarsObj), // arg 2: ref CoroutineFunction
			new (OpCodes.Ldflda, customAnimRoutine),

			// Consumes args & returns an int [0, 1, 2] which maps to instruction to jump to
			Transpilers.EmitDelegate(SetCustomHudVars),

			// Jump to an instruction based on the return type
			new (OpCodes.Switch, new Label[]{ returnFalse.labels[0], hunter2startNew, elseIfCompleted }),

			// The return false path
			returnFalse,
			new (OpCodes.Ret),
		]);

		#endregion

		// Return the results!
		return cm.Instructions();
	}

	/// <summary>
	/// Delegate for <see cref="ChangeHud"/> which handles the logic for either setting
	/// the HUD to the animations and coroutine of custom crests, or else branching
	/// appropriately if there's nothing to set.
	/// </summary>
	/// <param name="self"></param>
	/// <param name="basicFrameAnims">
	///		Reference to the local variable in <see cref="BindOrbHudFrame.DoChangeFrame"/>
	///		which is responsible for setting the names of the animations used for basic
	///		HUD visuals like appearing and disappearing.
	/// </param>
	/// <param name="coroutineFunction">
	///		Reference to the local variable in <see cref="BindOrbHudFrame.DoChangeFrame"/>
	///		which is responsible for setting the coroutine used for extra visual effects
	///		in the HUD; ex. Architect's craft bind, Wanderer's expanded harp look, etc.
	/// </param>
	/// <returns>A number indicating action the rest of the IL patch in
	/// <see cref="ChangeHud"/> should take.</returns>
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

			IEnumerator HudCoro() => crest.HudFrameCoroutine(self);

			self.currentFrameCrest = crest.ToolCrest;
			basicFrameAnims = VanillaBasicFrameAnims(self, crest.HudFrame);
			if (crest.HudFrameCoroutine != null)
				coroutineFunction = HudCoro;

			return ReturnBehaviour.ElseIfCompleted;
		}
		return ReturnBehaviour.NextElseIf;
	}

	/// <summary>
	/// Readable names for return values of <see cref="SetCustomHudVars"/> which describe
	/// the behaviour they trigger in the IL patch in <see cref="ChangeHud"/>.
	/// </summary>
	private enum ReturnBehaviour {
		ReturnFalse = 0,
		NextElseIf = 1,
		ElseIfCompleted = 2,
	}

	private static BasicFrameAnims VanillaBasicFrameAnims(BindOrbHudFrame self, VanillaCrest crest) {
		return crest switch {
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

}
