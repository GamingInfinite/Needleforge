using Needleforge.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Needleforge.Makers
{
    internal class CrestMaker
    {
        internal static ToolCrestsData.Data CreateDefaultSaveData() =>
            new()
            {
                IsUnlocked = true,
                Slots = [],
                DisplayNewIndicator = true,
            };

        internal static ToolCrest CreateCrest(CrestData crestData)
        {
            List<ToolCrest> crests = ToolItemManager.GetAllCrests();
            ToolCrest hunter = crests[0];

            ToolCrest newCrest = ScriptableObject.CreateInstance<ToolCrest>();

            newCrest.name = crestData.name;
            newCrest.crestGlow = crestData.CrestGlow ?? hunter.crestGlow;
            newCrest.crestSilhouette = crestData.Silhouette ?? hunter.crestSilhouette;
            newCrest.crestSprite = crestData.RealSprite ?? hunter.crestSprite;

            newCrest.displayName = crestData.displayName;
            newCrest.description = crestData.description;

            newCrest.slots = [.. crestData.slots];

            newCrest.heroConfig = crestData.AttackConfig ?? hunter.heroConfig;

            ToolItemManager.Instance.crestList.Add(newCrest);

            NeedleforgePlugin.newCrests.Add(newCrest);

            return newCrest;
        }
    }
}
