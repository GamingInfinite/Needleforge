using HutongGames.PlayMaker;
using System.Linq;
using System.Reflection;
using UnityEngine;
using DownSlashTypes = HeroControllerConfig.DownSlashTypes;

namespace Needleforge.Data;

/// <summary>
/// An extension of <see cref="HeroControllerConfig"/> which has bulk setters for related
/// groups of fields, allows specifying FSM edits for specific attacks, and adds some
/// extra options.
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
    /// Whether or not Hornet lifts off the ground at the beginning of a charged attack.
    /// </summary>
    public bool ChargedSlashDoesKickoff { get; set; } = false;

    /// <summary>
    /// Defines an FSM edit for Hornet's behaviour during charged attacks.
    /// See Hornet's "Nail Arts" FSM.
    /// </summary>
    /// <remarks>
    /// <para><inheritdoc cref="FsmEdit" path="//remarks"/></para>
    /// <para>
    /// A charged attack's GameObject should always be disabled when not attacking.
    /// If your attack is not using any of ChargedAttack's automatic disable options,
    /// you must deactivate it at the end of your FSM edit.
    /// </para>
    /// </remarks>
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

    #region Bulk Setters
    // These are partly convenient ways to set a bunch of related properties at once,
    // and partly an answer to the question of how to document properties of a class with
    // no docstrings that's critical for working with crest movesets.

    /// <summary>
    /// Bulk setter for whether or not Hornet can use her movement abilities,
    /// needolin, and needle strike.
    /// </summary>
    public void SetCanUseAbilities(bool value)
    {
        canBrolly = canDoubleJump = canHarpoonDash = canPlayNeedolin = canNailCharge = value;
    }

    /// <summary>
    /// Sets fields affecting the behaviour of regular attacks, including down attacks
    /// when downSlashType = <see cref="DownSlashTypes.Slash"/>.
    /// Any parameter left unset will have no effect on its field's value.
    /// </summary>
    /// <param name="time">Duration of an attack in seconds.</param>
    /// <param name="recovery">
    ///     Duration in seconds before the player can take other actions after attacking.
    /// </param>
    /// <param name="cooldown">Minimum duration in seconds between attack inputs.</param>
    /// <param name="quickSpeedMult">
    ///     A multiplier on the time and animation speed of attacks when they're sped up,
    ///     e.x. when Flea Brew is active.
    /// </param>
    /// <param name="quickCooldown">
    ///     Minimum duration in seconds between attack inputs when attacks are sped up,
    ///     e.x. when Flea Brew is active.
    /// </param>
    public void SetAttackFields(
        float? time = null, float? recovery = null, float? cooldown = null,
        float? quickSpeedMult = null, float? quickCooldown = null
    ) {
        if (time != null)
            attackDuration = (float)time;
        if (recovery != null)
            attackRecoveryTime = (float)recovery;
        if (cooldown != null)
            attackCooldownTime = (float)cooldown;

        if (quickSpeedMult != null)
            quickAttackSpeedMult = (float)quickSpeedMult;
        if (quickCooldown != null)
            quickAttackCooldownTime = (float)quickCooldown;
    }

    /// <summary>
    /// Sets fields affecting the behaviour of downspikes. These have no effect unless
    /// <see cref="HeroControllerConfig.downSlashType"/> = <see cref="DownSlashTypes.DownSpike"/>.
    /// Any parameter left unset will have no effect on its field's value.
    /// </summary>
    /// <param name="anticTime">Duration in seconds of Hornet's downspike anticipation.</param>
    /// <param name="time">Duration in seconds of the damaging portion of the downspike.</param>
    /// <param name="recoveryTime">
    ///     Duration in seconds before the player regains control after a downspike.
    /// </param>
    /// <param name="doesThrust">Whether or not Hornet moves during a downspike.</param>
    /// <param name="speed">
    ///     The speed at which Hornet moves (at a 45 degree angle) at the start of a
    ///     downspike. Overidden by <paramref name="velocity"/>.
    /// </param>
    /// <param name="velocity">
    ///     The starting angle and speed of Hornet's movement during a downspike.
    ///     Overrides <paramref name="speed"/>. Negative X values point in the direction
    ///     Hornet is facing.
    /// </param>
    /// <param name="acceleration">
    ///     An acceleration applied to Hornet's velocity for the duration of a downspike.
    ///     Negative X values point in the direction Hornet is facing.
    /// </param>
    /// <param name="doesBurstEffect">
    ///     Whether or not to play a small burst effect at the start of a downspike.
    /// </param>
    public void SetDownspikeFields(
        float? anticTime = null, float? time = null, float? recoveryTime = null,
        bool? doesThrust = null, float? speed = null, Vector2? velocity = null, Vector2? acceleration = null,
        bool? doesBurstEffect = null
    ) {
        if (anticTime != null)
            downspikeAnticTime = (float)anticTime;
        if (time != null)
            downspikeTime = (float)time;
        if (recoveryTime != null)
            downspikeRecoveryTime = (float)recoveryTime;

        if (doesThrust != null)
            downspikeThrusts = (bool)doesThrust;
        if (speed != null)
            downspikeSpeed = (float)speed;
        if (velocity != null)
            DownspikeVelocity = (Vector2)velocity;
        if (acceleration != null)
            DownspikeAcceleration = (Vector2)acceleration;

        if (doesBurstEffect != null)
            downspikeBurstEffect = (bool)doesBurstEffect;
    }

    /// <summary>
    /// Sets all fields needed to make a Custom-type downslash.
    /// </summary>
    /// <param name="eventName">
    ///     A name for an <see cref="FsmEvent"/> which will be used to start the custom
    ///     down slash. This must be a unique value for each crest.
    /// </param>
    /// <param name="fsmEdit">
    ///     Defines an FSM edit for Hornet's behaviour during down attacks.
    ///     See <see cref="DownSlashFsmEdit"/>.
    /// </param>
    public void SetCustomDownslash(string eventName, FsmEdit fsmEdit)
    {
        downSlashType = DownSlashTypes.Custom;
        downSlashEvent = eventName;
        DownSlashFsmEdit = fsmEdit;
    }

    /// <summary>
    /// Sets fields affecting the behaviour of the charged slash,
    /// provided <see cref="DashSlashFsmEdit"/> hasn't been set.
    /// Any parameter left unset will have no effect on its field's value.
    /// </summary>
    /// <remarks>
    /// Note that <see cref="HeroControllerConfig.dashStabSteps"/> has no effect on
    /// Needleforge crests. The length of the Steps array in <see cref="Attacks.DashAttack"/>
    /// is used instead.
    /// </remarks>
    /// <param name="time">Duration in seconds of each step of a dash attack.</param>
    /// <param name="speed">Lunging speed of each step of a dash attack.</param>
    /// <param name="bounceJumpSpeed">
    ///     The upward speed of the start of Hornet's bounce when a dash attack
    ///     hits an enemy.
    /// </param>
    /// <param name="forceShortBounce">
    ///     Forces the Hornet's bounce when a dash attack hits an enemy to be a standard
    ///     short bounce, regardless of the value of <paramref name="bounceJumpSpeed"/>.
    /// </param>
    public void SetDashStabFields(
        float? time = null, float? speed = null, float? bounceJumpSpeed = null,
        bool? forceShortBounce = null
    ) {
        if (time != null)
            dashStabTime = (float)time;
        if (speed != null)
            dashStabSpeed = (float)speed;
        if (bounceJumpSpeed != null)
            dashStabBounceJumpSpeed = (float)bounceJumpSpeed;
        if (forceShortBounce != null)
            forceShortDashStabBounce = (bool)forceShortBounce;
    }

    /// <summary>
    /// Sets fields affecting the behaviour of the charged slash,
    /// provided <see cref="ChargedSlashFsmEdit"/> hasn't been set.
    /// Any parameter left unset will have no effect on its field's value.
    /// </summary>
    /// <param name="lungeSpeed">
    ///     If &gt;= 0, the charged slash is considered a Lunge and will move Hornet
    ///     horizontally at the speed set. This prevents <paramref name="chain"/> and
    ///     <paramref name="recoils"/> from having any effect.
    /// </param>
    /// <param name="lungeDeceleration">
    ///     The rate of deceleration of Hornet's movement during a Lunging charged attack.
    /// </param>
    /// <param name="chain">
    ///     If &gt; 0, the charged slash is considered a Spin and can be extended by
    ///     mashing the attack button as many times as the value set. This is overridden
    ///     by <paramref name="lungeSpeed"/> and prevents <paramref name="recoils"/>
    ///     from having any effect.
    /// </param>
    /// <param name="recoils">
    ///     Whether or not Hornet is pushed back during regular-type charged attacks.
    /// </param>
    /// <param name="doesKickoff">
    ///     <inheritdoc cref="ChargedSlashDoesKickoff" path="//summary"/>
    /// </param>
    public void SetChargedSlashFields(
        int? chain = null, float? lungeSpeed = null, float? lungeDeceleration = null,
        bool? recoils = null, bool? doesKickoff = null
    ) {
        if (chain != null)
            chargeSlashChain = (int)chain;

        if (lungeSpeed != null)
            chargeSlashLungeSpeed = (float)lungeSpeed;
        if (lungeDeceleration != null)
            chargeSlashLungeDeceleration = (float)lungeDeceleration;

        if (recoils != null)
            chargeSlashRecoils = (bool)recoils;

        if (doesKickoff != null)
            ChargedSlashDoesKickoff = (bool)doesKickoff;
    }

    #endregion

}
