using HarmonyLib;
using Needleforge.Utils;
using System.Collections.Generic;
using System.Reflection.Emit;
using static Needleforge.Utils.ILUtils;

namespace Needleforge.Components;

/*
Inheriting NailSlashTravel avoids duplicate code & improves compatibility w/ other mods,
but none of the methods are virtual/abstract, so, Harmony patches and type checking are
acting as a sort of forced 'override' keyword.
*/

/// <summary>
/// Causes an attack to move when triggered, instead of following Hornet. Works with
/// <i>any</i> descendant of <see cref="NailAttackBase"/>.
/// </summary>
public class NailAttackTravel : NailSlashTravel
{
    internal NailAttackBase attack;
    internal HeroDownAttack? heroDownAttack;

    internal void SetCollidersActive(bool value)
    {
        if (attack)
        {
            if (collider2D)
                collider2D.enabled = value;
            attack.clashTinkPoly.enabled = value;
        }
    }
}

[HarmonyPatch(typeof(NailSlashTravel))]
internal static class NailAttackTravel_Overrides
{
    #region Utilities

    /// <returns>True if .attack != null.</returns>
    static bool ResetComponents(NailAttackTravel x)
    {
        x.slash = null;
        x.hasSlash = false;
        x.heroDownAttack = x.GetComponent<HeroDownAttack>();
        if (x.TryGetComponent<NailAttackBase>(out x.attack))
            x.damager = x.attack.EnemyDamager;
        return (bool)x.attack;
    }
    
    /// <returns>True if this is a <see cref="NailAttackTravel"/> and .attack != null.</returns>
    static bool ResetComponents(NailSlashTravel x)
        => x is NailAttackTravel y && ResetComponents(y);

    static bool ShouldRunHandlers(NailSlashTravel x)
        => x is not NailAttackTravel || x.enabled;

    #endregion

    #region Overridden Unity Messages

    [HarmonyPatch("Awake")]
    [HarmonyPrefix]
    static void Awake(NailSlashTravel __instance)
    {
        if (__instance is NailAttackTravel x && ResetComponents(x))
        {
            x.attack.AttackStarting += x.OnSlashStarting;
            x.attack.EndedDamage += x.OnDamageEnded;
            x.attack.SetLongNeedleHandled();
        }
    }


    [HarmonyPatch("OnEnable")]
    [HarmonyPrefix]
    static void OnEnable(NailSlashTravel __instance) => ResetComponents(__instance);


    [HarmonyPatch("FixedUpdate")]
    [HarmonyPostfix]
    static void FixedUpdate(NailSlashTravel __instance)
    {
        if (__instance is not NailAttackTravel x || !x.hasCollider || !x.isSlashActive)
            return;
        if (x.wasColliderActive && !x.collider2D.enabled && x.attack)
            x.SetCollidersActive(true);
    }

    #endregion

    #region Overridden Utility Functions

    [HarmonyPatch("Reset")]
    [HarmonyPostfix]
    static void Reset(NailSlashTravel __instance) => ResetComponents(__instance);


    [HarmonyPatch("StopTravelImpact")]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> StopTravelImpact(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator
    ) {
        /* Inserts an else-if clause into the final if statement which handles the
         * NailAttackTravel.attack field. An IL patch is the only way to prevent the
         * method from deactivating and reactivating the GameObject. */
        Label
            elseIf_L = default,
            else_L = generator.DefineLabel();
        return new CodeMatcher(instructions, generator)
            .Start()
            .MatchStartForward([
                new(x => Ldfld(x, "slash")),
                new(OpCodes.Call),
                new(x => Brfalse(x, out elseIf_L)),
            ])
            .MatchStartForward([
                new(x => x.labels.Contains(elseIf_L))
            ])
            .StealLabel(else_L, out _)
            .Insert([
                new(OpCodes.Ldarg_0) { labels = [elseIf_L] },
                Transpilers.EmitDelegate(ElseIfClause),
                new(OpCodes.Brfalse, else_L),
                new(OpCodes.Ret),
            ])
            .InstructionEnumeration();

        static bool ElseIfClause(NailSlashTravel self)
        {
            if (self is NailAttackTravel x && x.attack)
            {
                // HeroDownAttack won't cause a bounce if the attack gets canceled early
                if (!x.heroDownAttack)
                    x.attack.EndAttack();
                return true; // goto return
            }
            return false; // goto else
        }
    }

    #endregion

    #region Overridden Event Handlers

    [HarmonyPatch("OnHeroFlipped")]
    [HarmonyPrefix]
    static bool OnHeroFlipped(NailSlashTravel __instance)
    {
        if (!ShouldRunHandlers(__instance))
            return false;

        if (__instance is NailAttackTravel x && x.travelRoutine != null && x.attack)
            x.SetCollidersActive(false);
        return true;
    }


    [HarmonyPatch("OnThunked")]
    [HarmonyPrefix]
    static bool OnThunked(NailSlashTravel __instance) => ShouldRunHandlers(__instance);


    [HarmonyPatch("OnSlashStarting")]
    [HarmonyPrefix]
    static bool OnSlashStarting(NailSlashTravel __instance) => ShouldRunHandlers(__instance);


    [HarmonyPatch("OnDamageEnded")]
    [HarmonyPrefix]
    static bool OnDamageEnded(NailSlashTravel __instance, bool didHit)
    {
        bool shouldRun = ShouldRunHandlers(__instance);

        // HeroDownAttack won't cause a bounce if hornet can't do custom recoil
        if (shouldRun && didHit && __instance is NailAttackTravel x && x.heroDownAttack)
            x.hc.AllowRecoil();

        return shouldRun;
    }


    [HarmonyPatch("OnDamaged")]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> OnDamaged(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator
    ) {
        /* For NailAttackTravel components only, rewrites the opening if statement to use
         * the .attack field instead of the .slash field, and to include the standard
         * check for if event handlers should be running at all. */
        Label
            return_L = generator.DefineLabel(),
            endIf_L = default;
        return new CodeMatcher(instructions, generator)
            .Start()
            .MatchStartForward([
                new(OpCodes.Ldarg_0),
                new(x => Ldfld(x, "hasSlash")),
                new(x => Brfalse(x, out endIf_L)),
            ])
            .CreateLabel(out Label originalIf_L)
            .Insert([
                new(OpCodes.Ldarg_0),
                Transpilers.EmitDelegate(HeroDownAttackInterop),

                new(OpCodes.Ldarg_0),
                Transpilers.EmitDelegate(ReplacementIfClause),
                new(OpCodes.Switch, new Label[]{ return_L, originalIf_L, endIf_L }),
                new(OpCodes.Ret) { labels = [return_L] },
            ])
            .InstructionEnumeration();

        static GoTo ReplacementIfClause(NailSlashTravel self)
        {
            if (self is not NailAttackTravel x)
                return GoTo.OriginalIf;

            if (
                !ShouldRunHandlers(self)
                || (x.attack && (!x.attack.IsDamagerActive || !x.attack.CanDamageEnemies))
            )
                return GoTo.Return;

            return GoTo.EndIf;
        }
        static void HeroDownAttackInterop(NailSlashTravel self)
        {
            // HeroDownAttack won't cause a bounce if hornet can't do custom recoil
            if (self is NailAttackTravel x && x.heroDownAttack)
                x.hc.AllowRecoil();
        }
    }

    // Readable return values for the delegate of the OnDamaged IL patch.
    enum GoTo { Return, OriginalIf, EndIf }

    #endregion
}
