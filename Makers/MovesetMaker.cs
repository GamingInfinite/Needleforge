using Needleforge.Data;
using System.Linq;
using UnityEngine;
using static iTween;
using ConfigGroup = HeroController.ConfigGroup;

namespace Needleforge.Makers;

internal class MovesetMaker {
    private static ConfigGroup? hunter;

    internal static void InitializeMoveset(MovesetData moveset)
    {
        if (!TryFindDefaultMoveset())
        {
            ModHelper.LogError($"Failed to initialize moveset for {moveset.Crest.name}; HeroController not found.");
            return;
        }

        if (!moveset.HeroConfig)
        {
            moveset.HeroConfig = Object.Instantiate(hunter.Config);
        }

        HeroController hc = HeroController.instance;

        // We have to copy the hunter root directly to get default copies of its attacks to work, for some reason
        GameObject root = Object.Instantiate(hunter.ActiveRoot, hunter.ActiveRoot.transform.parent);
        GameObject chargedDefault = Object.Instantiate(hunter.ChargeSlash, root.transform);

        root.name = moveset.Crest.name;
        chargedDefault.name = chargedDefault.name.Replace("(Clone)", "");

        // Only use copied hunter attacks for attacks the moveset doesn't define

        moveset.ConfGroup = new ConfigGroup()
        {
            ActiveRoot = root,
            Config = moveset.HeroConfig,

            NormalSlashObject = AttackOrDefault(moveset.Slash, hunter.NormalSlashObject, root, hc),
            AlternateSlashObject = AttackOrDefault(moveset.SlashAlt, hunter.AlternateSlashObject, root, hc),
            UpSlashObject = AttackOrDefault(moveset.UpSlash, hunter.UpSlashObject, root, hc),
            WallSlashObject = AttackOrDefault(moveset.WallSlash, hunter.WallSlashObject, root, hc),

            DownSlashObject = AttackOrDefault(null, hunter.DownSlashObject, root, hc),
            DashStab = AttackOrDefault(null, hunter.DashStab, root, hc),
            ChargeSlash = AttackOrDefault(null, hunter.ChargeSlash, root, hc),
        };

        hc.configs = [.. hc.configs, moveset.ConfGroup];

        moveset.ConfGroup.Setup();
    }

    private static GameObject AttackOrDefault(Attack? attack, GameObject _default, GameObject parent, HeroController hc)
    {
        GameObject defaultCopy = parent.transform.Find(_default.name).gameObject;

        if (attack == null)
            return defaultCopy;

        Object.Destroy(defaultCopy);
        return attack.CreateGameObject(parent, hc);
    }

    private static bool TryFindDefaultMoveset() {
        HeroController hc = HeroController.instance;

        if (!hc)
            return false;

        if (hunter == null || !hunter.NormalSlashObject)
        {
            hunter = hc.configs.First(c => c.Config.name == "Default");
        }

        return true;
    }
}
