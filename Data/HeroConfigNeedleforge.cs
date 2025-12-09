using HutongGames.PlayMaker;
using System.Linq;
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
    /// this function defines an FSM edit for Hornet's behaviour during down attacks.
    /// See Hornet's "crest_attacks" FSM.
    /// </summary>
    /// <inheritdoc cref="FsmEdit" path="/remarks"/>
    public FsmEdit? DownSlashFsmEdit { get; set; } = null;

    /// <summary>
    /// Defines an FSM edit for Hornet's behaviour during dash attacks.
    /// See Hornet's "Sprint" FSM.
    /// </summary>
    /// <inheritdoc cref="FsmEdit" path="/remarks"/>
    public FsmEdit? DashSlashFsmEdit { get; set; } = null;

    /// <summary>
    /// Defines an FSM edit for Hornet's behaviour during charged attacks.
    /// See Hornet's "Nail Arts" FSM.
    /// </summary>
    /// <inheritdoc cref="FsmEdit" path="/remarks"/>
    public FsmEdit? ChargedSlashFsmEdit { get; set; } = null;

    /// <summary>
    /// Defines a new control path for Hornet's behaviour during an FSM-controlled attack.
    /// </summary>
    /// <remarks>
    /// Make any FSM edits you need to off of the provided <c>startState</c>, which
    /// already has the necessary incoming transitions.
    /// All states added to the <c>endStates</c> array will be provided with an outgoing
    /// transition that allows normal player control to resume.
    /// </remarks>
    public delegate void FsmEdit(
        PlayMakerFSM fsm, FsmState startState, out FsmState[] endStates
    );

    /// <summary>
    /// Copies all fields of the supplied config object to a new <see cref="HeroConfigNeedleforge"/> object.
    /// </summary>
    public static HeroConfigNeedleforge Copy(HeroControllerConfig source)
    {
        var clone = CreateInstance<HeroConfigNeedleforge>();
        var hcnType = typeof(HeroConfigNeedleforge);

        var sourceFields = typeof(HeroControllerConfig)
            .GetAllFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        var cloneFields = typeof(HeroConfigNeedleforge)
            .GetAllFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var f in sourceFields)
        {
            if (f.Name.StartsWith("m_") || !cloneFields.Any(x => x.Equals(f)))
                continue;
            f.SetValue(clone, f.GetValue(source));
        }

        return clone;
    }

}
