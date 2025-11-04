using HarmonyLib;
using Needleforge.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Needleforge.Patches
{
    [HarmonyPatch(typeof(BindOrbHudFrame), nameof(BindOrbHudFrame.Awake))]
    public class AddHudRoots
    {
        [HarmonyPostfix]
        public static void Postfix(BindOrbHudFrame __instance)
        {
            GameObject NeedleforgeHudRoots = new GameObject("NeedleforgeHudRoots");
            ModHelper.Log("Adding Needleforge Hud Roots");
            foreach (CrestData data in NeedleforgePlugin.newCrestData)
            {
                GameObject hudRoot = new GameObject($"{data.name}HUDRoot");
                hudRoot.transform.SetParent(NeedleforgeHudRoots.transform);
                NeedleforgePlugin.hudRoots[data.name] = hudRoot;

                if (data.HasCustomHudAnims) {
                    List<tk2dSpriteAnimationClip> library = [.. __instance.animator.Library.clips];
                    foreach(var anim in data.AllCustomAnims)
                        library.AddIfNotPresent(anim);
                }

                data.InitializeHud();
            }
            NeedleforgeHudRoots.transform.SetParent(__instance.transform);
        }
    }
}