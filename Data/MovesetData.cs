using System;
using ConfigGroup = HeroController.ConfigGroup;

namespace Needleforge.Data;

/*

TODO:

- Multihits on standard attacks - see architect
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

    public Attack? Slash { get; set; }
    public Attack? SlashAlt { get; set; }
    public Attack? UpSlash { get; set; }

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
