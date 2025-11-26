using UnityEngine;

namespace Needleforge.Data;

/// <summary>
/// Represents the visual, auditory, and damage properties of an attack in a crest moveset.
/// Changes to an attack's properties will update the <see cref="UnityEngine.GameObject"/>
/// it represents, if one has been created.
/// </summary>
public class Attack : AttackBase {

    #region API

    public override string AnimName {
        get => _animName;
        set {
            _animName = value;
            if (GameObject)
                nailSlash!.animName = value;
        }
    }
    private string _animName = "";

    public override Vector2 Scale {
        get => base.Scale;
        set {
            base.Scale = value;
            if (GameObject)
                nailSlash!.scale = value.MultiplyElements(_wallSlashFlipper);
        }
    }

    #endregion

    /// <summary>
    /// <para>
    /// Whether or not this attack is a wall slash. Setting this to <c>true</c> causes
    /// the attack's scale to flip on the X axis, so that the attack actually points in
    /// front of Hornet when she's wall-sliding.
    /// </para><para>
    /// When setting this attack on a <see cref="MovesetData.WallSlash"/> property,
    /// this will be set automatically.
    /// </para>
    /// </summary>
    internal bool IsWallSlash {
        get => _wallSlashFlipper.x < 0;
        set =>
            _wallSlashFlipper = Vector3.one with {
                x = value ? -1 : 1
            };
    }
    private Vector3 _wallSlashFlipper = Vector3.one;

    private NailSlash? nailSlash;
    protected override NailAttackBase? NailAttackBase => nailSlash;

    internal override GameObject CreateGameObject(GameObject parent, HeroController hc)
    {
        base.CreateGameObject(parent, hc);
        GameObject!.SetActive(false); // VERY IMPORTANT

        // Common component initialization

        nailSlash = GameObject!.AddComponent<NailSlash>();

        nailSlash.hc = hc;
        nailSlash.activateOnSlash = [];
        nailSlash.enemyDamager = damager;

        // Customizations

        nailSlash.scale = Scale.MultiplyElements(_wallSlashFlipper);
        nailSlash.animName = AnimName;
        nailSlash.AttackStarting += TintIfNotImbued;

        GameObject!.SetActive(true);
        return GameObject!;
    }

}
