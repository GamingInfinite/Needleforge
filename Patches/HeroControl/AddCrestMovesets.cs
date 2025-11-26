using HarmonyLib;
using Needleforge.Makers;
using UnityEngine;

namespace Needleforge.Patches.HeroControl;

[HarmonyPatch(typeof(HeroController), nameof(HeroController.Awake))]
internal class AddCrestMovesets {
    private static void Postfix()
    {
        ModHelper.Log("Initializing Crest Movesets...");
        foreach (var crest in NeedleforgePlugin.newCrestData) {
            ModHelper.Log($"Init {crest.name} Moveset");
            MovesetMaker.InitializeMoveset(crest.Moveset);
        }
    }
}

#if DEBUG && false

[HarmonyPatch]
internal class DebugMoveset {

    [HarmonyPatch(typeof(NailSlash), nameof(NailSlash.Awake))]
    [HarmonyPrefix]
    private static void awaking(NailSlash __instance) {
        if (__instance.transform.parent.name != "NeoCrest")
            return;
        Debug.Log($" ++ {__instance.name} ns awake post");
    }

    [HarmonyPatch(typeof(NailSlash), nameof(NailSlash.StartSlash))]
    [HarmonyPrefix]
    private static void startslash(NailSlash __instance) {
        if (__instance.transform.parent.name != "NeoCrest")
            return;
        Debug.Log(new string('-', 40));
        Debug.Log($" -- {__instance.name} ns startslash");
    }

    [HarmonyPatch(typeof(NailAttackBase), nameof(NailAttackBase.OnSlashStarting))]
    [HarmonyPrefix]
    private static void onslashstart(NailAttackBase __instance) {
        if (__instance.transform.parent.name != "NeoCrest")
            return;
        Debug.Log($" -- {__instance.name} ns onslashstarting");
    }

    [HarmonyPatch(typeof(NailSlash), nameof(NailSlash.PlaySlash))]
    [HarmonyPrefix]
    private static void playslash(NailSlash __instance) {
        if (__instance.transform.parent.name != "NeoCrest")
            return;
        Debug.Log($" -- {__instance.name} ns playslash");
    }

    [HarmonyPatch(typeof(NailAttackBase), nameof(NailAttackBase.OnPlaySlash))]
    [HarmonyPrefix]
    private static void onplayslash(NailAttackBase __instance) {
        if (__instance.transform.parent.name != "NeoCrest")
            return;
        Debug.Log($" -- {__instance.name} ns onplayslash");
    }

    [HarmonyPatch(typeof(NailSlash), nameof(NailSlash.OnAnimationEventTriggered))]
    [HarmonyPrefix]
    private static void onanimevttrig(NailSlash __instance) {
        if (__instance.transform.parent.name != "NeoCrest")
            return;
        Debug.Log($" -- {__instance.name} ns onAnimEvtTriggered");
    }

    [HarmonyPatch(typeof(NailSlash), nameof(NailSlash.SetCollidersActive))]
    [HarmonyPrefix]
    private static void setcolliders(NailSlash __instance, ref bool value) {
        if (__instance.transform.parent.name != "NeoCrest")
            return;
        Debug.Log($" -- {__instance.name} ns setColliders to {value}");
    }

    [HarmonyPatch(typeof(NailSlash), nameof(NailSlash.OnAnimationCompleted))]
    [HarmonyPrefix]
    private static void animcompleted(NailSlash __instance) {
        if (__instance.transform.parent.name != "NeoCrest")
            return;
        Debug.Log($" -- {__instance.name} ns animCompleted");
    }

    [HarmonyPatch(typeof(NailSlash), nameof(NailSlash.CancelAttack), [typeof(bool)])]
    [HarmonyPrefix]
    private static void canceled(NailSlash __instance) {
        if (__instance.transform.parent.name != "NeoCrest")
            return;
        Debug.Log($" -- {__instance.name} ns cancelattack");
    }

    [HarmonyPatch(typeof(Downspike), nameof(Downspike.QueueBounce))]
    [HarmonyPrefix]
    private static void downspikequeuebounce(Downspike __instance) {
        //if (__instance.transform.parent.name != "NeoCrest")
        //    return;
        Debug.Log($" -- {__instance.name} ds queuebounce | queuedBounce: {__instance.queuedBounce} | waitForTrigger: {__instance.waitForBounceTrigger} | triggerHit: {__instance.bounceTriggerHit} | cancustomrecoil: {HeroController.instance.CanCustomRecoil()} | {System.Environment.StackTrace}");
    }

    [HarmonyPatch(typeof(Downspike), nameof(Downspike.TryDownBounce))]
    [HarmonyPrefix]
    private static void downspiketrybounce(Downspike __instance) {
        //if (__instance.transform.parent.name != "NeoCrest")
        //    return;
        Debug.Log($" -- {__instance.name} ds trybounce | queuedBounce: {__instance.queuedBounce} | waitForTrigger: {__instance.waitForBounceTrigger} | triggerHit: {__instance.bounceTriggerHit} | cancustomrecoil: {HeroController.instance.CanCustomRecoil()} | {System.Environment.StackTrace}");
    }

    [HarmonyPatch(typeof(Downspike), nameof(Downspike.OnAnimationEventTriggered))]
    [HarmonyPrefix]
    private static void downspikeonanimevttrig(Downspike __instance) {
        //if (__instance.transform.parent.name != "NeoCrest")
        //    return;
        Debug.Log($" -- {__instance.name} ds onAnimEvtTriggered.. which shouldnt be possible... | {System.Environment.StackTrace}");
    }

}

#endif
