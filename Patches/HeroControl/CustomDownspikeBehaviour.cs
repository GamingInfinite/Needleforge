using HarmonyLib;
using Needleforge.Components;
using Needleforge.Data;
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
            && __instance.Config is HeroConfigNeedleforge cfg
        ) {
            Vector2 heroFacing = __instance.cState.facingRight ? new(-1, 1) : Vector2.one;

            __instance.rb2d.linearVelocity =
                (cfg.DownspikeVelocity + (cfg.DownspikeAcceleration * __instance.downSpikeTimer)) * heroFacing;
        }
    }

    [HarmonyPatch(typeof(HeroController), nameof(HeroController.DownspikeBounce))]
    [HarmonyPrefix]
    private static void SetBounceConfig(HeroController __instance, ref HeroSlashBounceConfig bounceConfig)
    {
        if (__instance.currentDownspike is DownspikeWithBounceConfig nds)
        {
            bounceConfig = nds.bounceConfig;
        }
    }
}
