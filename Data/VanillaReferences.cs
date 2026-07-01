using System.Linq;
using UnityEngine;

namespace Needleforge.Data;

/// <summary>
/// Stores references to vanilla sounds and animation libraries for use in cloning.
/// </summary>
public class VanillaReferences
{
#pragma warning disable CS1591 // Missing XML comment
    /// <summary>
    /// Contains the library names for each of the crests.
    /// </summary>
    public static class AnimationLibraryNames
    {

        public static readonly string SHAMAN = "Hornet CrestWeapon Shaman Anim";
        public static readonly string WANDERER = "Hornet CrestWeapon Dagger Anim";
        public static readonly string WITCH = "Hornet CrestWeapon Whip Anim";
        public static readonly string REAPER = "Hornet CrestWeapon Scythe Anim";
        public static readonly string BEAST = "Hornet CrestWeapon Warrior Anim";
        public static readonly string CLOAKLESS = "Hornet Cloakless Anim";
        public static readonly string ARCHITECT = "Hornet CrestWeapon Drill Lance Anim";
        public static readonly string DEFAULT = "Knight";
    }

    private static tk2dSpriteAnimation? HunterLibrary;
    private static tk2dSpriteAnimation? BeastLibrary;
    private static tk2dSpriteAnimation? ReaperLibrary;
    private static tk2dSpriteAnimation? WitchLibrary;
    private static tk2dSpriteAnimation? ShamanLibrary;
    private static tk2dSpriteAnimation? ArchitectLibrary;
    private static tk2dSpriteAnimation? CloaklessLibrary;
    private static tk2dSpriteAnimation? WandererLibrary;

    internal static void InitialiseReferences()
    {
        HunterLibrary = HeroController.instance.GetComponent<tk2dSpriteAnimator>().library;
        BeastLibrary = ToolItemManager.GetCrestByName("Warrior").HeroConfig.heroAnimOverrideLib;
        ReaperLibrary = ToolItemManager.GetCrestByName("Reaper").HeroConfig.heroAnimOverrideLib;
        WitchLibrary = ToolItemManager.GetCrestByName("Witch").HeroConfig.heroAnimOverrideLib;
        ShamanLibrary = ToolItemManager.GetCrestByName("Spell").HeroConfig.heroAnimOverrideLib;
        ArchitectLibrary = ToolItemManager.GetCrestByName("Toolmaster").HeroConfig.heroAnimOverrideLib;
        CloaklessLibrary = ToolItemManager.GetCrestByName("Cloakless").HeroConfig.heroAnimOverrideLib;
        WandererLibrary = ToolItemManager.GetCrestByName("Wanderer").HeroConfig.heroAnimOverrideLib;
    }

    /// <summary>
    /// Fetches the animation library reference for a given crest type.
    /// </summary>
    /// <param name="crestType"></param>
    /// <returns></returns>
    public static tk2dSpriteAnimation? GetLibraryForCrestType(VanillaCrest? crestType)
    {
        return crestType switch
        {
            VanillaCrest.HUNTER => HunterLibrary,
            VanillaCrest.HUNTER_V2 => HunterLibrary,
            VanillaCrest.HUNTER_V3 => HunterLibrary,
            VanillaCrest.BEAST => BeastLibrary,
            VanillaCrest.BEAST_RAGE => BeastLibrary,
            VanillaCrest.REAPER => ReaperLibrary,
            VanillaCrest.WITCH => WitchLibrary,
            VanillaCrest.CURSED => WitchLibrary,
            VanillaCrest.SHAMAN => ShamanLibrary,
            VanillaCrest.ARCHITECT => ArchitectLibrary,
            VanillaCrest.CLOAKLESS => CloaklessLibrary,
            VanillaCrest.WANDERER => WandererLibrary,
            _ => null,
        };
    }

    public static AudioClip? GetAudioClipForCrestType(VanillaCrest? crestType)
    {
        GameObject hornet = HeroController.instance.gameObject;
        Transform attacks = hornet.transform.Find("Attacks");

        AudioClip? clip = null;
        string crestName = "";

        switch (crestType)
        {
            case VanillaCrest.HUNTER:
            case VanillaCrest.HUNTER_V2:
            case VanillaCrest.HUNTER_V3:
                crestName = "Default"; break;
            case VanillaCrest.BEAST:
                crestName = "Warrior"; break;
            case VanillaCrest.ARCHITECT:
                crestName = "Toolmaster"; break;
            case VanillaCrest.WITCH:
            case VanillaCrest.CURSED:
                crestName = "Whip"; break;
            case VanillaCrest.REAPER:
                crestName = "Reaper"; break;
            case VanillaCrest.CLOAKLESS:
                crestName = "Cloakless"; break;
            case VanillaCrest.WANDERER:
                crestName = "Wanderer"; break;
            case VanillaCrest.SHAMAN:
                crestName = "Shaman"; break;
        }

        GameObject normalSlash = HeroController.instance.configs
            .First(x => x.Config.name == crestName)
            .NormalSlashObject;

        AudioSource source = normalSlash.gameObject.GetComponent<AudioSource>();
        if (source == null) {
            ModHelper.LogWarning($"AudioSource for vanilla crest type {crestType} normal slash not found, returning null.");
            return null;
        }
        clip = normalSlash.GetComponent<AudioSource>().clip;

        if (clip == null)
        {
            ModHelper.LogWarning($"AudioClip for vanilla crest type {crestType} not found, returning null.");
        }

        return clip;
    }
}

