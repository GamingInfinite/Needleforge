using Needleforge.Attacks;
using Needleforge.Data;
using System.Linq;
using UnityEngine;
using ConfigGroup = HeroController.ConfigGroup;

namespace Needleforge.Makers;

internal class MovesetMaker
{
    private static ConfigGroup? hunter;

    internal static void InitializeMoveset(MovesetData moveset)
    {
        if (!TryFindDefaultMovesets())
            return;

        if (!moveset.HeroConfig)
            moveset.HeroConfig = HeroConfigNeedleforge.Copy(hunter!.Config);

        HeroController hc = HeroController.instance;

        GameObject root = new(moveset.Crest.name);
        root.transform.SetParent(hunter!.ActiveRoot.transform.parent);
        root.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        moveset.ConfigGroup = new ConfigGroup()
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
            ChargeSlash =       AttackOrDefault(moveset.ChargedSlash, hunter.ChargeSlash),
            TauntSlash =        AttackOrDefault(null, hunter.TauntSlash),

            AlternateSlashObject = moveset.AltSlash?.CreateGameObject(root, hc),
            AltUpSlashObject =     moveset.AltUpSlash?.CreateGameObject(root, hc),
            AltDownSlashObject =   moveset.AltDownSlash?.CreateGameObject(root, hc),
        };

        hc.configs = [.. hc.configs, moveset.ConfigGroup];

        moveset.ExtraInitialization();
        HeroConfigErrorChecking(moveset);
        moveset.ConfigGroup.Setup();

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

    private static void HeroConfigErrorChecking(MovesetData moveset) {
		HeroController hc = HeroController.instance;
		string
            name = moveset.Crest.name,
			m = nameof(CrestData.Moveset),
            mcfg = $"{m}.{nameof(MovesetData.HeroConfig)}",
            tcfg = $"{nameof(ToolCrest)}.{nameof(ToolCrest.HeroConfig)}",
            gcfg = $"{m}.{nameof(MovesetData.ConfigGroup)}.{nameof(ConfigGroup.Config)}",
            correctSetter = $"The only place you should set the moveset config is {mcfg}";

        // Config in MovesetData, ToolCrest, and ConfigGroup should be the exact same object.
		if (
			!ReferenceEquals(moveset.HeroConfig, moveset.Crest.ToolCrest!.HeroConfig)
			|| !ReferenceEquals(moveset.HeroConfig, moveset.ConfigGroup!.Config)
		) {
			ModHelper.LogWarning(
				$"{name}: {mcfg} object is not the same object as its {gcfg} and/or " +
                $"{tcfg}; this can cause issues with its attacks and save data. " +
                $"{correctSetter}");
		}

        // Config objects CANNOT be shared by reference between any two ToolCrests or ConfigGroups
		string sharedCfg = "is a direct reference to another crest's config. This can " +
			$"cause issues with both crests' attacks and save data. {correctSetter}";
		if (
			ToolItemManager.GetAllCrests().Except([moveset.Crest.ToolCrest])
			.Any(x => ReferenceEquals(x.HeroConfig, moveset.Crest.ToolCrest!.HeroConfig))
		) {
			ModHelper.LogError($"{name}: {tcfg} {sharedCfg}");
		}
		if (
			hc.configs.Except([moveset.ConfigGroup!])
			.Any(x => ReferenceEquals(x.Config, moveset.ConfigGroup!.Config))
		) {
			ModHelper.LogError($"{name}: {gcfg} {sharedCfg}");
		}

        // The crest's name and the name in its config MUST be identical
        if (name != moveset.HeroConfig!.name) {
            ModHelper.LogError(
                $"{name}: The crest's .{nameof(CrestData.name)} does not match the " +
                $"name in its {mcfg}. Custom attacks may not work. {correctSetter}");
        }
	}

}
