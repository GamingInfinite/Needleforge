using Needleforge.Attacks;
using Needleforge.Data;
using SharpDX.DirectInput;
using System.Linq;
using UnityEngine;
using static Needleforge.Data.VanillaAttacks;
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


        // In case of vanilla options
        #region Charged slash
        GameObject? Charged_Slash = null;

        if (moveset.UseVanillaChargedSlash != null)
        {
            GameObject? chargedPrefab = ChargedSlashes.GetChargedSlashForCrest(moveset.UseVanillaChargedSlash);
            if (chargedPrefab != null)
            {
                ClonedAttack clonedChargedAttack = new()
                {
                    OriginalObject = chargedPrefab,
                    Name = $"{moveset.Crest.name} {moveset.UseVanillaChargedSlash} Charged Slash clone"
                };
                Charged_Slash = clonedChargedAttack.CreateGameObject(root, hc);
            }
        }

        if (Charged_Slash == null)
            Charged_Slash = AttackOrDefault(moveset.ChargedSlash, hunter.ChargeSlash);
        #endregion

        #region Downslash

        #region Ensuring correct event is sent when using vanilla down slashes

        switch (moveset.UseVanillaDownSlash)
        {
            case VanillaCrest.BEAST:
            case VanillaCrest.BEAST_RAGE:
                moveset.HeroConfig.downSlashType = HeroControllerConfig.DownSlashTypes.Custom;
                moveset.HeroConfig.downSlashEvent = ToolItemManager.GetCrestByName("Warrior").HeroConfig.DownSlashEvent;
                break;
            case VanillaCrest.REAPER:
                moveset.HeroConfig.downSlashType = HeroControllerConfig.DownSlashTypes.Custom;
                moveset.HeroConfig.downSlashEvent = ToolItemManager.GetCrestByName("Reaper").HeroConfig.DownSlashEvent;
                break;
            case VanillaCrest.WITCH:
            case VanillaCrest.CURSED:
                moveset.HeroConfig.downSlashType = HeroControllerConfig.DownSlashTypes.Custom;
                moveset.HeroConfig.downSlashEvent = ToolItemManager.GetCrestByName("Witch").HeroConfig.DownSlashEvent;
                break;
            case VanillaCrest.SHAMAN:
                moveset.HeroConfig.downSlashType = HeroControllerConfig.DownSlashTypes.Custom;
                moveset.HeroConfig.downSlashEvent = ToolItemManager.GetCrestByName("Spell").HeroConfig.DownSlashEvent;
                break;
            case VanillaCrest.ARCHITECT:
                moveset.HeroConfig.downSlashType = HeroControllerConfig.DownSlashTypes.Custom;
                moveset.HeroConfig.downSlashEvent = ToolItemManager.GetCrestByName("Toolmaster").HeroConfig.DownSlashEvent;
                break;
            case VanillaCrest.HUNTER:
            case VanillaCrest.HUNTER_V2:
            case VanillaCrest.HUNTER_V3:
            case VanillaCrest.CLOAKLESS:
                moveset.HeroConfig.downSlashType = HeroControllerConfig.DownSlashTypes.DownSpike;
                break;
            case VanillaCrest.WANDERER:
                moveset.HeroConfig.downSlashType = HeroControllerConfig.DownSlashTypes.Slash;
                break;

        }

        #endregion
        GameObject? AltDownSlash = null;
        GameObject? DownSlash = null;

        if (moveset.UseVanillaDownSlash != null)
        {
            DownAttack? clonedDown = null;
            DownAttack? clonedAltDown = null;
            #region switch case for DownAttack
            switch (moveset.UseVanillaDownSlash)
            {
                case VanillaCrest.BEAST:
                    clonedDown = DownSlashes.BeastCopy();
                    break;
                case VanillaCrest.BEAST_RAGE:
                    clonedDown = DownSlashes.BeastRageCopy();
                    break;
                case VanillaCrest.REAPER:
                    clonedDown = DownSlashes.ReaperCopy();
                    break;
                case VanillaCrest.WITCH:
                case VanillaCrest.CURSED:
                    clonedDown = DownSlashes.WitchCopy();
                    break;
                case VanillaCrest.SHAMAN:
                    clonedDown = DownSlashes.ShamanCopy();
                    break;
                case VanillaCrest.ARCHITECT:
                    clonedDown = DownSlashes.ArchitectCopy();
                    clonedAltDown = DownSlashes.ArchitectChargedCopy();
                    break;
                case VanillaCrest.HUNTER:
                case VanillaCrest.HUNTER_V2:
                case VanillaCrest.HUNTER_V3:
                    clonedDown = DownSlashes.HunterCopy();
                    break;
                case VanillaCrest.CLOAKLESS:
                    clonedDown = DownSlashes.CloaklessCopy();
                    break;
                case VanillaCrest.WANDERER:
                    clonedDown = DownSlashes.WandererCopy();
                    break;
            }
            #endregion

            if (clonedDown != null)
                moveset.DownSlash = clonedDown;
            if (clonedAltDown != null)
                moveset.AltDownSlash = clonedAltDown;
        }

        DownSlash = AttackOrDefault(moveset.DownSlash, hunter.DownSlashObject);
        
        if (moveset.AltDownSlash != null)
            AltDownSlash = moveset.AltDownSlash.CreateGameObject(root, hc);
        #endregion

        #region Dash slash
        GameObject? DashSlash = null;

        if (moveset.UseVanillaDashSlash != null)
        {
            DashAttack? clonedDash = null;
            #region switch case for DashAttack
            switch (moveset.UseVanillaDashSlash)
            {
                case VanillaCrest.BEAST:
                    clonedDash = DashSlashes.BeastCopy();
                    break;
                case VanillaCrest.BEAST_RAGE:
                    clonedDash = DashSlashes.BeastRageCopy();
                    break;
                case VanillaCrest.REAPER:
                    clonedDash = DashSlashes.ReaperCopy();
                    break;
                case VanillaCrest.WITCH:
                case VanillaCrest.CURSED:
                    clonedDash = DashSlashes.WitchCopy();
                    break;
                case VanillaCrest.SHAMAN:
                    clonedDash = DashSlashes.ShamanCopy();
                    break;
                case VanillaCrest.ARCHITECT:
                    clonedDash = DashSlashes.ArchitectCopy();
                    break;
                case VanillaCrest.HUNTER:
                case VanillaCrest.HUNTER_V2:
                case VanillaCrest.HUNTER_V3:
                    clonedDash = DashSlashes.HunterCopy();
                    break;
                case VanillaCrest.CLOAKLESS:
                    clonedDash = DashSlashes.CloaklessCopy();
                    break;
                case VanillaCrest.WANDERER:
                    clonedDash = DashSlashes.WandererCopy();
                    break;
            }
            #endregion

            if (clonedDash != null)
                moveset.DashSlash = clonedDash;
        }

        DashSlash = AttackOrDefault(moveset.DashSlash, hunter.DashStab);
        #endregion

        moveset.ConfigGroup = new ConfigGroup()
        {
            ActiveRoot = root,
            Config = moveset.HeroConfig,

            // If the moveset doesn't define one of the minimum required attacks
            // for crests to function, copy it from Hunter
            NormalSlashObject = AttackOrDefault(moveset.Slash,     hunter.NormalSlashObject),
            UpSlashObject =     AttackOrDefault(moveset.UpSlash,   hunter.UpSlashObject),
            WallSlashObject =   AttackOrDefault(moveset.WallSlash, hunter.WallSlashObject),
            DownSlashObject =   DownSlash,
            DashStab =          DashSlash,
            DashStabAlt =       null,
            ChargeSlash =       Charged_Slash,
            TauntSlash =        AttackOrDefault(null, hunter.TauntSlash),

            AlternateSlashObject = moveset.AltSlash?.CreateGameObject(root, hc),
            AltUpSlashObject =     moveset.AltUpSlash?.CreateGameObject(root, hc),
            AltDownSlashObject = AltDownSlash,
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
