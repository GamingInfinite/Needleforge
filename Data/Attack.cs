using UnityEngine;

namespace Needleforge.Data;

/// <summary>
/// Represents the visual, auditory, and damage properties of a standard attack in a
/// crest moveset.
/// Changes to an attack's properties will update the <see cref="GameObject"/>
/// it represents, if one has been created.
/// </summary>
public class Attack : AttackBase
{

    #region API

    public override string AnimName
    {
        get => _animName;
        set
        {
            _animName = value;
            if (GameObject)
                nailSlash!.animName = value;
        }
    }
    private string _animName = "";

    public override Vector2 Scale
    {
        get => base.Scale;
        set
        {
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
        set
        {
            _wallSlashFlipper = Vector3.one with {
                x = value ? -1 : 1
            };
            if (GameObject)
                nailSlash!.scale = Scale.MultiplyElements(_wallSlashFlipper);
        }
    }
    private Vector3 _wallSlashFlipper = Vector3.one;

    protected NailSlash? nailSlash;
    protected override NailAttackBase? NailAttack => nailSlash;

    protected override void AddComponents(HeroController hc)
    {
        nailSlash = GameObject!.AddComponent<NailSlash>();
        nailSlash.animName = AnimName;
    }

    protected override void LateInitializeComponents(HeroController hc)
    {
        nailSlash!.scale = Scale.MultiplyElements(_wallSlashFlipper);
    }

}
