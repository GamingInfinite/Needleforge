using System.Collections.Generic;
using UnityEngine;

namespace Needleforge.Data;

/// <summary>
/// Represents a custom tool color.
/// </summary>
public class ColorData
{
    /// <summary>The name of the color.</summary>
    public string name = "";

    /// <summary>
    /// Whether or not this color is considered an attacking type,
    /// like the red and skill types.
    /// </summary>
    public bool isAttackType = false;

    /// <summary>The visual color slots of this type will use in the inventory.</summary>
    public Color color;

    /// <summary>An icon for slots of this color on the crest UI.</summary>
    public Sprite? slotIcon;

    /// <summary>The section header for tools of this color in the tool pane.</summary>
    public Sprite? header;

    /// <summary>The custom <see cref="ToolItemType"/> value this color uses.</summary>
    public ToolItemType Type
    {
        get
        {
            int index = NeedleforgePlugin.newColors.IndexOf(this);
            return (ToolItemType)(index + 4);
        }
    }

    private readonly List<ToolItemType> _extraValidTypes = [];

    /// <summary>
    /// If true, tools of this color are equippable into <i>any</i> slot color,
    /// and slots of this color can accept <i>any</i> color of tool.
    /// </summary>
    public bool allColorsValid = false;

    /// <summary>
    /// Allows slots of this color to accept tools of the given type,
    /// and tools of this color to be equippable into slots of the given type.
    /// </summary>
    public void AddValidType(ToolItemType type)
    {
        _extraValidTypes.Add(type);
    }

    /// <summary>
    /// Allows slots of this color to accept tools of all of the given types,
    /// and tools of this color to be equippable into slots of all of the given types.
    /// </summary>
    public void AddValidTypes(params ToolItemType[] types)
    {
        _extraValidTypes.AddRange(types);
    }

    /// <summary>
    /// The list of tool/slot colors which are compatible with tools/slots of this color.
    /// </summary>
    public List<ToolItemType> ValidTypes
    {
        get
        {
            return [Type, .. _extraValidTypes];
        }
    }
}