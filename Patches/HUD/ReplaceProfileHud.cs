using HarmonyLib;
using Needleforge.Data;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using CrestTypes = SaveProfileHealthBar.CrestTypes;

namespace Needleforge.Patches.HUD;

/// <summary>
/// Patches which enable custom crests to use custom HUD frames on the profile menu.
/// </summary>
[HarmonyPatch(typeof(SaveProfileHealthBar), nameof(SaveProfileHealthBar.ShowHealth))]
internal static class ReplaceProfileHud {
    private static void Postfix(SaveProfileHealthBar __instance, bool steelsoulMode, string crestId)
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
                    crest.HudFrame.SteelProfileIcon != null
                    ? crest.HudFrame.SteelProfileIcon
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

/// <summary>
/// Patch that gets rid of the "could not parse crest id" error in the console.
/// </summary>
[HarmonyPatch]
internal static class ReplaceProfileHud_EnumTryParse
{
    static MethodBase TargetMethod()
        => typeof(Enum).GetMethods()
            .Single(x => x.Name == nameof(Enum.TryParse) && x.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(CrestTypes));

    static bool Prefix(string value, ref CrestTypes result, ref bool __result)
    {
        int i = NeedleforgePlugin.newCrestData.FindIndex(x => x.name == value);
        if (i >= 0)
        {
            result = CrestTypes.Hunter;
            __result = true;
            return false;
        }
        return true;
    }
}
