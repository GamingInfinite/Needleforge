using HutongGames.PlayMaker;
using Needleforge.Components;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using DownSlashTypes = HeroControllerConfig.DownSlashTypes;
using UObject = UnityEngine.Object;

namespace Needleforge.Data;

/// <summary>
/// <para>
/// Represents the visual, auditory, and damage properties of a down attack in a crest
/// moveset.
/// Changes to an attack's properties will update the <see cref="GameObject"/>
/// it represents, if one has been created.
/// </para><para>
/// The type and behaviour of down attacks are determined by properties of
/// <see cref="MovesetData.HeroConfig"/>, particularly
/// <see cref="HeroControllerConfig.downSlashType">downSlashType</see>, which must
/// be set <i>before</i> the moveset is initialized.
/// </para>
/// </summary>
public class DownAttack : AttackBase, IAttackWithOwnEffectAnim, IAttackWithHeroConfigAccess
{
    #region API

    /// <summary>
    /// A reference to the library where this attack's effect animation is found.
    /// <inheritdoc cref="AttackBase.Name" path="//*[@id='prop-updates-go']"/>
    /// </summary>
    /// <remarks>
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
    /// </remarks>
    public new tk2dSpriteAnimation? AnimLibrary
    {
        get => base.AnimLibrary;
        set => base.AnimLibrary = value;
    }

    /// <summary>
    /// The name of the animation clip to use for this attack's effect.
    /// <inheritdoc cref="AttackBase.Name" path="//*[@id='prop-updates-go']"/>
    /// </summary>
    /// <inheritdoc cref="AnimLibrary" path="/remarks"/>
    public string AnimName
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
            if (downspike)
                downspike.bounceConfig = value;
        }
    }
    private HeroSlashBounceConfig _bounceConfig = UObject.Instantiate(HeroSlashBounceConfig.Default);

    #endregion

    public HeroControllerConfig? HeroConfig { get; set; }

    private HeroDownAttack? heroDownAttack;
    private DownspikeWithBounceConfig? downspike;
    private NailSlash? nailSlash;

    protected override NailAttackBase? NailAttack =>
        heroDownAttack ? heroDownAttack.attack : null;

    protected override void AddComponents(HeroController hc)
    {
        if (!HeroConfig)
        {
            throw new System.InvalidOperationException(
                $"{nameof(HeroConfig)} must be set for a down attack to " +
                 "know what kind of down attack it is."
            );
        }

        heroDownAttack = GameObject!.AddComponent<HeroDownAttack>();
        heroDownAttack.hc = hc;

        switch (HeroConfig.downSlashType) {
            case DownSlashTypes.DownSpike:
                downspike = GameObject.AddComponent<DownspikeWithBounceConfig>();
                heroDownAttack.attack = downspike;
                break;
            case DownSlashTypes.Slash:
            case DownSlashTypes.Custom:
                nailSlash = GameObject.AddComponent<NailSlash>();
                heroDownAttack.attack = nailSlash;
                break;
        }
    }

    protected override void LateInitializeComponents(HeroController hc)
    {
        float downAngle = DirectionUtils.GetAngle(DirectionUtils.Down);

        Damager!.direction = downAngle;

        switch (HeroConfig!.downSlashType) {
            case DownSlashTypes.DownSpike:
                downspike!.animName = AnimName;
                downspike!.heroBox = hc.heroBox;
                downspike!.horizontalKnockbackDamager =
                    hc.transform.Find($"Attacks/Downspike Knockback Top").GetComponent<DamageEnemies>();
                downspike!.verticalKnockbackDamager =
                    hc.transform.Find($"Attacks/Downspike Knockback Bottom").GetComponent<DamageEnemies>();
                downspike!.leftExtraDirection = 135;
                downspike!.rightExtraDirection = 45;

                downspike!.bounceConfig = BounceConfig;

                Damager!.manualTrigger = true;
                Damager!.forceSpikeUpdate = true;
                break;

            case DownSlashTypes.Slash:
            case DownSlashTypes.Custom:
                nailSlash!.animName = AnimName;
                nailSlash!.bounceConfig = BounceConfig;
                Damager!.corpseDirection =
                    new TeamCherry.SharedUtils.OverrideFloat() {
                        IsEnabled = true,
                        Value = downAngle
                    };
                break;
        }
    }

}
