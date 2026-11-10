using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Needleforge.Patches;

[HarmonyPatch]
internal static class DetectBarnacle
{
    private static readonly HashSet<DamageEnemies> customDamagers = [];

    internal static void Register(DamageEnemies? damager)
    {
        if (!damager)
            return;
        customDamagers.Add(damager!);

        int attackLayer = LayerMask.NameToLayer("Attack");
        if (attackLayer >= 0)
            damager!.gameObject.layer = attackLayer;

        if (!damager!.GetComponent<Rigidbody2D>())
        {
            var body = damager.gameObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.simulated = true;
        }
    }

    [HarmonyPatch(typeof(DamageEnemies), nameof(DamageEnemies.GetDamageDealt))]
    [HarmonyPostfix]
    private static void ReportNeedleDamage(DamageEnemies __instance, ref int __result)
    {
        if (__result <= 0 && customDamagers.Contains(__instance))
            __result = Mathf.Max(1, PlayerData.instance.nailDamage);
    }
}