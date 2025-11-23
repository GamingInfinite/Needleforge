using System;
using ConfigGroup = HeroController.ConfigGroup;

namespace Needleforge.Data;

/*

TODO:

- Special handling for DownSlash, DashSlash, and ChargedSlash - possibly different classes
- FSM edits...
- Make sure everything is thoroughly documented

*/

public class MovesetData {

    internal readonly CrestData Crest;

    internal MovesetData(CrestData owner)
    {
        Crest = owner;
    }

    /// <summary>
    /// Defines how Hornet behaves when this crest is equipped. Properties of this object
    /// control speed and recovery time for attacks, which of Hornet's animations are
    /// overridden for this crest, which movement abilities she can use, whether or not
    /// she can access her inventory or tools, some of the behaviour of charged and down
    /// slashes, and more.
    /// </summary>
    public HeroControllerConfig? HeroConfig
    {
        get => _heroConf;
        set
        {
            if (value) value.name = Crest.name;

            _heroConf = value;
            if (Crest.ToolCrest) Crest.ToolCrest.heroConfig = value;
            if (ConfGroup != null) ConfGroup.Config = value;
        }
    }
    private HeroControllerConfig? _heroConf;

    /// <summary>
    /// Defines the visual, auditory, and damage properties of the default side attack.
    /// </summary>
    public Attack? Slash { get; set; }

    /// <summary>
    /// Defines the visual, auditory, and damage properties of the alternate side attack,
    /// which is used when the player attacks multiple times in quick succession.
    /// </summary>
    public Attack? SlashAlt { get; set; }

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

    // TODO down slash
    // TODO dash slash
    // TODO charged slash

    public ConfigGroup? ConfGroup;

    /// <summary>
    /// <para>
    /// Runs immediately after the <see cref="UnityEngine.GameObject"/>s for
    /// this moveset's attacks are created and set up. Any additional modifications or
    /// set up for attacks or <see cref="HeroConfig"/> which couldn't be done without
    /// GameObject access can be done in this event.
    /// </para><para>
    /// It's recommended to use the properties of each of this moveset's
    /// <see cref="Attack"/>s to make modifications to them. For finer control which may
    /// require more knowledge of the underlying structure of an attack, each
    /// <see cref="Attack.GameObject"/> can be modified directly.
    /// </para>
    /// </summary>
    public event Action? OnInitialized;

    /// <inheritdoc cref="OnInitialized"/>
    internal void ExtraInitialization() => OnInitialized?.DynamicInvoke();

}
