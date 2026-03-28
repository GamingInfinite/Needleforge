using HarmonyLib;
using Needleforge.Components;
using UnityEngine;

namespace Needleforge.Patches;

[HarmonyPatch(typeof(HeroDownAttack))]
internal static class PogoLogicTweaks
{
    // Fixes a vanilla bug where NailSlash down attacks trigger two auto-bounces
    [HarmonyPatch("OnEndedDamage")]
    [HarmonyPrefix]
    static void PreventDoubleBounces(HeroDownAttack __instance, bool didHit)
    {
        if (didHit && __instance.bounceQueued && __instance.attack)
            __instance.hc.AffectedByGravity(true);
        __instance.bounceQueued = false;
    }

    // NailAttackTravel prevents custom recoil, which prevents auto-bouncing.
    // For safety in case component (event handler) order is changed later.
    [HarmonyPatch("ContinueBounceTrigger")]
    [HarmonyPrefix]
    static void AllowTravellingBounces(HeroDownAttack __instance, GameObject otherObj)
    {
        if (
            !HeroDownAttack.IsNonBounce(otherObj)
            && __instance.TryGetComponent<NailAttackTravel>(out var x)
            && x.enabled
        ) {
            __instance.hc.AllowRecoil();
        }
    }
}
