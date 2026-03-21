using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace Needleforge.Utils;

/// <summary>
/// Instruction matching predicates and extensions for making IL patches more readable.
/// </summary>
internal static class ILUtils {

    #region St/Ld loc

    /// <summary>
    /// True if the opcode is any variant of <see cref="OpCodes.Ldloc"/>.
    /// </summary>
    internal static bool Ldloc(CodeInstruction x)
        => Ldloc(x, out int _);

    /// <summary>
    /// True if the opcode is any variant of <see cref="OpCodes.Ldloc"/> and the index matches.
    /// </summary>
    internal static bool Ldloc(CodeInstruction x, int index)
        => Ldloc(x, out int i) && index == i;

    /// <summary>
    /// True if the opcode is any variant of <see cref="OpCodes.Ldloc"/>.
    /// If true, <paramref name="index"/> is set to the index loaded by the instruction.
    /// </summary>
    internal static bool Ldloc(CodeInstruction x, out int index) {
        index = -1;
        if (x.opcode == OpCodes.Ldloc_0) index = 0;
        else if (x.opcode == OpCodes.Ldloc_1) index = 1;
        else if (x.opcode == OpCodes.Ldloc_2) index = 2;
        else if (x.opcode == OpCodes.Ldloc_3) index = 3;
        else if ((x.opcode == OpCodes.Ldloc || x.opcode == OpCodes.Ldloc_S) && x.operand is LocalBuilder lb)
            index = lb.LocalIndex;

        return index >= 0;
    }

    /// <summary>
    /// True if the opcode is any variant of <see cref="OpCodes.Stloc"/>.
    /// </summary>
    internal static bool Stloc(CodeInstruction x)
        => Stloc(x, out int _);

    /// <summary>
    /// True if the opcode is any variant of <see cref="OpCodes.Stloc"/> and the index matches.
    /// </summary>
    internal static bool Stloc(CodeInstruction x, int index)
        => Stloc(x, out int i) && index == i;

    /// <summary>
    /// True if the opcode is any variant of <see cref="OpCodes.Stloc"/>.
    /// If true, <paramref name="index"/> is set to the index set by the instruction.
    /// </summary>
    internal static bool Stloc(CodeInstruction x, out int index) {
        index = -1;
        if (x.opcode == OpCodes.Stloc_0) index = 0;
        else if (x.opcode == OpCodes.Stloc_1) index = 1;
        else if (x.opcode == OpCodes.Stloc_2) index = 2;
        else if (x.opcode == OpCodes.Stloc_3) index = 3;
        else if ((x.opcode == OpCodes.Stloc || x.opcode == OpCodes.Stloc_S) && x.operand is LocalBuilder lb)
            index = lb.LocalIndex;

        return index >= 0;
    }

    #endregion

    #region St/Ld fld

    /// <summary>
    /// True if the opcode is <see cref="OpCodes.Ldfld"/>.
    /// </summary>
    internal static bool Ldfld(CodeInstruction x)
        => Ldfld(x, out string _);

    /// <summary>
    /// True if the opcode is <see cref="OpCodes.Ldfld"/> and the field name matches.
    /// </summary>
    internal static bool Ldfld(CodeInstruction x, string name)
        => Ldfld(x, out string n) && name == n;

    /// <summary>
    /// True if the opcode is <see cref="OpCodes.Ldfld"/>.
    /// If true, <paramref name="name"/> is set to the field name loaded by the instruction.
    /// </summary>
    internal static bool Ldfld(CodeInstruction x, out string name) {
        name = "";
        if (x.opcode == OpCodes.Ldfld && x.operand is FieldInfo f)
            name = f.Name;
        return !string.IsNullOrWhiteSpace(name);
    }

    /// <summary>
    /// True if the opcode is <see cref="OpCodes.Stfld"/>.
    /// </summary>
    internal static bool Stfld(CodeInstruction x)
        => Stfld(x, out string _);

    /// <summary>
    /// True if the opcode is <see cref="OpCodes.Stfld"/> and the field name matches.
    /// If true, <paramref name="field"/> is set to the <see cref="FieldInfo"/> of the instruction.
    /// </summary>
    internal static bool Stfld(CodeInstruction x, string name, out FieldInfo field) {
        field = x.operand is FieldInfo f ? f : null!;
        return Stfld(x, name);
    }

    /// <summary>
    /// True if the opcode is <see cref="OpCodes.Stfld"/> and the field name matches.
    /// </summary>
    internal static bool Stfld(CodeInstruction x, string name)
        => Stfld(x, out string n) && name == n;

    /// <summary>
    /// True if the opcode is <see cref="OpCodes.Stfld"/>.
    /// If true, <paramref name="name"/> is set to the field name set by the instruction.
    /// </summary>
    internal static bool Stfld(CodeInstruction x, out string name) {
        name = "";
        if (x.opcode == OpCodes.Stfld && x.operand is FieldInfo f)
            name = f.Name;
        return !string.IsNullOrWhiteSpace(name);
    }

    #endregion

    #region Branching

    /// <summary>
    /// True if the opcode is any variant of <see cref="OpCodes.Br"/>.
    /// </summary>
    internal static bool Br(CodeInstruction x)
        => x.opcode == OpCodes.Br || x.opcode == OpCodes.Br_S;

    /// <summary>
    /// True if the opcode is any variant of <see cref="OpCodes.Br"/>. If true,
    /// <paramref name="label"/> is set to the <see cref="Label"/> branched to.
    /// </summary>
    internal static bool Br(CodeInstruction x, out Label label) {
        label = Br(x) ? (Label)x.operand : default;
        return Br(x);
    }

    /// <summary>
    /// True if the opcode is any variant of <see cref="OpCodes.Brfalse"/>.
    /// </summary>
    internal static bool Brfalse(CodeInstruction x)
        => x.opcode == OpCodes.Brfalse || x.opcode == OpCodes.Brfalse_S;

    /// <summary>
    /// True if the opcode is any variant of <see cref="OpCodes.Brtrue"/>.
    /// </summary>
    internal static bool Brtrue(CodeInstruction x)
        => x.opcode == OpCodes.Brtrue || x.opcode == OpCodes.Brtrue_S;

    #endregion

    #region Calling

    /// <summary>
    /// True if the opcode is <see cref="OpCodes.Call"/> and the method name matches.
    /// </summary>
    internal static bool Call(CodeInstruction x, string name)
        => x.opcode == OpCodes.Call
            && x.operand is MethodInfo m && m.Name == name;

    /// <summary>
    /// True if the opcode is <see cref="OpCodes.Call"/> and the method name and type matches.
    /// </summary>
    internal static bool Call(CodeInstruction x, MethodInfo method)
        => x.opcode == OpCodes.Call
            && x.operand is MethodInfo m && m == method;

    /// <summary>
    /// True if the opcode is <see cref="OpCodes.Callvirt"/> and the method name matches.
    /// </summary>
    internal static bool Callvirt(CodeInstruction x, string name)
        => x.opcode == OpCodes.Callvirt
            && x.operand is MethodInfo m && m.Name == name;

    /// <summary>
    /// True if the opcode is <see cref="OpCodes.Callvirt"/> and the method name and type matches.
    /// </summary>
    internal static bool Callvirt(CodeInstruction x, MethodInfo method)
        => x.opcode == OpCodes.Callvirt
            && x.operand is MethodInfo m && m == method;

    #endregion

    #region Labels

    /// <summary>
    /// Replaces the first label of the instruction at the current position with the
    /// defined <paramref name="newLabel"/>, and returns the original label
    /// in <paramref name="oldLabel"/>.
    /// </summary>
    /// <returns>The same code matcher</returns>
    internal static CodeMatcher StealLabel(this CodeMatcher cm, Label newLabel, out Label oldLabel)
        => cm.StealLabel(0, newLabel, out oldLabel);

    /// <summary>
    /// Replaces the label at index <paramref name="i"/> of the instruction at the
    /// current position with the defined <paramref name="newLabel"/>, and returns the
    /// original label in <paramref name="oldLabel"/>.
    /// </summary>
    /// <returns>The same code matcher</returns>
    internal static CodeMatcher StealLabel(this CodeMatcher cm, int i, Label newLabel, out Label oldLabel) {
        oldLabel = cm.Instruction.labels[i];
        cm.Instruction.labels[i] = newLabel;
        return cm;
    }

    #endregion

}
