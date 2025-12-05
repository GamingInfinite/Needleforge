using Needleforge.Data;
using System.Linq;
using UnityEngine;
using ConfigGroup = HeroController.ConfigGroup;

namespace Needleforge.Makers;

internal class MovesetMaker {
    private static ConfigGroup? hunter, witch;

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
            DashStab =          DashAttackOrDefault(moveset.DashSlash, hunter.DashStab, witch!.DashStab),
            ChargeSlash =       AttackOrDefault(null, hunter.ChargeSlash),
            TauntSlash =        AttackOrDefault(null, hunter.TauntSlash),

            AlternateSlashObject = moveset.AltSlash?.CreateGameObject(root, hc),
            AltUpSlashObject =     moveset.AltUpSlash?.CreateGameObject(root, hc),
            AltDownSlashObject =   moveset.AltDownSlash?.CreateGameObject(root, hc),
            DashStabAlt =          null,
        };

        hc.configs = [.. hc.configs, moveset.ConfGroup];

        moveset.ExtraInitialization();
        moveset.ConfGroup.Setup();

        GameObject? AttackOrDefault(AttackBase? attack, GameObject? _default)
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

        GameObject? DashAttackOrDefault(DashAttack? attack, GameObject? defaultSingle, GameObject? defaultMulti)
        {
            if (moveset.HeroConfig!.dashStabSteps <= 1)
                return AttackOrDefault(attack, defaultSingle);
            else
                return AttackOrDefault(attack, defaultMulti);

            //if (attack == null) {
            //    if (!defaultMulti)
            //        return null;
            //    else {
            //        GameObject clone = Object.Instantiate(defaultMulti, root.transform);
            //        clone.name = clone.name.Replace("(Clone)", "");
            //        for (int i = 2; i < moveset.HeroConfig!.dashStabSteps; i++) {
            //            int copyIndex = (i % 2 == 0) ? 1 : 0;
            //            GameObject subAttack = Object.Instantiate(clone.transform.GetChild(copyIndex).gameObject, clone.transform);
            //        }
            //        return clone;
            //    }
            //}
            //return attack.CreateGameObject(root, hc);
        }
    }

    private static bool TryFindDefaultMovesets() {
        HeroController hc = HeroController.instance;

        if (!hc)
            return false;

        if (hunter == null || !hunter.Config || !hunter.NormalSlashObject)
            hunter = hc.configs.First(c => c.Config.name == "Default");
        
        if (witch == null || !witch.Config || !witch.NormalSlashObject)
            witch = hc.configs.First(c => c.Config.name == "Whip");

        return true;
    }
}
