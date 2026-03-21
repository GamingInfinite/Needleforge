using GlobalEnums;
using HarmonyLib;
using Needleforge.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Needleforge.Patches.HUD;

[HarmonyPatch(typeof(BindOrbHudFrame), nameof(BindOrbHudFrame.Awake))]
internal class AddHudRootsAndAnims
{
    [HarmonyPostfix]
    private static void Postfix(BindOrbHudFrame __instance)
    {
        GameObject hudRootParent = new("NeedleforgeHudRoots") { layer = (int)PhysLayers.UI };
        hudRootParent.transform.SetParent(__instance.transform);
        ResetPos(hudRootParent);

        // ensures roots are by default layered above hud frame and below bind orb
        hudRootParent.transform.SetLocalPositionZ(-0.0001f);

        ModHelper.Log("Adding Needleforge Hud Roots");
        foreach (CrestData data in NeedleforgePlugin.newCrestData)
        {
            GameObject hudRoot = new($"{data.name}HUDRoot") { layer = (int)PhysLayers.UI };
            hudRoot.transform.SetParent(hudRootParent.transform);
            ResetPos(hudRoot);
            NeedleforgePlugin.hudRoots[data.name] = hudRoot;

            UpdateHudAnimLibrary(__instance, data.HudFrame);
            data.HudFrame.InitializeRoot();
        }

        static void ResetPos(GameObject go)
        {
            go.transform.localScale = Vector3.one;
            go.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }

    /// <summary>
    /// Adds all custom animations set on the <see cref="HudFrameData"/> to
    /// the animation libraries of the current <see cref="BindOrbHudFrame"/>.
    /// </summary>
    private static void UpdateHudAnimLibrary(BindOrbHudFrame hudFrame, HudFrameData hudData)
    {
        if (hudData.HasAnyRegularCustomAnims)
        {
            List<tk2dSpriteAnimationClip>
                library = [.. hudFrame.animator.Library.clips];
            foreach (var anim in hudData.AllRegularCustomAnims())
            {
                library.AddIfNotPresent(anim);
            }
            hudFrame.animator.Library.clips = [.. library];
            hudFrame.animator.Library.isValid = false;
            hudFrame.animator.Library.ValidateLookup();
        }

        if (hudData.HasAnySteelCustomAnims)
        {
            SteelSoulAnimProxy
                proxy = hudFrame.GetComponent<SteelSoulAnimProxy>();
            List<tk2dSpriteAnimationClip>
                library = [.. proxy.steelSoulAnims.clips];
            foreach(var anim in hudData.AllSteelCustomAnims())
            {
                library.AddIfNotPresent(anim);
            }
            proxy.steelSoulAnims.clips = [.. library];
            proxy.steelSoulAnims.isValid = false;
            proxy.steelSoulAnims.ValidateLookup();
        }
    }

    /// <summary>
    /// <para>
    /// <inheritdoc
    ///     cref="UpdateHudAnimLibrary(BindOrbHudFrame, HudFrameData)"
    ///     path="/summary"/>
    /// </para><para>
    /// Called by <see cref="HudFrameData"/> objects to cause the hud's animation
    /// library to receive changes to their animation properties even after the
    /// above <see cref="Postfix"/> runs.
    /// </para>
    /// </summary>
    internal static void UpdateHudAnimLibrary(HudFrameData hudData)
    {
        BindOrbHudFrame hudFrame = Object.FindAnyObjectByType<BindOrbHudFrame>();
        if (!hudData.Root || !hudFrame || !hudFrame.didAwake)
            return;
        UpdateHudAnimLibrary(hudFrame, hudData);
    }
}
