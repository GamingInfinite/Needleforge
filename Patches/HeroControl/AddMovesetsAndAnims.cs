using HarmonyLib;
using Needleforge.Data;
using Needleforge.Makers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PlayerDataTest;

namespace Needleforge.Patches.HeroControl;

[HarmonyPatch(typeof(HeroController))]
internal static class AddMovesetsAndAnims
{
    [HarmonyPatch(nameof(HeroController.Awake))]
    [HarmonyPostfix]
    private static void InitMovesets(HeroController __instance)
    {
        ModHelper.Log("Fetching vanilla references...");
        VanillaReferences.InitialiseReferences();
        ModHelper.Log("Initializing Crest Movesets...");
        foreach (var crest in NeedleforgePlugin.newCrestData)
        {
            ModHelper.Log($"Init {crest.name} Moveset");
            TryAddDefaultAnimations(__instance);
            MovesetMaker.InitializeMoveset(crest.Moveset);
            TryCloneVanillaAnimations(__instance, crest);
        }

        //For building the VanillaAttacks classes.
        #if DEBUG
        VanillaAttacks.Debug.LogAllAttackInfo();
        #endif
    }

    [HarmonyPatch(nameof(HeroController.SetConfigGroup))]
    [HarmonyPrefix]
    private static bool SilenceError(HeroController __instance, HeroController.ConfigGroup configGroup)
    {
        // only on loading a save
        if (__instance.didStart)
            return true;

        // if this is a needleforge crest abort silently instead of loudly
        if (configGroup == null && NeedleforgePlugin.newCrests.Any(x => ReferenceEquals(x.HeroConfig, __instance.crestConfig)))
            return false;

        return true;
    }

    /// <summary>
    /// Keys are the names of animations that custom crests need for their attacks to
    /// function.
    /// Values contain the name of an existing default animation to copy to create the
    /// required one, and whether or not the copy should keep the original's triggers.
    /// </summary>
    private static readonly Dictionary<string, (string orig, bool keepTriggers)>
        requiredAnimations = new() {
            // Helpful for down attacks
            { "DownSlash", ("DownSpike", true) },
            { "DownSlashAlt", ("DownSpike", true) },

            // Helpful for charged slashes
            { "Slash_Charged_Loop", ("Slash_Charged", false) },

            // Necessary for crests without any dash slash customization to function
            { "Dash Attack 1", ("Dash Attack", true) },
            { "Dash Attack Antic 1", ("Dash Attack Antic", true) },
        };

    /// <summary>
    /// Creates copies of several of Hornet's default attack animations with new names
    /// and adds them to her animation library, to ensure attacks on custom crests are
    /// still reasonably functional even if no hero override anim library was provided.
    /// </summary>
    private static void TryAddDefaultAnimations(HeroController hc)
    {
        tk2dSpriteAnimation heroClipLib = hc.AnimCtrl.animator.Library;
        List<tk2dSpriteAnimationClip> newclips = [];

        foreach (var (needed, (template, keepTriggers)) in requiredAnimations)
        {
            if (heroClipLib.GetClipByName(needed) == null)
            {
                tk2dSpriteAnimationClip templateAnim = heroClipLib.GetClipByName(template);
                newclips.Add(CopyClip(needed, templateAnim, keepTriggers));
            }
        }

        // One extra one - copying Wanderer's UpSlash for UpSlashAlt.
        // Hunter doesn't have one, so can't use requiredAnimations.
        tk2dSpriteAnimation wandererLib = HeroController.instance.configs.First(
            x => x.Config.name == "Wanderer").Config.heroAnimOverrideLib;

        newclips.Add(CopyClip("UpSlashAlt", wandererLib.GetClipByName("UpSlash"), true));

        if (newclips.Count > 0)
        {
            heroClipLib.clips = [.. heroClipLib.clips, .. newclips];
            heroClipLib.isValid = false;
            heroClipLib.ValidateLookup();
        }
    }

    /// <summary>
    /// In the case of certain cloned vanilla attacks being used, certain animations must be cloned.
    /// This must be used after the moveset has been initialised to allow the user to set their
    /// own custom animations if they wish. This adds to the crest's override library, and not
    /// the default one.
    /// </summary>
    private static void TryCloneVanillaAnimations(HeroController hc, CrestData crest)
    {
        TryCloneVanillaDownSlashAnimations(hc, crest);
        TryCloneVanillaDashSlashAnimations(hc, crest);
        TryCloneVanillaChargedSlashAnimations(hc, crest);
    }

    private static void TryCloneVanillaDownSlashAnimations(HeroController hc, CrestData crest)
    {
        RequiredAnimationSet set = new RequiredAnimationSet()
        {
            BeastAnims = [ "SpinBall Antic", "SpinBall Launch",
                    "SpinBall", "SpinBall Grind", "SpinBall Rebound" ],
            ReaperAnims = ["v3 Down Slash Antic", "v3 Down Slash"],
            WitchAnims = ["DownSpike", "DownSpike Antic", "Downspike Followup"],
            ShamanAnims = ["DownSlash"],
            ArchitectAnims = [ "DownSpike Charge", "DownSpike Antic", "DownSpike", "DownSpike Charged",
                "Drill Grind", "Drill Grind Charged"],
            WandererAnims = ["DownSlash"],
        };

        TryCloneVanillaAnimationSet(crest, crest.Moveset.UseVanillaDownSlash, set);
    }

    private static void TryCloneVanillaChargedSlashAnimations(HeroController hc, CrestData crest)
    {
        RequiredAnimationSet set = new RequiredAnimationSet()
        {
            BeastAnims = ["NeedleArt Dash"]
        };

        TryCloneVanillaAnimationSet(crest, crest.Moveset.UseVanillaChargedSlash, set);
    }

    private static void TryCloneVanillaDashSlashAnimations(HeroController hc, CrestData crest)
    {
        RequiredAnimationSet set = new RequiredAnimationSet()
        {
            BeastAnims = [ "Dash Attack Antic", "Dash Attack Leap",
                "Dash Attack Slash" ],
            ReaperAnims = ["Dash Upper Antic", "Dash Upper",
                "Dash Upper Recovery"],
            ShamanAnims = ["Dash Attack Antic", "Dash Attack Leap",
                "Dash Attack Slash"],
            ArchitectAnims = ["Dash Attack Charge", "Dash Attack"],
            WandererAnims = ["Wanderer Dash Attack", "Wanderer Dash Attack Alt",
                "Wanderer DashRecoil", "Wanderer RecoilStab"],
            WitchAnims = [ "Dash Attack Antic 1", "Dash Attack 1",
                "Dash Attack Recover", "Dash Attack Antic 2", "Dash Attack 2"]
        };

        TryCloneVanillaAnimationSet(crest, crest.Moveset.UseVanillaDashSlash, set);
    }

    /// <summary>
    /// Helper class to store names of required animations.
    /// Hunter is never required as it is default.
    /// </summary>
    private class RequiredAnimationSet
    {
        public string[] BeastAnims = [];
        public string[] ReaperAnims = [];
        public string[] WitchAnims = [];
        public string[] ShamanAnims = [];
        public string[] ArchitectAnims = [];
        public string[] CloaklessAnims = [];
        public string[] WandererAnims = [];
    }

    /// <summary>
    /// Core function to clone vanilla animations.
    /// Checks attack type, selects correct animation set, and clones.
    /// Does nothing if no attack type or animations are specified for the crest.
    /// </summary>
    private static void TryCloneVanillaAnimationSet(CrestData crest, VanillaCrest? attackType, RequiredAnimationSet requiredAnims)
    {
        string[] requiredAnimations = { };
        string libraryName = "";

        if (attackType == null) { return; }

        switch (attackType)
        {
            case VanillaCrest.BEAST:
            case VanillaCrest.BEAST_RAGE:
                requiredAnimations = requiredAnims.BeastAnims;
                libraryName = VanillaReferences.AnimationLibraryNames.BEAST;
                break;
            case VanillaCrest.REAPER:
                requiredAnimations = requiredAnims.ReaperAnims;
                libraryName = VanillaReferences.AnimationLibraryNames.REAPER;
                break;
            case VanillaCrest.WITCH:
            case VanillaCrest.CURSED:
                requiredAnimations = requiredAnims.WitchAnims;
                libraryName = VanillaReferences.AnimationLibraryNames.WITCH;
                break;
            case VanillaCrest.SHAMAN:
                requiredAnimations = requiredAnims.ShamanAnims;
                libraryName = VanillaReferences.AnimationLibraryNames.SHAMAN;
                break;
            case VanillaCrest.ARCHITECT:
                requiredAnimations = requiredAnims.ArchitectAnims;
                libraryName = VanillaReferences.AnimationLibraryNames.ARCHITECT;
                break;
            case VanillaCrest.CLOAKLESS:
                requiredAnimations = requiredAnims.CloaklessAnims;
                libraryName = VanillaReferences.AnimationLibraryNames.CLOAKLESS;
                break;
            case VanillaCrest.WANDERER:
                requiredAnimations = requiredAnims.WandererAnims;
                libraryName = VanillaReferences.AnimationLibraryNames.WANDERER;
                break;
        }

        if (requiredAnimations.Length == 0)
            return;

        CloneAnimationsToIfNotExists(crest, libraryName, requiredAnimations);
    }

    /// <summary>
    /// Copies an animation clip and gives the copy a new name.
    /// If <paramref name="keepTriggers"/> = false new frames without any event triggers
    /// will be created for the copy; otherwise the same frame objects are used.
    /// </summary>
    private static tk2dSpriteAnimationClip CopyClip(string newName, tk2dSpriteAnimationClip orig, bool keepTriggers)
    {
        var frames = orig.frames;
        if (!keepTriggers)
            frames = [..frames.Select(f => new tk2dSpriteAnimationFrame() {
                spriteCollection = f.spriteCollection,
                spriteId = f.spriteId,
                triggerEvent = false,
            })];

        return new() {
            name = newName,
            fps = orig.fps,
            frames = frames,
            loopStart = orig.loopStart,
            wrapMode = orig.wrapMode
        };
    }

    //Helpers to clone animatins from vanilla libraries to custom libraries
    private static void CloneAnimationTo(tk2dSpriteAnimation libraryToCloneTo, string libraryNameToCloneFrom, string cloneAnimationName)
    {
        foreach (HeroController.ConfigGroup configGroup in HeroController.instance.configs)
        {
            HeroControllerConfig config = configGroup.Config;
            if (config == null) { continue; }

            tk2dSpriteAnimation library = config.heroAnimOverrideLib;
            if (library == null) { continue; } // Ignore default, we should never have to clone from it.

            if (library.name != libraryNameToCloneFrom) { continue; }

            tk2dSpriteAnimationClip clip = library.GetClipByName(cloneAnimationName);
            if (clip == null)
            {
                ModHelper.LogError($"Animation {cloneAnimationName} not found in library {libraryToCloneTo}." +
                "Failed to make clone.");
            }

            tk2dSpriteAnimationClip clone = new tk2dSpriteAnimationClip();
            clone.CopyFrom(clip);

            List<tk2dSpriteAnimationClip> list = libraryToCloneTo.clips.ToList<tk2dSpriteAnimationClip>();
            list.Add(clone);

            libraryToCloneTo.clips = list.ToArray();
            libraryToCloneTo.isValid = false;
            libraryToCloneTo.ValidateLookup();
            ModHelper.Log($"Cloned animation {cloneAnimationName} from {libraryNameToCloneFrom} to {libraryToCloneTo.name}.");
        }
    }
    private static void CloneAnimationsToIfNotExists(CrestData crestToCloneTo, string libraryNameToCloneFrom, string[] cloneAnimationNames)
    {
        HeroConfigNeedleforge? config = crestToCloneTo.Moveset.HeroConfig;
        if (config == null) { return; }

        tk2dSpriteAnimation? library = config.heroAnimOverrideLib;
        if (library == null)
        {
            GameObject libobj = new GameObject($"{crestToCloneTo.name}LibraryObject");
            GameObject.DontDestroyOnLoad(libobj);

            library = libobj.AddComponent<tk2dSpriteAnimation>();
            library.clips = Array.Empty<tk2dSpriteAnimationClip>();
            config.heroAnimOverrideLib = library;

            ModHelper.Log($"Crest {crestToCloneTo.name} has no animation library, but requests to clone vanilla animations." +
                "Created new library and assigned to crest.");
        }

        foreach (string anim in cloneAnimationNames)
        {
            if (library.GetClipByName(anim) != null) { continue; }

            ModHelper.Log($"Required animation {anim} for crest {crestToCloneTo.name} not found." +
                $"Cloning from vanilla {libraryNameToCloneFrom}.");
            CloneAnimationTo(library, libraryNameToCloneFrom, anim);
        }
    }
}
