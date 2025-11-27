using HarmonyLib;
using Needleforge.Components;
using UnityEngine;

namespace Needleforge.Patches.HeroControl;

[HarmonyPatch]
internal class CustomDownspikeBehaviour
{
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.Downspike))]
    [HarmonyPostfix]
    private static void SetVelocity(HeroController __instance)
    {
        if (
            (__instance.downSpikeTimer - Time.deltaTime) <= __instance.Config.DownSpikeTime
            && __instance.Config.DownspikeThrusts
            && __instance.cState.downSpiking
            && __instance.currentDownspike is NeedleforgeDownspike nds
        ) {
            __instance.rb2d.linearVelocity =
                nds.Velocity + (nds.acceleration * __instance.downSpikeTimer);
        }
    }

    [HarmonyPatch(typeof(HeroController), nameof(HeroController.DownspikeBounce))]
    [HarmonyPrefix]
    private static void SetBounceConfig(HeroController __instance, ref HeroSlashBounceConfig bounceConfig)
    {
        if (__instance.currentDownspike is NeedleforgeDownspike nds) {
            bounceConfig = nds.bounceConfig;
        }
    }
}
