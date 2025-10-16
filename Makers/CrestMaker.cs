using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Needleforge.Makers
{
    public class CrestMaker
    {
        public static ToolCrestsData.Data defaultSave = new()
        {
            IsUnlocked = true,
            Slots = [],
            DisplayNewIndicator = true,
        };

        public static ToolCrest CreateCrest(Sprite? RealSprite, Sprite? Silhouette, HeroControllerConfig? attackConfig, string name)
        {
            List<ToolCrest> crests = ToolItemManager.GetAllCrests();
            ToolCrest hunter = crests[0];

            ToolCrest newCrest = new();

            newCrest.name = name;
            newCrest.crestGlow = hunter.crestGlow;
            newCrest.crestSilhouette = Silhouette ?? hunter.crestSilhouette;
            newCrest.crestSprite = RealSprite ?? hunter.crestSprite;

            newCrest.displayName = new() { Key = $"{name}CRESTNAME", Sheet = $"{name}" };
            newCrest.description = new() { Key = $"{name}CRESTDESC", Sheet = $"{name}" };

            newCrest.slots = [];

            newCrest.heroConfig = attackConfig ?? hunter.heroConfig;
            newCrest.SaveData = defaultSave;

            ToolItemManager.Instance.crestList.Add(newCrest);

            NeedleforgePlugin.newCrests.Add(newCrest);

            return newCrest;
        }
    }
}
