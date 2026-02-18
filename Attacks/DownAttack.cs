using Needleforge.Components;
using Needleforge.Data;
using System;
using System.Collections;
using UnityEngine;
using DownSlashTypes = HeroControllerConfig.DownSlashTypes;
using UObject = UnityEngine.Object;

namespace Needleforge.Attacks;

/// <summary>
/// Represents the visual, auditory, and damage properties of a down attack in a crest
/// moveset.
/// Changes to an attack's properties will update the <see cref="GameObject"/>
/// it represents, if one has been created.
/// </summary>
/// <remarks>
/// The type and behaviour of down attacks are determined by properties of
/// <see cref="MovesetData.HeroConfig"/>, particularly
/// <see cref="HeroControllerConfig.downSlashType">downSlashType</see>, which must
/// be set <i>before</i> the moveset is initialized.
/// </remarks>
public class DownAttack : AttackBase
{
    /// <inheritdoc cref="DownAttack"/>
    public DownAttack() { }

    #region API

    /// <inheritdoc cref="AttackBase.AnimName"/>
    /// <remarks>
    /// <para>
    /// If the <see cref="HeroConfig"/> has the down slash type set to
    /// <see cref="DownSlashTypes.DownSpike"/>, this attack's effect animation must
    /// <b>not</b> have <b>any</b> frames which trigger an event.
    /// </para><para>
    /// If the <see cref="HeroConfig"/> has the down slash type set to
    /// <see cref="DownSlashTypes.Slash"/> or <see cref="DownSlashTypes.Custom"/>,
    /// this attack's effect animation must have <b>two</b> frames which trigger an event;
    /// these frames determine when the attack's hitbox is enabled and disabled.
    /// </para>
    /// </remarks>
    public override string AnimName
    {
        get => _animName;
        set {
            _animName = value;
            if (nailSlash) nailSlash.animName = value;
            if (downspike) downspike.animName = value;
        }
    }
    private string _animName = "";

    /// <summary>
    /// Controls the feel of the bounce when this down attack hits some obstacles/enemies.
    /// If <see cref="MovesetData.HeroConfig"/>'s down slash type is
    /// <see cref="DownSlashTypes.Custom"/>, this has no effect.
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

    /// <summary>
    /// A reference to the hero config of the <see cref="MovesetData"/> this attack is
    /// associated with. Used to decide which type of <see cref="NailAttackBase"/>-derived
    /// component to attack to the created GameObject.
    /// </summary>
    protected internal HeroControllerConfig? HeroConfig { get; internal set; }

    private HeroDownAttack? heroDownAttack;
    private DownspikeWithBounceConfig? downspike;
    private NailSlash? nailSlash;
    private PlayMakerFSM? reactionFsm;

    /// <inheritdoc/>
    protected override NailAttackBase? NailAttack =>
        heroDownAttack ? heroDownAttack.attack : null;

    /// <inheritdoc/>
    protected override void AddComponents(HeroController hc)
    {
        if (!HeroConfig)
        {
            throw new InvalidOperationException(
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

            case DownSlashTypes.Custom:
                reactionFsm = GameObject.AddComponent<PlayMakerFSM>();
                goto case DownSlashTypes.Slash;

            case DownSlashTypes.Slash:
                nailSlash = GameObject.AddComponent<NailSlash>();
                heroDownAttack.attack = nailSlash;
                break;
        }
    }

    /// <inheritdoc/>
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

            case DownSlashTypes.Custom:
                GameManager.instance.StartCoroutine(InitReactionFsm());
                goto case DownSlashTypes.Slash;

            case DownSlashTypes.Slash:
                nailSlash!.animName = AnimName;
                nailSlash!.bounceConfig = BounceConfig;
                Damager!.corpseDirection =
                    new TeamCherry.SharedUtils.OverrideFloat() {
                        IsEnabled = true,
                        Value = downAngle
                    };
                break;
        }

        IEnumerator InitReactionFsm() {
            yield return null; // wait one frame for components to Awake
            reactionFsm!.SetFsmTemplate(ReactionFsmTemplate);
        }
    }

    private static FsmTemplate ReactionFsmTemplate {
        get {
            if (!_reactionFsmTemplate) {
                var reaper = HeroController.instance.transform.Find("Attacks/Scythe/DownSlash New");
                var reaperFsm = reaper.GetComponent<PlayMakerFSM>();
                _reactionFsmTemplate = reaperFsm.fsmTemplate;
            }
            return _reactionFsmTemplate;
        }
    }
    private static FsmTemplate? _reactionFsmTemplate;

}
