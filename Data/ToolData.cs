using System;
using TeamCherry.Localization;
using UnityEngine;

namespace Needleforge.Data
{
    /// <summary>
    /// Represents a Custom Basic Tool. Provides customization to its Type and Sprite
    /// and provides properties to read important values from its in-game counterpart (ToolItem).
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
        /// The internal name of the tool.
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
        /// A reference to the object created to handle the tool's appearance and save data.
        /// This will always return a value during gameplay, but may be null during game
        /// start up, and is destroyed and recreated when the player quits to the menu.
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
        /// Whether or not this tool is currently equipped.
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

    /// <summary>
    /// Represents a custom liquid tool. Provides customization to its Type and Sprite
    /// and provides properties to read important values from its in-game counterpart (ToolItemStatesLiquid).
    /// Can be created with <see cref="NeedleforgePlugin.AddLiquidTool(string, int, int, Color)"/>
    /// and its overloads.
    /// </summary>
    public class LiquidToolData : ToolData
    {
        /// <summary>
        /// Maximum amount of refills kept in the inventory for this tool.
        /// </summary>
        public int maxRefills = 20;

        /// <summary>
        /// Maximum uses of the tool Hornet can carry at once.
        /// </summary>
        public int storageAmount = 6;

        /// <summary>
        /// Name of the PlayerData bool which determines if this tool has infinite refills.
        /// </summary>
        public string infiniteRefills = "";

        /// <summary>
        /// Color of the tool's refill liquid sprite in the inventory.
        /// </summary>
        public Color color = Color.white;

        /// <summary>
        /// Type of resource used to replenish uses of the tool.
        /// </summary>
        public ToolItem.ReplenishResources resource = ToolItem.ReplenishResources.Shard;

        /// <summary>
        /// Determines how the amount of <see cref="resource"/> it costs to
        /// replenish uses of the tool is calculated.
        /// </summary>
        public ToolItem.ReplenishUsages replenishUsage = ToolItem.ReplenishUsages.Percentage;

        /// <summary>
        /// Multiplier on the amount of <see cref="resource"/> it costs to
        /// replenish uses of the tool.
        /// </summary>
        public float replenishMult = 1f;

        /// <summary>
        /// Name of the animation clip Hornet plays when using this tool.
        /// </summary>
        public string clip = "Charge Up";

        // For some reason inheritdoc wasn't working here - TODO investigate that
        /// <summary>
        /// Reference to the object created to handle the tool's appearance and save data.
        /// This will always return a value during gameplay, but may be null during game
        /// start up, and is destroyed and recreated when the player quits to the menu.
        /// </summary>
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

        /// <summary>
        /// Set of sprites used to display the tool in the inventory and HUD when
        /// it has some uses remaining.
        /// </summary>
        public StateSprites? FullSprites;

        /// <summary>
        /// Set of sprites used to display the tool in the inventory and HUD when
        /// it has zero uses remaining.
        /// </summary>
        public StateSprites? EmptySprites;

        /// <summary>
        /// A function which runs right before Hornet plays the animation for using this tool.
        /// </summary>
        public Action beforeAnim
        {
            get { return NeedleforgePlugin.toolEventHooks[$"{name} BEFORE ANIM"]; }
            set { NeedleforgePlugin.toolEventHooks[$"{name} BEFORE ANIM"] = value; }
        }

        /// <summary>
        /// A function which runs right after Hornet plays the animation for using this tool.
        /// </summary>
        public Action afterAnim
        {
            get { return NeedleforgePlugin.toolEventHooks[$"{name} AFTER ANIM"]; }
            set { NeedleforgePlugin.toolEventHooks[$"{name} AFTER ANIM"] = value; }
        }
    }

    /// <summary>
    /// A set of sprites for displaying a tool in the inventory and HUD.
    /// </summary>
    public class StateSprites
    {
        /// <summary>
        /// Regular inventory sprite for the tool.
        /// </summary>
        public Sprite? InventorySprite;

        /// <summary>
        /// Regular HUD sprite for the tool.
        /// </summary>
        public Sprite? HudSprite;

        /// <summary>
        /// Poison variant of the inventory sprite for the tool.
        /// </summary>
        public Sprite? PoisonInventorySprite;

        /// <summary>
        /// Poison variant of the HUD sprite for the tool.
        /// </summary>
        public Sprite? PoisonHudSprite;
    }
}