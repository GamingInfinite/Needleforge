using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace Needleforge.Utils;

/// <summary>
/// Instruction matching predicates and other small utilities to help make IL patches
/// more readable.
/// </summary>
internal static class ILUtils {

    #region St/Ld loc

    /// <summary>
    /// Returns true if the given instruction's opcode is any variation of
    /// <see cref="OpCodes.Ldloc"/>.
    /// </summary>
    internal static bool LdlocRelaxed(CodeInstruction ci) {
        return ci.opcode == OpCodes.Ldloc
            || ci.opcode == OpCodes.Ldloc_0
            || ci.opcode == OpCodes.Ldloc_1
            || ci.opcode == OpCodes.Ldloc_2
            || ci.opcode == OpCodes.Ldloc_3
            || ci.opcode == OpCodes.Ldloc_S;
    }

    /// <summary>
    /// Returns true if the given instruction's opcode is any variation of
    /// <see cref="OpCodes.Ldloc"/> and the index of the local variable being loaded
    /// is the given index.
    /// </summary>
    internal static bool LdlocWithIndex(CodeInstruction ci, int index) {
        return (index == 0 && ci.opcode == OpCodes.Ldloc_0)
            || (index == 1 && ci.opcode == OpCodes.Ldloc_1)
            || (index == 2 && ci.opcode == OpCodes.Ldloc_2)
            || (index == 3 && ci.opcode == OpCodes.Ldloc_3)
            || (
                (ci.opcode == OpCodes.Ldloc || ci.opcode == OpCodes.Ldloc_S)
                && ci.operand is int v && v == index
            );
    }

    /// <summary>
    /// Returns true if the given instruction's opcode is any variation of
    /// <see cref="OpCodes.Stloc"/>.
    /// </summary>
    internal static bool StlocRelaxed(CodeInstruction ci) {
        return ci.opcode == OpCodes.Stloc
            || ci.opcode == OpCodes.Stloc_0
            || ci.opcode == OpCodes.Stloc_1
            || ci.opcode == OpCodes.Stloc_2
            || ci.opcode == OpCodes.Stloc_3
            || ci.opcode == OpCodes.Stloc_S;
    }

    /// <summary>
    /// Returns the index of the local variable being set by any variation of the
    /// <see cref="OpCodes.Stloc"/> instruction.
    /// If the instruction isn't stloc, returns -1.
    /// </summary>
    internal static int GetStlocIndex(CodeInstruction ci) {
        if (!StlocRelaxed(ci)) return -1;
        if (ci.opcode == OpCodes.Stloc_0) return 0;
        if (ci.opcode == OpCodes.Stloc_1) return 1;
        if (ci.opcode == OpCodes.Stloc_2) return 2;
        if (ci.opcode == OpCodes.Stloc_3) return 3;
        if (ci.operand is int v) return v;
        return -1;
    }

    #endregion

    #region St/Ld fld

    /// <summary>
    /// Returns true if the given instruction's opcode is <see cref="OpCodes.Ldfld"/>
    /// and the name of the field being loaded equals the given name.
    /// </summary>
    internal static bool LdfldWithName(CodeInstruction ci, string name) {
        if (ci.opcode != OpCodes.Ldfld)
            return false;
        return ci.operand is FieldInfo
            && (ci.operand as FieldInfo)?.Name == name;
    }

    /// <summary>
    /// Returns true if the given instruction's opcode is <see cref="OpCodes.Stfld"/>
    /// and the name of the field being set equals the given name.
    /// </summary>
    internal static bool StfldWithName(CodeInstruction ci, string name) {
        return ci.opcode == OpCodes.Stfld
            && ci.operand is FieldInfo f
            && f.Name == name;
    }

    #endregion

    #region Branching

    /// <summary>
    /// Returns true if the given instruction's opcode is any variation of
    /// <see cref="OpCodes.Br"/>.
    /// </summary>
    internal static bool BrRelaxed(CodeInstruction ci) {
        return ci.opcode == OpCodes.Br
            || ci.opcode == OpCodes.Br_S;
    }

    /// <summary>
    /// Returns true if the given instruction's opcode is any variation of
    /// <see cref="OpCodes.Brfalse"/>.
    /// </summary>
    internal static bool BrfalseRelaxed(CodeInstruction ci) {
        return ci.opcode == OpCodes.Brfalse
            || ci.opcode == OpCodes.Brfalse_S;
    }

    #endregion

    #region Calling

    /// <summary>
    /// Returns true if the given instruction's opcode is <see cref="OpCodes.Call"/>
    /// and the name of the method being called equals the given name.
    /// </summary>
    internal static bool CallWithMethodName(CodeInstruction ci, string name) {
        if (ci.opcode != OpCodes.Call)
            return false;
        return ci.operand is MethodInfo
            && (ci.operand as MethodInfo)?.Name == name;
    }

    /// <summary>
    /// Returns true if the given instruction's opcode is <see cref="OpCodes.Callvirt"/>
    /// and the name of the method being called equals the given name.
    /// </summary>
    internal static bool CallvirtWithMethodName(CodeInstruction ci, string name) {
        if (ci.opcode != OpCodes.Callvirt)
            return false;
        return ci.operand is MethodInfo
            && (ci.operand as MethodInfo)?.Name == name;
    }

    #endregion

}
