using UnityEngine;
using DownSlashTypes = HeroControllerConfig.DownSlashTypes;

namespace Needleforge.Data;

/// <summary>
/// An extension of <see cref="HeroControllerConfig"/> which allows setting
/// start velocity and acceleration vectors for Hornet for downspike-type down attacks.
/// </summary>
public class HeroConfigNeedleforge : HeroControllerConfig
{
    /// <summary>
    /// If <see cref="HeroControllerConfig.downSlashType"/> = <see cref="DownSlashTypes.DownSpike"/>
    /// and <see cref="HeroControllerConfig.downspikeThrusts"/> = <c>true</c>,
    /// this vector is the starting speed and angle of Hornet's movement during the attack.
    /// Negative X values move in the direction Hornet is facing.
    /// If this is set, <see cref="HeroControllerConfig.DownspikeSpeed"/> will not be used.
    /// </summary>
    public Vector2 DownspikeVelocity
    {
        get => _velocity ?? new(-DownspikeSpeed, -DownspikeSpeed);
        set => _velocity = value;
    }
    private Vector2? _velocity;

    /// <summary>
    /// If <see cref="HeroControllerConfig.downSlashType"/> = <see cref="DownSlashTypes.DownSpike"/>
    /// and <see cref="HeroControllerConfig.downspikeThrusts"/> = <c>true</c>,
    /// this vector is an acceleration (in meters per second) applied
    /// to the <see cref="DownspikeVelocity"/> over the duration of the attack.
    /// Negative X values move in the direction Hornet is facing.
    /// </summary>
    public Vector2 DownspikeAcceleration { get; set; } = new(0, 0);

    /// <summary>
    /// Sets whether or not Hornet can use her movement abilities, needolin, and needle strike.
    /// </summary>
    public bool CanUseAbilities
    {
        set => canBrolly = canDoubleJump = canHarpoonDash = canPlayNeedolin = canNailCharge = value;
    }
}
