using System.Collections.Generic;
using UnityEngine;

namespace Needleforge.Data;

public class ColorData
{
    public string name = "";
    public Color color;

    public ToolItemType type;
    public List<ToolItemType> acceptableTypes = [];
}