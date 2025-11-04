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

                if (data.HudFrame.HasAnyCustomAnims)
                {
                    List<tk2dSpriteAnimationClip>
                        library = [.. __instance.animator.Library.clips];
                    foreach(var anim in data.HudFrame.AllCustomAnims())
                    {
                        library.AddIfNotPresent(anim);
                    }
                    __instance.animator.Library.clips = [.. library];
                }

                data.HudFrame.InitializeRoot();
            }
            NeedleforgeHudRoots.transform.SetParent(__instance.transform);
        }

        /// <summary>
        /// Called by <see cref="HudFrameData"/> objects to cause the hud's animation
        /// library to receive changes to their animation properties even after the
        /// above <see cref="Postfix"/> runs.
        /// </summary>
        internal static void UpdateHudAnimLibrary(HudFrameData hudData)
        {
            BindOrbHudFrame hudFrame = Object.FindAnyObjectByType<BindOrbHudFrame>();
            if (!hudData.Root || !hudFrame || !hudFrame.enabled || !hudFrame.didAwake)
                return;

            if (hudData.HasAnyCustomAnims)
            {
                List<tk2dSpriteAnimationClip>
                    library = [.. hudFrame.animator.Library.clips];
                foreach (var anim in hudData.AllCustomAnims())
                {
                    library.AddIfNotPresent(anim);
                }
                hudFrame.animator.Library.clips = [.. library];
            }
        }
    }
}