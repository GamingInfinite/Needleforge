using System;
using TeamCherry.Localization;
using UnityEngine;

namespace Needleforge.Data
{
    /// <summary>
    /// Represents a Custom Basic Tool.  Provides customization to its Type and Sprite
    /// and provides properties to read important values from its in-game counterpart (ToolItem)
    /// Can be created with <see cref="NeedleforgePlugin.AddTool(string)"/> and its overloads.
    /// </summary>
    public class ToolData
    {
        /// <summary>
        /// Main Sprite of the Tool in Inventory UI
        /// </summary>
        public Sprite? inventorySprite;
        
        /// <summary>
        /// Type of Tool (eg. Red, Yellow, etc.)
        /// </summary>
        public ToolItemType type;
        
        /// <summary>
        /// The name
        /// </summary>
        public string name = "";
        
        /// <summary>
        /// Dictates if the tool is unlocked from the start of the game.
        /// </summary>
        public bool UnlockedAtStart = true;
        
        /// <summary>
        /// In-Game Localized Name (as created via i18n)
        /// </summary>
        public LocalisedString displayName;
        
        /// <summary>
        /// In-Game Localized Descritpion (as created via i18n)
        /// </summary>
        public LocalisedString description;

        /// <summary>
        /// ToolItem representation of the ToolData.
        /// </summary>
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

        /// <summary>
        /// Whether your tool is Equipped
        /// </summary>
        public bool IsEquipped
        {
            get { return Item && Item.IsEquipped; }
        }

        /// <summary>
        /// PlayerData string meant for PrePatcher.  Meant for internal use only.
        /// </summary>
        public string unlockedPDString
        {
            get { return $"is{name}Unlocked"; }
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
            get { return NeedleforgePlugin.toolEventHooks[$"{name} BEFORE ANIM"]; }
            set { NeedleforgePlugin.toolEventHooks[$"{name} BEFORE ANIM"] = value; }
        }

        public Action afterAnim
        {
            get { return NeedleforgePlugin.toolEventHooks[$"{name} AFTER ANIM"]; }
            set { NeedleforgePlugin.toolEventHooks[$"{name} AFTER ANIM"] = value; }
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