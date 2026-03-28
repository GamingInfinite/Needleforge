using HarmonyLib;

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
}
