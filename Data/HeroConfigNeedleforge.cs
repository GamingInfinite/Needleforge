using HutongGames.PlayMaker;
using System.Reflection;
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
    /// Bulk setter for whether or not Hornet can use her movement abilities,
    /// needolin, and needle strike.
    /// </summary>
    public void SetCanUseAbilities(bool value)
    {
        canBrolly = canDoubleJump = canHarpoonDash = canPlayNeedolin = canNailCharge = value;
    }

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
    /// If <see cref="HeroControllerConfig.downSlashType"/> = <see cref="DownSlashTypes.Custom"/>,
    /// this function defines an FSM edit for Hornet's behaviour during down attacks on a custom crest.
    /// See Hornet's "crest_attacks" FSM.
    /// </summary>
    /// <inheritdoc cref="AttackFsmEdit" path="/remarks"/>
    public AttackFsmEdit? DownSlashFsmEdit { get; set; } = null;

	/// <summary>
	/// Defines an FSM edit for Hornet's behaviour during dash attacks on a custom crest.
	/// See Hornet's "Sprint" FSM.
	/// </summary>
	/// <inheritdoc cref="AttackFsmEdit" path="/remarks"/>
	public AttackFsmEdit? DashSlashFsmEdit { get; set; } = null;

	/// <summary>
	/// Defines an FSM edit for Hornet's behaviour during charged attacks on a custom crest.
	/// See Hornet's "Nail Arts" FSM.
	/// </summary>
	/// <inheritdoc cref="AttackFsmEdit" path="/remarks"/>
	public AttackFsmEdit? ChargedSlashFsmEdit { get; set; } = null;

    /// <summary>
    /// Defines an FSM edit for Hornet's behaviour during an FSM-controlled attack
    /// on a custom crest.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Make any FSM edits you need to off of <c>anticState</c>, then set
    /// <c>endState</c> to the end state of an attack that missed and
    /// <c>hitReactionState</c> to the end state of an attack that hit something.
    /// </para><para>
    /// Needleforge adds the necessary transitions between those states and the rest of the FSM.
    /// </para>
    /// </remarks>
    public delegate void AttackFsmEdit(
        PlayMakerFSM fsm, FsmState anticState,
        out FsmState endState, out FsmState hitReactionState
    );

	/// <summary>
	/// Copies all fields of the supplied config object to a new <see cref="HeroConfigNeedleforge"/> object.
	/// </summary>
	public static HeroConfigNeedleforge Copy(HeroControllerConfig hcc)
    {
        var clone = ScriptableObject.CreateInstance<HeroConfigNeedleforge>();

        var fields = typeof(HeroControllerConfig)
            .GetAllFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var f in fields)
        {
            if (f.Name.StartsWith("m_"))
                continue;
            f.SetValue(clone, f.GetValue(hcc));
        }

        return clone;
    }

}
