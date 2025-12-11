using HarmonyLib;
using Needleforge.Makers;
using System.Collections.Generic;
using System.Linq;

namespace Needleforge.Patches.HeroControl;

[HarmonyPatch(typeof(HeroController), nameof(HeroController.Awake))]
internal class AddMovesetsAndAnims
{
    private static void Postfix(HeroController __instance)
    {
        ModHelper.Log("Initializing Crest Movesets...");
        foreach (var crest in NeedleforgePlugin.newCrestData) {
            ModHelper.Log($"Init {crest.name} Moveset");
            TryAddDefaultAnimations(__instance);
            MovesetMaker.InitializeMoveset(crest.Moveset);
        }
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

        if (newclips.Count > 0)
        {
            heroClipLib.clips = [.. heroClipLib.clips, .. newclips];
            heroClipLib.isValid = false;
            heroClipLib.ValidateLookup();
        }
    }

    /// <summary>
    /// Copies an animation clip and gives the copy a new name.
    /// If <paramref name="keepTriggers"/> = false new frames without any event triggers
    /// will be created for the copy; otherwise the same frame objects are used.
    /// </summary>
    private static tk2dSpriteAnimationClip CopyClip(string name, tk2dSpriteAnimationClip orig, bool keepTriggers)
    {
        var frames = orig.frames;
        if (!keepTriggers)
            frames = [..frames.Select(f => new tk2dSpriteAnimationFrame() {
                spriteCollection = f.spriteCollection,
                spriteId = f.spriteId,
                triggerEvent = false,
            })];

        return new() {
            name = name,
            fps = orig.fps,
            frames = frames,
            loopStart = orig.loopStart,
            wrapMode = orig.wrapMode
        };
    }

}
