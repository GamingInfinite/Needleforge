using System.Collections.Generic;
using UnityEngine;

namespace Needleforge.Data;

public class ColorData
{
    public string name = "";
    public bool isAttackType = false;
    public Color color;
    
    public Sprite slotIcon;
    public Sprite header;

    public ToolItemType type;
    private List<ToolItemType> _extraValidTypes = [];
    public bool allColorsValid = false;

    public void AddValidType(ToolItemType type)
    {
        _extraValidTypes.Add(type);
    }

    public List<ToolItemType> ValidTypes
    {
        get
        {
            List<ToolItemType> validTypes = [type, .._extraValidTypes];
            return validTypes;
        }
    }
}