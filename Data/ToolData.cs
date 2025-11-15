using System;
using TeamCherry.Localization;
using UnityEngine;

namespace Needleforge.Data
{
    public class ToolData
    {
        public Sprite? inventorySprite;
        public ToolItemType type;
        public string name = "";
        public bool UnlockedAtStart = true;
        public LocalisedString displayName;
        public LocalisedString description;

        public ToolItem? Item
        {
            get
            {
                if (HeroController.instance != null)
                {
                    foreach (var tool in NeedleforgePlugin.newTools)
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
                return Item && Item.IsEquipped;
            }
        }
        public string unlockedPDString
        {
            get
            {
                return $"is{name}Unlocked";
            }
        }
    }

    public class LiquidToolData : ToolData
    {
        public int maxRefills = 20;
        public int storageAmount = 6;
        public string infiniteRefills = "";
        public Color color = Color.white;
        public ToolItem.ReplenishResources resource = ToolItem.ReplenishResources.Shard;
        public ToolItem.ReplenishUsages replenishUsage = ToolItem.ReplenishUsages.Percentage;
        public float replenishMult = 1f;

        public string clip = "";

        new public ToolItemStatesLiquid? Item
        {
            get
            {
                if (HeroController.instance != null)
                {
                    foreach (var tool in NeedleforgePlugin.newTools)
                    {
                        if (tool.name == name)
                        {
                            return (ToolItemStatesLiquid)tool;
                        }
                    }
                }
                return null;
            }
        }

        public StateSprites? FullSprites;
        public StateSprites? EmptySprites;

        public Action beforeAnim
        {
            get
            {
                return NeedleforgePlugin.toolEventHooks[$"{name} BEFORE ANIM"];
            }
            set
            {
                NeedleforgePlugin.toolEventHooks[$"{name} BEFORE ANIM"] = value;
            }
        }
        public Action afterAnim
        {
            get
            {
                return NeedleforgePlugin.toolEventHooks[$"{name} AFTER ANIM"];
            }
            set
            {
                NeedleforgePlugin.toolEventHooks[$"{name} AFTER ANIM"] = value;
            }
        }
    }

    public class StateSprites
    {
        public Sprite? InventorySprite;
        public Sprite? HudSprite;
        public Sprite? PoisonInventorySprite;
        public Sprite? PoisonHudSprite;
    }
}
