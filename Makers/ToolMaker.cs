using Needleforge.Data;
using TeamCherry.Localization;
using UnityEngine;

namespace Needleforge.Makers
{
    public class ToolMaker
    {
        public static ToolItemsData.Data defaultData = new()
        {
            IsUnlocked = true,
            IsHidden = false,
            HasBeenSeen = true,
            HasBeenSelected = true,
            AmountLeft = 0,
        };

        public static ToolItemLiquidsData.Data defaultLiquidsData = new()
        {
            RefillsLeft = 20,
            SeenEmptyState = true,
            UsedExtra = false,
        };

        public static ToolItemBasic CreateBasicTool(Sprite? inventorySprite, ToolItemType type, string name, LocalisedString displayName, LocalisedString description)
        {
            ToolItem item = ToolItemManager.Instance.toolItems[62];


            ToolItemBasic newTool = new();

            newTool.name = name;

            newTool.description = description;
            newTool.displayName = displayName;

            newTool.type = type;

            newTool.baseStorageAmount = 0;

            newTool.inventorySprite = inventorySprite ?? item.GetInventorySprite(ToolItem.IconVariants.Default);
            newTool.SavedData = defaultData;
            newTool.alternateUnlockedTest = new()
            {
                TestGroups = [
                    new () {
                        Tests = [
                            new(){
                                FieldName = NeedleforgePlugin.GetToolDataByName(name).unlockedPDString,
                                Type = PlayerDataTest.TestType.Bool,
                                BoolValue = true,
                            }
                            ]
                    }
                    ]
            };

            AddCustomTool(newTool);

            return newTool;
        }

        public static void AddCustomTool(ToolItem toolItem)
        {
            ToolItemManager.Instance.toolItems.Add(toolItem);

            NeedleforgePlugin.newTools.Add(toolItem);
        }

        //TODO: CreateLiquidTool
        public static ToolItemStatesLiquid CreateLiquidTool(string name, int storageAmount, int maxRefills, Color fluidColor, string infiniteRefillsPD, 
            ToolItem.ReplenishResources resource, ToolItem.ReplenishUsages replenishUsage, float replenishMult, 
            StateSprites? full, StateSprites? empty, LocalisedString displayName, LocalisedString description)
        {
            ToolItemStatesLiquid fleaBrew = (ToolItemStatesLiquid)ToolItemManager.Instance.toolItems[26];

            ToolItemStatesLiquid newLiquidTool = new();

            newLiquidTool.name = name;
            newLiquidTool.liquidColor = fluidColor;
            newLiquidTool.refillsMax = maxRefills;
            newLiquidTool.baseStorageAmount = storageAmount;
            newLiquidTool.infiniteRefillsBool = infiniteRefillsPD;

            newLiquidTool.hasUsableEmptyState = false;

            newLiquidTool.fullState = new()
            {
                DisplayName = displayName,
                Description = description,
                HudSprite = full != null ? full.HudSprite : fleaBrew.fullState.HudSprite,
                InventorySprite = full != null ? full.InventorySprite : fleaBrew.fullState.InventorySprite,
                HudSpritePoison = full != null ? full.PoisonHudSprite : fleaBrew.fullState.HudSpritePoison,
                InventorySpritePoison = full != null ? full.PoisonInventorySprite : fleaBrew.fullState.InventorySpritePoison,
                Usage = new()
                {
                    FsmEventName = name,
                    IsNonBlockingEvent = false,
                }
            };
            newLiquidTool.emptyState = new()
            {
                DisplayName = displayName,
                Description = description,
                HudSprite = empty != null ? empty.HudSprite : fleaBrew.emptyState.HudSprite,
                InventorySprite = empty != null ? empty.InventorySprite : fleaBrew.emptyState.InventorySprite,
                HudSpritePoison = empty != null ? empty.PoisonHudSprite : fleaBrew.emptyState.HudSpritePoison,
                InventorySpritePoison = empty != null ? empty.PoisonInventorySprite : fleaBrew.emptyState.InventorySpritePoison,
                Usage = new()
                {
                    FsmEventName = name,
                    IsNonBlockingEvent = false,
                }
            };

            newLiquidTool.replenishResource = resource;
            newLiquidTool.replenishUsage = replenishUsage;
            newLiquidTool.replenishUsageMultiplier = replenishMult;

            newLiquidTool.bottleCost = 10;
            newLiquidTool.delayBottleBreak = false;

            newLiquidTool.refillEffectHero = fleaBrew.refillEffectHero;
            newLiquidTool.extraDescriptionSection = fleaBrew.extraDescriptionSection;

            newLiquidTool.SavedData = defaultData;
            newLiquidTool.LiquidSavedData = defaultLiquidsData;

            AddCustomTool(newLiquidTool);

            return newLiquidTool;
        }
    }
}
