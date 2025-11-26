using UnityEngine;
using DownSlashTypes = HeroControllerConfig.DownSlashTypes;

namespace Needleforge.Data;

public class DownAttack : AttackBase
{
    private static readonly HeroSlashBounceConfig defaultBounceConfig;

    static DownAttack() {
        defaultBounceConfig = ScriptableObject.CreateInstance<HeroSlashBounceConfig>();
        defaultBounceConfig.jumpedSteps = -20;
        defaultBounceConfig.jumpSteps = 8;
        defaultBounceConfig.hideSlashOnBounceCancel = false;
    }

    #region API

    /// <summary>
    /// <para>
    /// A reference to the library where this attack's effect animation is found.
    /// <inheritdoc cref="AttackBase.Name" path="//*[@id='prop-updates-go']"/>
    /// </para>
    /// <div id="anim-info">
    /// <para>
    /// The effect animation for a down attack should not loop.
    /// </para><para>
    /// If the moveset for this attack has set <see cref="DownSlashTypes.DownSpike"/> in
    /// the <see cref="MovesetData.HeroConfig"/>, <b>all</b> of this animation's frames
    /// must have <see cref="tk2dSpriteAnimationFrame.triggerEvent"/> = <b><c>false</c></b>.
    /// </para><para>
    /// If the moveset for this attack has set <see cref="DownSlashTypes.Slash"/>,
    /// this animation must have <b>two</b> frames for which
    /// <see cref="tk2dSpriteAnimationFrame.triggerEvent"/> = <c>true</c>;
    /// these frames determine when the attack's hitbox is enabled and disabled.
    /// </para>
    /// </div>
    /// </summary>
    public new tk2dSpriteAnimation? AnimLibrary
    {
        get => base.AnimLibrary;
        set => base.AnimLibrary = value;
    }

    /// <summary>
    /// <para>
    /// The name of the animation clip to use for this attack's effect.
    /// <inheritdoc cref="AttackBase.Name" path="//*[@id='prop-updates-go']"/>
    /// </para>
    /// <inheritdoc cref="AnimLibrary" path="//*[@id='anim-info']"/>
    /// </summary>
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

    /// <summary>
    /// Controls the feel of the bounce when this down attack hits some obstacles/enemies.
    /// This is only used if <see cref="MovesetData.HeroConfig"/>'s down slash type is
    /// set to <see cref="DownSlashTypes.Slash"/>.
    /// </summary>
    // TODO: figure out and document what the properties of this object do.
    public HeroSlashBounceConfig BounceConfig
    {
        get => _bounceConfig;
        set {
            _bounceConfig = value;
            if (nailSlash)
                nailSlash.bounceConfig = value;
        }
    }
    private HeroSlashBounceConfig _bounceConfig = Object.Instantiate(defaultBounceConfig);

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
        base.CreateGameObject(parent, hc);
        GameObject!.SetActive(false); // VERY IMPORTANT

        heroDownAttack = GameObject.AddComponent<HeroDownAttack>();
        heroDownAttack.hc = hc;

        if (!HeroConfig)
            throw new System.Exception($"{nameof(HeroConfig)} must be set for a down attack to know what kind of down attack it is.");

        switch (HeroConfig.downSlashType) {
            case DownSlashTypes.DownSpike:
                // Common component initialization

                downspike = GameObject.AddComponent<Downspike>();
                heroDownAttack.attack = downspike;

                downspike.hc = hc;
                downspike.activateOnSlash = [];
                downspike.enemyDamager = damager;
                downspike.heroBox = hc.heroBox;
                downspike.horizontalKnockbackDamager = hc.transform.Find("Attacks/Downspike Knockback Top").GetComponent<DamageEnemies>();
                downspike.verticalKnockbackDamager = hc.transform.Find("Attacks/Downspike Knockback Bottom").GetComponent<DamageEnemies>();
                downspike.leftExtraDirection = 135;
                downspike.rightExtraDirection = 45;

                damager!.manualTrigger = true;
                damager!.forceSpikeUpdate = true;

                // Customizations
                downspike.animName = AnimName;

                break;
            case DownSlashTypes.Slash:
                // Common component initialization

                nailSlash = GameObject.AddComponent<NailSlash>();
                heroDownAttack.attack = nailSlash;

                nailSlash.hc = hc;
                nailSlash.activateOnSlash = [];
                nailSlash.enemyDamager = damager;

                damager!.corpseDirection =
                    new TeamCherry.SharedUtils.OverrideFloat()
                    {
                        IsEnabled = true,
                        Value = DirectionUtils.GetAngle(DirectionUtils.Down)
                    };

                // Customizations
                nailSlash.animName = AnimName;
                nailSlash.bounceConfig = BounceConfig;

                break;
            default:
                throw new System.NotImplementedException();
        }

        NailAttackBase!.scale = Scale;
        NailAttackBase!.AttackStarting += TintIfNotImbued;

        damager!.direction = DirectionUtils.GetAngle(DirectionUtils.Down);

        GameObject.SetActive(true);
        return GameObject!;
    }
}
