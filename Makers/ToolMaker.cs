using System;
using System.Collections.Generic;
using System.Text;
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

        //TODO: Fix and Test
        public static ToolItemBasic CreateBasicTool(Sprite? inventorySprite, ToolItemType type, string name)
        {
            ToolItem item = ToolItemManager.Instance.toolItems[62];


            ToolItemBasic newTool = new();

            newTool.name = name;

            newTool.description = new() { Key = $"{name}TOOLDESC", Sheet = $"{name}TOOL" };
            newTool.displayName = new() { Key = $"{name}TOOLNAME", Sheet = $"{name}TOOL" };

            newTool.type = type;

            newTool.baseStorageAmount = 0;

            newTool.inventorySprite = inventorySprite ?? item.GetInventorySprite(ToolItem.IconVariants.Default);
            newTool.SavedData = defaultData;


            ToolItemManager.Instance.toolItems.Add(newTool);

            NeedleforgePlugin.newTools.Add(newTool);

            return newTool;
        }

        //TODO: CreateLiquidTool
    }
}
