using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Needleforge.Data
{
    public class ToolData
    {
        public Sprite? inventorySprite;
        public ToolItemType type;
        public string name = "";
        
        public ToolItem? Item
        {
            get
            {
                if (HeroController.instance != null)
                {
                    foreach(var tool in NeedleforgePlugin.newTools)
                    {
                        if (tool.name == name)
                        {
                            return tool;
                        }
                    }
                }
                return null;
            }
        }

        public bool IsEquipped
        {
            get
            {
                if (Item != null)
                {
                    return Item.IsEquipped;
                }
                return false;
            }
        }
    }
}
