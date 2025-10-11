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
            DisplayNewIndicator = false,
        };

        public static ToolCrest CreateCrest(Sprite RealSprite, Sprite Silhouette, string name)
        {
            List<ToolCrest> crests = ToolItemManager.GetAllCrests();
            ToolCrest hunter = crests[0];

            ToolCrest newCrest = new();

            newCrest.name = name;
            newCrest.crestGlow = hunter.crestGlow;
            newCrest.crestSilhouette = Silhouette;
            newCrest.crestSprite = RealSprite;

            newCrest.displayName = new() { Key = $"{name}CRESTNAME", Sheet = $"{name}" };
            newCrest.description = new() { Key = $"{name}CRESTDESC", Sheet = $"{name}" };

            newCrest.slots = [];

            newCrest.heroConfig = hunter.heroConfig;
            newCrest.SaveData = defaultSave;

            ToolItemManager.Instance.crestList.Add(newCrest);

            return newCrest;
        }

        public static ToolCrest CreateCrest(string name)
        {
            List<ToolCrest> crests = ToolItemManager.GetAllCrests();
            ToolCrest hunter = crests[0];

            return CreateCrest(hunter.crestSprite, hunter.crestSilhouette, name);
        }

        public static ToolCrest CreateCrest()
        {
            List<ToolCrest> crests = ToolItemManager.GetAllCrests();

            return CreateCrest($"NewCrest{crests.Count}");
        }
    }
}
