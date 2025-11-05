using HarmonyLib;
using Needleforge.Data;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using CrestTypes = SaveProfileHealthBar.CrestTypes;

namespace Needleforge.Patches;

/// <summary>
/// Patches which enable custom crests to use custom HUD frames on the profile menu.
/// </summary>
[HarmonyPatch(typeof(SaveProfileHealthBar), nameof(SaveProfileHealthBar.ShowHealth))]
internal class ReplaceProfileHud {

    [HarmonyPostfix]
    private static void ReplaceHUD (SaveProfileHealthBar __instance, bool steelsoulMode, string crestId)
    {
        foreach(var crest in NeedleforgePlugin.newCrestData)
        {
            if (crest.name == crestId)
            {
                var fallback = __instance.crests[(int)ConvertCrestType(crest.HudFrame.Preset)];

                Sprite spool =
                    crest.HudFrame.ProfileIcon != null
                    ? crest.HudFrame.ProfileIcon
                    : fallback.SpoolImage;

                Sprite steelSpool =
                    crest.HudFrame.ProfileIconSteel != null
                    ? crest.HudFrame.ProfileIconSteel
                    : fallback.SpoolImageSteel;

                __instance.spoolImage.sprite = steelsoulMode ? steelSpool : spool;
            }
        }
    }

    private static CrestTypes ConvertCrestType(VanillaCrest crest) =>
        crest switch
        {
            VanillaCrest.HUNTER_V2 => CrestTypes.Hunter_v2,
            VanillaCrest.HUNTER_V3 => CrestTypes.Hunter_v3,
            VanillaCrest.BEAST => CrestTypes.Warrior,
            VanillaCrest.REAPER => CrestTypes.Reaper,
            VanillaCrest.WANDERER => CrestTypes.Wanderer,
            VanillaCrest.WITCH => CrestTypes.Witch,
            VanillaCrest.ARCHITECT => CrestTypes.Toolmaster,
            VanillaCrest.SHAMAN => CrestTypes.Spell,
            VanillaCrest.CURSED => CrestTypes.Cursed,
            VanillaCrest.CLOAKLESS => CrestTypes.Cloakless,
            _ => CrestTypes.Hunter,
        };


}
