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

    private HeroControllerConfig? heroConf;
    public HeroControllerConfig? HeroConfig
    {
        get => heroConf;
        set
        {
            if (value) value.name = Crest.name;

            heroConf = value;
            if (Crest.ToolCrest) Crest.ToolCrest.heroConfig = value;
            if (ConfGroup != null) ConfGroup.Config = value;
        }
    }

    public Attack? Slash { get; set; }
    public Attack? SlashAlt { get; set; }
	public Attack? WallSlash { get; set; }
    public Attack? UpSlash { get; set; }

    // TODO down slash
    // TODO dash slash
    // TODO charged slash

    public ConfigGroup? ConfGroup;

    public event Action OnConfigGroupCreated;

    internal void ConfigGroupCreated() => OnConfigGroupCreated?.DynamicInvoke();

    // TODO OnInitialized event that gives you access to the created attack gameobjects so you can do further customizations that we didn't provide an API for


}
