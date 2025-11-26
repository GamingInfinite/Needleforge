using System;
using UnityEngine;
using ConfigGroup = HeroController.ConfigGroup;

namespace Needleforge.Data;

/*

TODO:

- Special handling for DownSlash, DashSlash, and ChargedSlash - possibly different classes
- FSM edits...
- Make sure everything is thoroughly documented
- idea: custom down attack type that has some configuration options (antic time, attack duration/velocity/gravity, etc) that allows making things similar to shaman/beast/reaper downslash without dealing in fsms? maybe?

*/

/// <summary>
/// Represents a moveset for a custom crest.
/// </summary>
public class MovesetData {

    internal readonly CrestData Crest;

    internal MovesetData(CrestData owner)
    {
        Crest = owner;
    }

    /// <summary>
    /// <para>
    /// Defines how Hornet behaves when this crest is equipped. Properties of this object
    /// control speed and recovery time for attacks, which of Hornet's animations are
    /// overridden for this crest, which movement abilities she can use, whether or not
    /// she can access her inventory or tools, some of the behaviour of charged and down
    /// slashes, and more.
    /// </para><para>
    /// If left null, then after a save file is loaded this will be set to a copy of
    /// Hunter crest's configuration.
    /// You can modify this default config in <see cref="OnInitialized"/> or
    /// any time during gameplay.
    /// </para>
    /// </summary>
    public HeroControllerConfig? HeroConfig
    {
        get => _heroConf;
        set
        {
            if (value) value.name = Crest.name;

            _heroConf = value;
            if (Crest.ToolCrest)
                Crest.ToolCrest.heroConfig = value;
            if (ConfGroup != null)
                ConfGroup.Config = value;
            if (DownSlash != null)
                DownSlash.HeroConfig = value;
            if (AltDownSlash != null)
                AltDownSlash.HeroConfig = value;
        }
    }
    private HeroControllerConfig? _heroConf;

    /// <summary>
    /// Defines the visual, auditory, and damage properties of the side attack.
    /// </summary>
    public Attack? Slash { get; set; }

    /// <summary>
    /// Defines the visual, auditory, and damage properties of the up attack.
    /// </summary>
    public Attack? UpSlash { get; set; }

    /// <summary>
    /// Defines the visual, auditory, and damage properties of the wall-sliding attack.
    /// </summary>
    public Attack? WallSlash
    {
        get => _wallSlash;
        set
        {
            if (value != null)
                value.IsWallSlash = true;
            _wallSlash = value;
        }
    }
    private Attack? _wallSlash;

    /// <summary>
    /// <para>
    /// Defines the visual, auditory, and damage properties of the down attack.
    /// </para><para>
    /// The type and behaviour of down attacks are determined by properties of
    /// <see cref="HeroConfig"/>, particularly
    /// <see cref="HeroControllerConfig.downSlashType">downSlashType</see>, which must
    /// be set <i>before</i> the moveset is initialized.
    /// </para>
    /// </summary>
    public DownAttack? DownSlash
    {
        get => _downSlash;
        set
        {
            if (value != null)
                value.HeroConfig = HeroConfig;
            _downSlash = value;
        }
    }
    private DownAttack? _downSlash;

    /// <summary>
    /// Defines the visual, auditory, and damage properties of the alternate side attack,
    /// which is used when the player attacks multiple times in quick succession.
    /// Optional.
    /// </summary>
    public Attack? AltSlash { get; set; }

    /// <summary>
    /// Defines the visual, auditory, and damage properties of the alternate up attack,
    /// which is used when the player attacks multiple times in quick succession.
    /// Optional.
    /// </summary>
    public Attack? AltUpSlash { get; set; }

    /// <summary>
    /// <para>
    /// Defines the visual, auditory, and damage properties of the alternate down attack,
    /// which is used when the player attacks multiple times in quick succession.
    /// Optional.
    /// </para><para>
    /// The type and behaviour of down attacks are determined by properties of
    /// <see cref="HeroConfig"/>, particularly
    /// <see cref="HeroControllerConfig.downSlashType">downSlashType</see>, which must
    /// be set <i>before</i> the moveset is initialized.
    /// </para>
    /// </summary>
    public DownAttack? AltDownSlash
    {
        get => _altDownSlash;
        set
        {
            if (value != null)
                value.HeroConfig = HeroConfig;
            _altDownSlash = value;
        }
    }
    private DownAttack? _altDownSlash;

    // TODO down slash
    // TODO dash slash
    // TODO charged slash

    /// <summary>
    /// <para>
    /// Container for this crest's <see cref="HeroConfig"/> and attacks which is
    /// referenced by the <see cref="HeroController"/>.
    /// This will only contain a value after a save has been loaded;
    /// it can be accessed in <see cref="OnInitialized"/>.
    /// </para><para>
    /// Modifying already-set properties of this object can update <see cref="HeroConfig"/>
    /// but will <i>not</i> update any <see cref="Attack"/>s this moveset defines.
    /// Setting properties of this object manually should only be done if
    /// the <see cref="Attack"/> properties of this moveset are undefined.
    /// </para>
    /// </summary>
    public ConfigGroup? ConfGroup { get; internal set; }

    /// <summary>
    /// <para>
    /// Runs immediately after the <see cref="GameObject"/>s for
    /// this moveset's attacks are created and set up. Any additional modifications or
    /// set up for attacks or <see cref="HeroConfig"/> which couldn't be done without
    /// GameObject access can be done in this event.
    /// </para><para>
    /// It's recommended to use the properties of each of this moveset's
    /// <see cref="Attack"/>s to make modifications to them. For finer control which may
    /// require more knowledge of the underlying structure of an attack, each
    /// <see cref="Attack.GameObject"/> can be modified directly.
    /// </para><para>
    /// If no custom <see cref="Attack"/> was defined for any of the minimum set of
    /// attacks needed for crests to function, their <see cref="GameObject"/>s will
    /// be accessible through the properties of <see cref="ConfGroup"/>.
    /// </para>
    /// </summary>
    public event Action? OnInitialized;

    /// <inheritdoc cref="OnInitialized"/>
    internal void ExtraInitialization() => OnInitialized?.DynamicInvoke();

}
