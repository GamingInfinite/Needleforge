using Needleforge.Attacks;
using Needleforge.Data;
using System.Linq;
using UnityEngine;
using ConfigGroup = HeroController.ConfigGroup;

namespace Needleforge.Makers;

internal class MovesetMaker {
    private static ConfigGroup? hunter;

    internal static void InitializeMoveset(MovesetData moveset)
    {
        if (!TryFindDefaultMovesets())
            return;

        if (!moveset.HeroConfig)
            moveset.HeroConfig = HeroConfigNeedleforge.Copy(hunter!.Config);

        HeroController hc = HeroController.instance;

        // Necessary for crests without any dash slash customization to function
        var heroClipLib = hc.AnimCtrl.animator.Library;
		if (heroClipLib.GetClipByName("Dash Attack 1") == null)
        {
            tk2dSpriteAnimationClip
                dashAtk = heroClipLib.GetClipByName("Dash Attack"),
                dashAntic = heroClipLib.GetClipByName("Dash Attack Antic");
            
            var newclips = heroClipLib.clips
                .Append(new()
                {
                    name = "Dash Attack 1", fps = dashAtk.fps, frames = dashAtk.frames,
                    loopStart = dashAtk.loopStart, wrapMode = dashAtk.wrapMode
                })
				.Append(new()
                {
                    name = "Dash Attack Antic 1", fps = dashAntic.fps, frames = dashAntic.frames,
                    loopStart = dashAntic.loopStart, wrapMode = dashAntic.wrapMode
                });

            heroClipLib.clips = [.. newclips];
            heroClipLib.isValid = false;
            heroClipLib.ValidateLookup();
		}

        GameObject root = new(moveset.Crest.name);
        root.transform.SetParent(hunter!.ActiveRoot.transform.parent);
        root.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        moveset.ConfGroup = new ConfigGroup()
        {
            ActiveRoot = root,
            Config = moveset.HeroConfig,

            // If the moveset doesn't define one of the minimum required attacks
            // for crests to function, copy it from Hunter
            NormalSlashObject = AttackOrDefault(moveset.Slash,     hunter.NormalSlashObject),
            UpSlashObject =     AttackOrDefault(moveset.UpSlash,   hunter.UpSlashObject),
            WallSlashObject =   AttackOrDefault(moveset.WallSlash, hunter.WallSlashObject),
            DownSlashObject =   AttackOrDefault(moveset.DownSlash, hunter.DownSlashObject),
            DashStab =          DashAttackOrDefault(moveset.DashSlash, hunter.DashStab),
            ChargeSlash =       AttackOrDefault(null, hunter.ChargeSlash),
            TauntSlash =        AttackOrDefault(null, hunter.TauntSlash),

            AlternateSlashObject = moveset.AltSlash?.CreateGameObject(root, hc),
            AltUpSlashObject =     moveset.AltUpSlash?.CreateGameObject(root, hc),
            AltDownSlashObject =   moveset.AltDownSlash?.CreateGameObject(root, hc),
		};

        hc.configs = [.. hc.configs, moveset.ConfGroup];

        moveset.ExtraInitialization();
        moveset.ConfGroup.Setup();

        GameObject? AttackOrDefault(GameObjectProxy? attack, GameObject? _default)
        {
            if (attack == null)
            {
                if (!_default)
                    return null;
                else
                {
                    GameObject clone = Object.Instantiate(_default, root.transform);
                    clone.name = clone.name.Replace("(Clone)", "");
                    return clone;
                }
            }
            return attack.CreateGameObject(root, hc);
        }

        GameObject? DashAttackOrDefault(GameObjectProxy? attack, GameObject? _default)
        {
            if (attack == null)
            {
                if (!_default)
                    return null;
                else
                {
                    GameObject cloneParent = new("Dash Stab Parent");
                    cloneParent.transform.parent = root.transform;
                    cloneParent.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    cloneParent.transform.localScale = Vector3.one;

                    GameObject clone = Object.Instantiate(_default, cloneParent.transform);
                    clone.name = clone.name.Replace("(Clone)", "");

                    return cloneParent;
                }
            }
            return attack.CreateGameObject(root, hc);
        }
    }

    private static bool TryFindDefaultMovesets() {
        HeroController hc = HeroController.instance;

        if (!hc)
            return false;

        if (hunter == null || !hunter.Config || !hunter.NormalSlashObject)
            hunter = hc.configs.First(c => c.Config.name == "Default");

        return true;
    }
}
