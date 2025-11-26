using UnityEngine;
using DownSlashTypes = HeroControllerConfig.DownSlashTypes;

namespace Needleforge.Data;

public class DownAttack : AttackBase
{
    #region API

    public override string AnimName
    {
        get => _animName;
        set {
            _animName = value;
            if (GameObject)
            {
                if (nailSlash) nailSlash.animName = value;
                if (downspike) downspike.animName = value;
            }
        }
    }
    private string _animName = "";

    #endregion

    internal HeroControllerConfig? HeroConfig;

    private HeroDownAttack? heroDownAttack;
    private Downspike? downspike;
    private NailSlash? nailSlash;

    protected override NailAttackBase? NailAttackBase
    {
        get =>
            HeroConfig!.downSlashType switch
            {
                DownSlashTypes.Slash => nailSlash,
                DownSlashTypes.DownSpike => downspike,
                _ => throw new System.NotImplementedException()
            };
    }

    internal override GameObject CreateGameObject(GameObject parent, HeroController hc)
    {
        Debug.LogWarning("I am going to scream");
        base.CreateGameObject(parent, hc);
        Debug.LogWarning("please print dude im begging you");
        GameObject!.SetActive(false); // VERY IMPORTANT

        heroDownAttack = GameObject.AddComponent<HeroDownAttack>();
        heroDownAttack.hc = hc;

        if (!HeroConfig)
            throw new System.Exception($"{nameof(HeroConfig)} must be set for a down attack to know what kind of down attack it is.");

        switch (HeroConfig.downSlashType) {
            case DownSlashTypes.DownSpike:
                Debug.LogWarning("DOWNSPIKE");
                // Common component initialization

                downspike = GameObject.AddComponent<Downspike>();
                heroDownAttack.attack = downspike;

                downspike.hc = hc;
                downspike.activateOnSlash = [];
                downspike.enemyDamager = damager;
                downspike.heroBox = hc.heroBox;
                downspike.horizontalKnockbackDamager = hc.transform.Find("Attacks/Downspike Knockback Top").GetComponent<DamageEnemies>();
                downspike.verticalKnockbackDamager = hc.transform.Find("Attacks/Downspike Knockback Bottom").GetComponent<DamageEnemies>();

                // Customizations

                downspike.animName = AnimName;
                Debug.LogWarning("DOWNSPIKE DONE");

                break;
            case DownSlashTypes.Slash:
                Debug.LogWarning("DOWNSLASH NOT A SPIKE");
                // Common component initialization

                nailSlash = GameObject.AddComponent<NailSlash>();
                heroDownAttack.attack = nailSlash;

                nailSlash.hc = hc;
                nailSlash.activateOnSlash = [];
                nailSlash.enemyDamager = damager;

                // Customizations

                nailSlash.animName = AnimName;
                Debug.LogWarning("DOWNSLASH NOT A SPIKE DONE");

                break;
            default:
                throw new System.NotImplementedException();
        }

        NailAttackBase!.scale = Scale;
        NailAttackBase!.AttackStarting += TintIfNotImbued;

        foreach(var component in GameObject.GetComponents<Component>()) {
            Debug.LogWarning($"  D ATTACK HAS {component.GetType().Name}");
        }

        GameObject.SetActive(true);
        return GameObject!;
    }
}
