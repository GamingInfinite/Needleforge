using HarmonyLib;
using Needleforge.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Needleforge.Patches
{
    [HarmonyPatch(typeof(BindOrbHudFrame), nameof(BindOrbHudFrame.Awake))]
    public class AddHudRootsAndAnims
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

                if (data.HudFrame.HasAnyCustomAnims) {
                    List<tk2dSpriteAnimationClip>
                        library = [.. __instance.animator.Library.clips];
                    foreach(var anim in data.HudFrame.AllCustomAnims()){
                        library.AddIfNotPresent(anim);
                    }
                    __instance.animator.Library.clips = [.. library];
				}

                data.HudFrame.Initialize();
            }
            NeedleforgeHudRoots.transform.SetParent(__instance.transform);
        }
    }
}